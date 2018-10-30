using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<TranslationItemViewModel> _translationLines;        
        private ObservableCollection<TranslationItemViewModel> _selectedTranslationLines;
        private TranslationItemViewModel _currentTranslationLineItem;

        private ObservableCollection<GroupViewModel> _groups;
        private ICollectionView _translationLinesView;

        private List<ContainsViewModel> _containsModes;
        private ContainsViewModel _currentContainsMode;

        private FileInfo _translationFile;

        private Boolean _groupItems;
        private GroupViewModel _groupFilter;
        private String _addressFilter;
        private String _contentFilter;
        private Nullable<Boolean> _translatedFilter;
        private Nullable<Boolean> _unsavedFilter;

        private Dispatcher dispatcher;
        private DateTime filterStart;
        private Timer filterDelayTimer;

        public MainWindowViewModel(IOService ioService)
        {
            Title = "Pokémon translation editor";
            _ioService = ioService;

            LoadContainsModes();

            filterDelayTimer = new Timer();
            filterDelayTimer.Interval = 50;
            filterDelayTimer.AutoReset = true;
            filterDelayTimer.Elapsed += new ElapsedEventHandler(filterDelayTimer_Elapsed);

            dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void LoadContainsModes()
        {
            _containsModes = new List<ContainsViewModel>();
            foreach(var value in Enum.GetValues(typeof(ContainFilterMode)))
            {
                var enumValue = (ContainFilterMode)value;
                _containsModes.Add(new ContainsViewModel(enumValue.ToString(), enumValue));
            }
            _currentContainsMode = _containsModes[0];
        }

        public ObservableCollection<GroupViewModel> Groups
        {
            get
            {
                if(_groups == null)
                {
                    _groups = new ObservableCollection<GroupViewModel>();
                }
                return _groups;
            }
        }

        public List<ContainsViewModel> ContainsModes
        {
            get
            {
                return _containsModes;
            }
        }


        public ObservableCollection<TranslationItemViewModel> TranslationLines
        {
            get
            {
                if(_translationLines == null)
                {
                    _translationLines = new ObservableCollection<TranslationItemViewModel>();
                }
                return _translationLines;
            }
        }

        public virtual ICollectionView TranslationLinesView
        {
            get
            {
                if (_translationLinesView == null)
                {
                    _translationLinesView = CollectionViewSource.GetDefaultView(TranslationLines);
                    _translationLinesView.Filter = (e => MatchesFilter(e as TranslationItemViewModel));
                }
                return _translationLinesView;
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

        public ObservableCollection<TranslationItemViewModel> SelectedTranslationLines
        {
            get
            {
                if (_selectedTranslationLines == null)
                    _selectedTranslationLines = new ObservableCollection<TranslationItemViewModel>();
                return _selectedTranslationLines;
            }
        }

        private Boolean MatchesFilter(TranslationItemViewModel translationLine)
        {
            if (translationLine == null)
                return false;
            return (!TranslatedFilter.HasValue || translationLine.IsTranslated == TranslatedFilter.Value)
                && (!UnsavedFilter.HasValue || translationLine.HasUnsavedChanges == UnsavedFilter.Value)
                && (GroupFilter == null || String.IsNullOrWhiteSpace(GroupFilter.Value) || translationLine.Group == GroupFilter.Value)
                && (String.IsNullOrWhiteSpace(AddressFilter) || translationLine.Address.StartsWith(AddressFilter, StringComparison.InvariantCultureIgnoreCase))
                && MatchesContentFilter(translationLine)
                ;
        }

        private Boolean MatchesContentFilter(TranslationItemViewModel translationLine)
        {
            if (String.IsNullOrWhiteSpace(ContentFilter))
                return true;
            switch(CurrentContainsMode.FilterMode)
            {
                case ContainFilterMode.Both:
                    return translationLine.TranslatedSingleLine.IndexOf(ContentFilter, StringComparison.InvariantCultureIgnoreCase) > 0 
                        && translationLine.UnTranslatedSingleLine.IndexOf(ContentFilter, StringComparison.InvariantCultureIgnoreCase) > 0;
                case ContainFilterMode.Translated:
                    return translationLine.TranslatedSingleLine.IndexOf(ContentFilter, StringComparison.InvariantCultureIgnoreCase) > 0;
                case ContainFilterMode.Untranslated:
                    return translationLine.UnTranslatedSingleLine.IndexOf(ContentFilter, StringComparison.InvariantCultureIgnoreCase) > 0;
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

        public Nullable<Boolean> UnsavedFilter
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
                OnPropertyChanged("UnsavedFilter");
                TranslationLinesView.Refresh();
            }
        }

        public GroupViewModel GroupFilter
        {
            get
            {
                return _groupFilter;
            }
            set
            {
                if (value == _groupFilter)
                    return;

                _groupFilter = value;
                OnPropertyChanged("GroupFilter");
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

        private void filterDelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - filterStart).TotalSeconds > .3)
            {
                dispatcher.BeginInvoke((Action)delegate ()
                {
                    filterDelayTimer.Stop();
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
                return new RelayCommand(ChangeLinesGroup, CanChangeLinesGroup);
            }
        }

        private void ChangeLinesGroup()
        {
            var vm = new ChangeGroupViewModel();
            vm.Groups = Groups;
            DialogViewModel = vm;
            DialogViewModel.ShowDialog = true;
            if(vm.Confirmed)
            {
                foreach(var line in SelectedTranslationLines)
                {
                    line.Group = vm.SelectedGroup;
                }
                if (GroupItems)
                {
                    TranslationLinesView.SortDescriptions.Clear();
                    TranslationLinesView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
                    TranslationLinesView.Refresh();
                }                
            }
        }

        private bool CanChangeLinesGroup()
        {
            return SelectedTranslationLines.Count > 0;
        }

        public RelayCommand CopyUntranslatedToTranslatedLinesCommand
        {
            get
            {
                return new RelayCommand(CopyUntranslatedToTranslatedLines, CanCopyUntranslatedToTranslatedLines);
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

        private bool CanCopyUntranslatedToTranslatedLines()
        {
            return SelectedTranslationLines.Count > 0;
        }

        public RelayCommand DeleteSelectedLinesCommand
        {
            get
            {
                return new RelayCommand(DeleteSelectedLines, CanDeleteSelectedLines);
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

        private bool CanDeleteSelectedLines()
        {
            return SelectedTranslationLines.Count > 0;
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
            foreach (var editedLine in TranslationLines.Where(l => l.HasUnsavedChanges))
            {
                editedLine.SaveMultiLineTextCommand.Execute(null);
            }
        }

        internal Boolean CanSaveEditedLines()
        {
            var lineWithUnsavedChange = TranslationLines.FirstOrDefault(l => l.HasUnsavedChanges);
            return lineWithUnsavedChange != null;
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
            foreach (var editedLine in TranslationLines.Where(l => l.HasUnsavedChanges))
            {
                editedLine.RestoreMultiLineTextCommand.Execute(null);
            }
        }

        internal Boolean CanDiscardEditedLines()
        {
            var lineWithUnsavedChange = TranslationLines.FirstOrDefault(l => l.HasUnsavedChanges);
            return lineWithUnsavedChange != null;
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
            OnPropertyChanged("UnsavedFilter");
            _groupFilter = Groups.FirstOrDefault();
            OnPropertyChanged("GroupFilter");
            _addressFilter = null;
            OnPropertyChanged("AddressFilter");
            _contentFilter = null;
            OnPropertyChanged("ContentFilter");
            _currentContainsMode = _containsModes[0];
            OnPropertyChanged("CurrentContainsMode");
            TranslationLinesView.Refresh();
        }

        private bool CanClearFilter()
        {
            return GroupFilter != null || !String.IsNullOrWhiteSpace(AddressFilter) || !String.IsNullOrWhiteSpace(ContentFilter);
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
            Groups.Add(new GroupViewModel("<ALL>", ""));
            GroupFilter = Groups[0];
            foreach (var line in PointerText.ReadPointersFromFile(translationSourceFile))
            {
                if (!Groups.Any(g => g.Value == line.Group))
                    Groups.Add(new GroupViewModel(line.Group, line.Group));
                TranslationLines.Add(new TranslationItemViewModel(line));
            }
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
        }
    }
}
