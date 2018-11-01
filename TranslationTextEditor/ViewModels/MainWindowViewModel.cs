using GalaSoft.MvvmLight.CommandWpf;
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
    public class MainWindowViewModel : ViewModelBase
    {
        private IOService _ioService;

        private TranslationItemViewModel _currentTranslationLineItem;
        private ContainsViewModel _currentContainsMode;

        private FileInfo _translationFile;

        private Boolean _groupItems;


        private String _addressFilter;
        private String _contentFilter;
        private String[] _contentFilters;
        private Nullable<Boolean> _translatedFilter;
        private Nullable<Boolean> _unsavedFilter;

        private Dispatcher dispatcher;
        private DateTime filterStart;
        private Timer filterDelayTimer;
        private Timer autoSaveTimer;

        public MainWindowViewModel(IOService ioService)
        {
            Title = "Pokémon translation editor";
            _ioService = ioService;

            LoadContainsModes();
            Groups = new ObservableCollection<String>();
            GroupsView = CollectionViewSource.GetDefaultView(Groups);
            GroupsView.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            SelectedGroups = new ObservableCollection<String>();
            SelectedGroups.CollectionChanged += SelectedGroups_CollectionChanged;
            TranslationLines = new ObservableCollection<TranslationItemViewModel>();
            TranslationLinesView = CollectionViewSource.GetDefaultView(TranslationLines);
            TranslationLinesView.Filter = (e => MatchesFilter(e as TranslationItemViewModel));
            SelectedTranslationLines = new ObservableCollection<TranslationItemViewModel>();


            filterDelayTimer = new Timer();
            filterDelayTimer.Interval = 50;
            filterDelayTimer.AutoReset = true;
            filterDelayTimer.Elapsed += FilterDelayTimer_Elapsed;

            autoSaveTimer = new Timer();
            autoSaveTimer.Interval = 1 * 60 * 1000;
            autoSaveTimer.AutoReset = true;
            autoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;

            dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void LoadContainsModes()
        {
            ContainsModes = new List<ContainsViewModel>();
            foreach(var value in Enum.GetValues(typeof(ContainFilterMode)))
            {
                var enumValue = (ContainFilterMode)value;
                ContainsModes.Add(new ContainsViewModel(enumValue.ToString(), enumValue));
            }
            _currentContainsMode = ContainsModes[0];
        }

        public ObservableCollection<String> Groups { get; private set; }
        public ICollectionView GroupsView { get; private set; }
        public ObservableCollection<String> SelectedGroups { get; private set; }
        public List<ContainsViewModel> ContainsModes { get; private set; }
        public ObservableCollection<TranslationItemViewModel> TranslationLines { get; private set; }
        public ICollectionView TranslationLinesView { get; private set; }
        public ObservableCollection<TranslationItemViewModel> SelectedTranslationLines { get; private set; }

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

        private Boolean MatchesFilter(TranslationItemViewModel translationLine)
        {
            if (translationLine == null)
                return false;
            return (!TranslatedFilter.HasValue || translationLine.IsTranslated == TranslatedFilter.Value)
                && (!EditingFilter.HasValue || translationLine.HasChangesInEditor == EditingFilter.Value)                
                && (String.IsNullOrWhiteSpace(AddressFilter) || translationLine.Address.StartsWith(AddressFilter, StringComparison.InvariantCultureIgnoreCase))
                && (SelectedGroups.Count == 0 || SelectedGroups.Contains(translationLine.Group))
                && MatchesContentFilter(translationLine)
                ;
        }

        private void SelectedGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TranslationLinesView.Refresh();
        }

        private Boolean MatchesContentFilter(TranslationItemViewModel translationLine)
        {
            if (String.IsNullOrWhiteSpace(ContentFilter))
                return true;
            switch(CurrentContainsMode.FilterMode)
            {
                case ContainFilterMode.Both:
                    foreach(var filter in _contentFilters)
                    {
                        if (translationLine.TranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            && translationLine.UnTranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                case ContainFilterMode.Translated:
                    foreach (var filter in _contentFilters)
                    {
                        if (translationLine.TranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                case ContainFilterMode.Untranslated:
                    foreach (var filter in _contentFilters)
                    {
                        if (translationLine.UnTranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                default:
                    return true;
            }
        }

        public Nullable<Boolean> TranslatedFilter
        {
            get
            {
                return _translatedFilter;
            }
            set
            {
                if (value == _translatedFilter)
                    return;
                _translatedFilter = value;
                OnPropertyChanged("TranslatedFilter");
                TranslationLinesView.Refresh();
            }
        }

        public Nullable<Boolean> EditingFilter
        {
            get
            {
                return _unsavedFilter;
            }
            set
            {
                if (value == _unsavedFilter)
                    return;
                _unsavedFilter = value;
                OnPropertyChanged("EditingFilter");
                TranslationLinesView.Refresh();
            }
        }

        public String AddressFilter
        {
            get { return _addressFilter; }
            set
            {
                if (value == _addressFilter)
                    return;

                _addressFilter = value;
                OnPropertyChanged("AddressFilter");
                FilterChangedAsync();
            }
        }

        public ContainsViewModel CurrentContainsMode
        {
            get { return _currentContainsMode;  }
            set
            {
                if (value == _currentContainsMode)
                    return;

                _currentContainsMode = value;
                OnPropertyChanged("CurrentContainsMode");
                TranslationLinesView.Refresh();
            }
        }

        public String ContentFilter
        {
            get { return _contentFilter; }
            set
            {
                if (value == _contentFilter)
                    return;

                _contentFilter = value;                
                OnPropertyChanged("ContentFilter");
                FilterChangedAsync();
            }
        }

        private void FilterChangedAsync()
        {
            filterStart = DateTime.Now;

            if (!filterDelayTimer.Enabled)
            {
                filterDelayTimer.Enabled = true;
                filterDelayTimer.Start();
            }
        }

        private void FilterDelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - filterStart).TotalSeconds > .3) //We only apply the typed filter 0.3 seconds after the user stopped typing.
            {
                dispatcher.BeginInvoke((Action)delegate ()
                {
                    filterDelayTimer.Stop();
                    _contentFilters = ContentFilter.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    TranslationLinesView.Refresh();
                });
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
            var vm = new ChangeGroupViewModel(Groups);
            DialogViewModel = vm;
            DialogViewModel.ShowDialog = true;
            if(vm.Confirmed)
            {
                if (SelectedGroups.Count > 0 && !SelectedGroups.Contains(vm.SelectedGroup))
                    SelectedGroups.Add(vm.SelectedGroup);
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

        public RelayCommand ClearFilterCommand
        {
            get
            {
                return new RelayCommand(ClearFilter, CanClearFilter);
            }
        }

        private void ClearFilter()
        {
            _translatedFilter = null;
            OnPropertyChanged("TranslatedFilter");
            _unsavedFilter = null;
            OnPropertyChanged("EditingFilter");
            SelectedGroups.Clear();
            _addressFilter = null;
            OnPropertyChanged("AddressFilter");
            _contentFilter = null;
            OnPropertyChanged("ContentFilter");
            _currentContainsMode = ContainsModes[0];
            OnPropertyChanged("CurrentContainsMode");
            TranslationLinesView.Refresh();
        }

        private bool CanClearFilter()
        {
            return TranslatedFilter.HasValue 
                || EditingFilter.HasValue 
                || SelectedGroups.Count > 0 
                || !String.IsNullOrWhiteSpace(AddressFilter) 
                || !String.IsNullOrWhiteSpace(ContentFilter)
                || _currentContainsMode != ContainsModes[0];
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
            Groups.Clear();
            SelectedGroups.Clear();
            foreach (var line in PointerText.ReadPointersFromFile(translationSourceFile))
            {
                if (!Groups.Contains(line.Group))
                    Groups.Add(line.Group);
                TranslationLines.Add(new TranslationItemViewModel(line));
            }
            autoSaveTimer.Start();
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

        private void AutoSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            AutoSavetranslationFile();
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
        }
    }
}
