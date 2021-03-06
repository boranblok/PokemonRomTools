﻿using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Threading;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDialogService
    {
        private IOService _ioService;
        private ILineLengthService _lineLengthService;

        private TranslationItemViewModel _currentTranslationLineItem;
        private FileInfo _translationFile;
        private Boolean _groupItems;
        private String _statusMessage;
        private String _currentLineMessage;
        private Timer autoSaveTimer;
        private Timer statusClearTimer;

        public event EventHandler NewFileLoaded;

        public MainWindowViewModel(IOService ioService, ILineLengthService lineLengthService)
        {
            Title = "Pokémon translation editor";
            _ioService = ioService;
            _lineLengthService = lineLengthService;

            Filter = new FilterViewModel();
            Filter.FilterChanged += Filter_FilterChanged;

            TranslationLines = new ObservableCollection<TranslationItemViewModel>();
            TranslationLinesView = CollectionViewSource.GetDefaultView(TranslationLines);
            TranslationLinesView.Filter = (e => Filter.MatchesFilter(e as TranslationItemViewModel));
            SelectedTranslationLines = new ObservableCollection<TranslationItemViewModel>();

            autoSaveTimer = new Timer();
            autoSaveTimer.Interval = 1 * 60 * 1000;
            autoSaveTimer.AutoReset = true;
            autoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;

            statusClearTimer = new Timer();
            statusClearTimer.Interval = 10 * 1000;
            statusClearTimer.AutoReset = false;
            statusClearTimer.Elapsed += StatusClearTimer_Elapsed;
        }

        private void AutoSaveTimer_Elapsed(object sender, ElapsedEventArgs e) => AutoSavetranslationFile();
        private void StatusClearTimer_Elapsed(object sender, ElapsedEventArgs e) => StatusMessage = String.Empty;

        private void Filter_FilterChanged(object sender, EventArgs e)
        {
            TranslationLinesView.Refresh();
            CurrentLineMessage = String.Format("{0} Lines", TranslationLinesView.Cast<TranslationItemViewModel>().Count());
        }

        private void OnNewFileLoaded() => NewFileLoaded?.Invoke(this, new EventArgs());

        public FilterViewModel Filter { get; private set; }
        
        public ObservableCollection<TranslationItemViewModel> TranslationLines { get; private set; }
        public ICollectionView TranslationLinesView { get; private set; }
        public ObservableCollection<TranslationItemViewModel> SelectedTranslationLines { get; private set; }

        public String StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged("StatusMessage");
                if (!String.IsNullOrWhiteSpace(_statusMessage))
                {
                    statusClearTimer.Stop();
                    statusClearTimer.Start();
                }
            }
        }

        public String CurrentLineMessage
        {
            get
            {
                return _currentLineMessage; // 
            }
            set
            {
                if (value == _currentLineMessage)
                    return;

                _currentLineMessage = value;
                OnPropertyChanged("CurrentLineMessage");
            }
        }

        public TranslationItemViewModel CurrentTranslationItem
        {
            get
            {
                return _currentTranslationLineItem;
            }
            set
            {
                _currentTranslationLineItem = value;
                OnPropertyChanged("CurrentTranslationItem");
                OnPropertyChanged("HasCurrentTranslationItem");
                OnPropertyChanged("HasNoCurrentTranslationItem");
            }
        }

        public Boolean HasCurrentTranslationItem
        {
            get
            {
                return CurrentTranslationItem != null;
            }
        }

        public Boolean HasNoCurrentTranslationItem
        {
            get
            {
                return !HasCurrentTranslationItem;
            }
        }

        public Boolean GroupItems
        {
            get
            {
                return _groupItems;
            }
            set
            {
                if (_groupItems && !value)
                    TranslationLinesView.GroupDescriptions.Clear();
                else if(!_groupItems && value)
                    TranslationLinesView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                _groupItems = value;

                OnPropertyChanged("GroupItems");
                OnPropertyChanged("NotGroupItems");
            }
        }

        public Boolean NotGroupItems
        {
            get
            {
                return !GroupItems;
            }
        }

        private DialogViewModelBase _DialogViewModel;
        public DialogViewModelBase DialogViewModel
        {
            get { return _DialogViewModel; }
            set
            {
                if (value == _DialogViewModel)
                    return;

                _DialogViewModel = value;
                OnPropertyChanged("DialogViewModel");
            }
        }

        public RelayCommand<IList> SelectionChangedCommand
        {
            get
            {
                return new RelayCommand<IList>(items => ChangeSelectedItems(items));
            }
        }

        private void ChangeSelectedItems(IList items)
        {
            SelectedTranslationLines.Clear();
            foreach (var item in items) SelectedTranslationLines.Add((TranslationItemViewModel)item);
            if(SelectedTranslationLines.Count == 1)
            {
                CurrentTranslationItem = SelectedTranslationLines[0];
            }
            else
            {
                CurrentTranslationItem = null;
            }
        }

        public RelayCommand ChangeLinesGroupCommand
        {
            get
            {
                return new RelayCommand(ChangeLinesGroup, IsAtLeastOneLineSelected);
            }
        }

        private void ChangeLinesGroup()
        {
            var vm = new ChangeGroupViewModel(Filter.Groups);
            DialogViewModel = vm;
            DialogViewModel.ShowDialog = true;
            if(vm.Confirmed)
            {
                if (Filter.SelectedGroups.Count > 0 && !Filter.SelectedGroups.Contains(vm.SelectedGroup))
                    Filter.SelectedGroups.Add(vm.SelectedGroup);
                foreach(var line in SelectedTranslationLines)
                {
                    line.Group = vm.SelectedGroup;
                }
                if (GroupItems)
                {
                    TranslationLinesView.SortDescriptions.Clear();
                    TranslationLinesView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));                    
                }
                TranslationLinesView.Refresh();
            }
        }

        private bool IsAtLeastOneLineSelected()
        {
            return SelectedTranslationLines.Count > 0;
        }

        public RelayCommand CopyUntranslatedToTranslatedLinesCommand
        {
            get
            {
                return new RelayCommand(CopyUntranslatedToTranslatedLines, IsAtLeastOneLineSelected);
            }
        }

        private void CopyUntranslatedToTranslatedLines()
        {
            var message = String.Format("Are you sure you want to copy {0} untranslated entries overwriting the translated lines?", SelectedTranslationLines.Count);
            var vm = new ConfirmationDialogViewModel("Are you sure?", message);
            DialogViewModel = vm;
            DialogViewModel.ShowDialog = true;
            if (vm.Confirmed)
            {
                foreach (var line in SelectedTranslationLines)
                {
                    line.CopyUntranslatedToTranslatedCommand.Execute(null);
                    line.SaveMultiLineTextCommand.Execute(null);
                }
            }           
        }

        public RelayCommand SetTextModeOnLinesCommand
        {
            get
            {
                return new RelayCommand(SetTextModeOnLines, IsAtLeastOneLineSelected);
            }
        }

        private void SetTextModeOnLines()
        {            
            var vm = new ChangeTextModeViewModel();
            DialogViewModel = vm;
            DialogViewModel.ShowDialog = true;
            if (vm.Confirmed)
            {
                foreach (var line in SelectedTranslationLines)
                {
                    line.IsSpecialDialog = vm.IsSpecialDialog;
                    line.SaveMultiLineTextCommand.Execute(null);
                }
            }
        }

        public RelayCommand TrimLinesToFitCommand
        {
            get
            {
                return new RelayCommand(TrimLinestoFit, IsAtLeastOneLineSelected);
            }
        }

        private void TrimLinestoFit()
        {
            var vm = new ConfirmationDialogViewModel("Are you sure?", "Are you sure you want to trim the selected lines?");
            DialogViewModel = vm;
            DialogViewModel.ShowDialog = true;
            if (vm.Confirmed)
            {
                foreach (var line in SelectedTranslationLines)
                {
                    if (line.RemainingLength < 0)
                        line.TranslatedMultiLine = line.TranslatedMultiLine.Substring(line.RemainingLength * -1).Trim();
                    line.SaveMultiLineTextCommand.Execute(null);
                    if (line.RemainingLength < 0)
                        line.TranslatedMultiLine = line.TranslatedMultiLine.Substring(line.RemainingLength * -1).Trim();
                    line.SaveMultiLineTextCommand.Execute(null);
                }
            }
        }

        public RelayCommand DeleteSelectedLinesCommand
        {
            get
            {
                return new RelayCommand(DeleteSelectedLines, IsAtLeastOneLineSelected);
            }
        }

        private void DeleteSelectedLines()
        {
            var message = String.Format("Are you sure you want to delete {0} entries?", SelectedTranslationLines.Count);
            var vm = new ConfirmationDialogViewModel("Are you sure?", message);
            DialogViewModel = vm;
            DialogViewModel.ShowDialog = true;
            if (vm.Confirmed)
            {
                var linesToRemove = new List<Int32>();
                foreach (var line in SelectedTranslationLines)
                {
                    linesToRemove.Add(TranslationLines.IndexOf(line));
                }
                foreach(var lineIndex in linesToRemove.OrderByDescending(i => i))
                {
                    TranslationLines.RemoveAt(lineIndex);
                    TranslationLinesView.Refresh();
                }
            }           
        }

        public RelayCommand SaveEditedLinesCommand
        {
            get
            {
                return new RelayCommand(SaveEditedLines, CanSaveEditedLines);
            }
        }


        internal void SaveEditedLines()
        {
            foreach (var editedLine in TranslationLines.Where(l => l.HasChangesInEditor))
            {
                editedLine.SaveMultiLineTextCommand.Execute(null);
            }
        }

        internal Boolean CanSaveEditedLines()
        {
            return TranslationLines.Any(l => l.HasChangesInEditor);
        }

        public RelayCommand DiscardEditedLinesCommand
        {
            get
            {
                return new RelayCommand(DiscardEditedLines, CanDiscardEditedLines);
            }
        }

        internal void DiscardEditedLines()
        {
            foreach (var editedLine in TranslationLines.Where(l => l.HasChangesInEditor))
            {
                editedLine.RestoreMultiLineTextCommand.Execute(null);
            }
        }

        internal Boolean CanDiscardEditedLines()
        {
            return TranslationLines.Any(l => l.HasChangesInEditor);
        }

        public RelayCommand SelectPreviousLineCommand
        {
            get
            {
                return new RelayCommand(SelectPreviousLine, CanSelectPreviousLine);
            }
        }

        private bool CanSelectPreviousLine()
        {
            return SelectedTranslationLines.Count == 1;
        }

        private void SelectPreviousLine()
        {
            TranslationLinesView.MoveCurrentToPrevious();
        }

        public RelayCommand SelectNextLineCommand
        {
            get
            {
                return new RelayCommand(SelectNextLine, CanSelectNextLine);
            }
        }

        private bool CanSelectNextLine()
        {
            return SelectedTranslationLines.Count == 1;
        }

        private void SelectNextLine()
        {
            TranslationLinesView.MoveCurrentToNext();
        }       

        public RelayCommand OpenTranslationFileCommand
        {
            get
            {
                return new RelayCommand(OpentranslationFile);
            }
        }

        private void OpentranslationFile()
        {
            var fileName = _ioService.OpenFileDialog(null, "Open a translation file to modify.");
            if(fileName != null && File.Exists(fileName))
            {
                TranslationFile = new FileInfo(fileName);
                LoadtranslationFile(TranslationFile);
            }
        }

        private void LoadtranslationFile(FileInfo translationSourceFile)
        {
            TranslationLines.Clear();
            Filter.Groups.Clear();
            Filter.SelectedGroups.Clear();
            foreach (var line in PointerText.ReadPointersFromFile(translationSourceFile))
            {
                if (!Filter.Groups.Contains(line.Group))
                    Filter.Groups.Add(line.Group);
                TranslationLines.Add(new TranslationItemViewModel(line, _lineLengthService, this));
            }
            autoSaveTimer.Start();
            StatusMessage = String.Format("Loaded {0}", translationSourceFile.Name);
            CurrentLineMessage = String.Format("{0} Lines", TranslationLinesView.Cast<TranslationItemViewModel>().Count());
            OnNewFileLoaded();
        }

        public RelayCommand SaveTranslationFileCommand
        {
            get
            {
                return new RelayCommand(SavetranslationFile, CanSaveTranslationFile );
            }
        }

        public RelayCommand SaveTranslationFileAsCommand
        {
            get
            {
                return new RelayCommand(SavetranslationFileAs, CanSaveTranslationFile );
            }
        }

        public FileInfo TranslationFile {
            get
            {
                return _translationFile;
            }
            set
            {
                _translationFile = value;
                Title = String.Format("Pokémon translation editor - {0}", _translationFile.Name);
                OnPropertyChanged("TranslationFile");
            }
        }

        private void SavetranslationFileAs()
        {
            var proposedName = String.Format("Translation_{0:yyyy-MM-dd_HH-mm}.txt", DateTime.Now);
            var newFileName = _ioService.SaveFileDialog(null, proposedName, "Select where to save the translation file.", "*.txt");
            if(!String.IsNullOrWhiteSpace(newFileName))
            {
                var newFile = new FileInfo(newFileName);
                WriteTranslationFile(newFile);
                if (newFile.Exists)
                    TranslationFile = newFile;
            }
        }

        

        private void AutoSavetranslationFile()
        {
            if (TranslationFile == null)
                return;

            DirectoryInfo autosaveFolder = new DirectoryInfo("autosave");
            if (!autosaveFolder.Exists)
            {
                autosaveFolder.Create();
                autosaveFolder.Refresh();
            }
            var existingFiles = autosaveFolder.GetFiles("*_autosave.txt");
            foreach(var file in existingFiles.OrderByDescending(f => f.LastWriteTime).Skip(10)) //TODO: parameter?
            {
                file.Delete();
            }
            var baseName = TranslationFile.Name.Substring(0, TranslationFile.Name.Length - TranslationFile.Extension.Length);
            var backupFileName = String.Format("{0}_{1:yyMMddHHmmss}_autosave.txt", baseName, DateTime.Now);
            var backupFileInfo = new FileInfo(Path.Combine(autosaveFolder.FullName, backupFileName));
            PointerText.WritePointersToFile(backupFileInfo, TranslationLines.Select(l => l.PointerText).OrderBy(l => l.Address));
        }

        private void SavetranslationFile()
        {
            WriteTranslationFile(TranslationFile);
        }

        private bool CanSaveTranslationFile()
        {
            return TranslationFile != null;
        }

        private void WriteTranslationFile(FileInfo outputFile)
        {
            PointerText.WritePointersToFile(outputFile, TranslationLines.Select(l => l.PointerText).OrderBy(l => l.Address));
            foreach(var line in TranslationLines)
            {
                line.HasChangesInMemory = false;
            }
            StatusMessage = String.Format("Saved translation to {0}", outputFile.Name);
        }
    }
}
