using Microsoft.Win32;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Threading;

namespace PkmnAdvanceTranslation
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IOService _ioService;
        private ObservableCollection<TranslationItemViewModel> _translationLines;
        private ObservableCollection<String> _groups;
        private ICollectionView _translationLinesView;
        private FileInfo _translationFile;

        private Boolean _groupItems;
        private String _groupFilter;
        private String _addressFilter;
        private String _contentFilter;
        private Nullable<Boolean> _translatedFilter;
        private Nullable<Boolean> _unsavedFilter;

        private Dispatcher dispatcher;
        private DateTime filterStart;
        private Timer filterDelayTimer;

        public MainWindowViewModel(IOService ioService)
        {
            _ioService = ioService;

            filterDelayTimer = new Timer();
            filterDelayTimer.Interval = 50;
            filterDelayTimer.AutoReset = true;
            filterDelayTimer.Elapsed += new ElapsedEventHandler(filterDelayTimer_Elapsed);

            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollection<String> Groups
        {
            get
            {
                if(_groups == null)
                {
                    _groups = new ObservableCollection<String>();
                }
                return _groups;
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
            get { return TranslationLinesView.CurrentItem as TranslationItemViewModel; }
            set
            {
                TranslationLinesView.MoveCurrentTo(value);
                OnPropertyChanged("CurrentTranslationItem");
            }
        }

        private Boolean MatchesFilter(TranslationItemViewModel translationLine)
        {
            if (translationLine == null)
                return false;
            return (!TranslatedFilter.HasValue || translationLine.IsTranslated == TranslatedFilter.Value)
                && (!UnsavedFilter.HasValue || translationLine.HasUnsavedChanges == UnsavedFilter.Value)
                && (String.IsNullOrWhiteSpace(GroupFilter) || translationLine.Group == GroupFilter)
                && (String.IsNullOrWhiteSpace(AddressFilter) || translationLine.Address.StartsWith(AddressFilter, StringComparison.InvariantCultureIgnoreCase))
                && (String.IsNullOrWhiteSpace(ContentFilter) || translationLine.SingleLineText.IndexOf(ContentFilter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                ;
        }

        internal void SaveEditedLines()
        {
            foreach (var editedLine in TranslationLines.Where(l => l.HasUnsavedChanges))
            {
                editedLine.SaveMultiLineTextCommand.Execute(null);
            }
        }

        internal void DiscardEditedLines()
        {
            foreach(var editedLine in TranslationLines.Where(l => l.HasUnsavedChanges))
            {
                editedLine.RestoreMultiLineTextCommand.Execute(null);
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
                TranslationLinesView.Refresh();
            }
        }

        public String GroupFilter
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

        public RelayCommand ClearFilterCommand
        {
            get
            {
                return new RelayCommand(param => ClearFilter(), param => CanClearFilter());
            }
        }

        private void ClearFilter()
        {
            _translatedFilter = null;
            _unsavedFilter = null;
            _groupFilter = _addressFilter = _contentFilter = null;
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
                return new RelayCommand(param => OpentranslationFile());
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
            foreach(var line in PointerText.ReadPointersFromFile(translationSourceFile))
            {
                if (!Groups.Contains(line.Group))
                    Groups.Add(line.Group);
                TranslationLines.Add(new TranslationItemViewModel(line));
            }
        }

        public RelayCommand SaveTranslationFileCommand
        {
            get
            {
                return new RelayCommand(param => SavetranslationFile(), param => CanSaveTranslationFile() );
            }
        }

        public RelayCommand SaveTranslationFileAsCommand
        {
            get
            {
                return new RelayCommand(param => SavetranslationFileAs(), param => CanSaveTranslationFile() );
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
            using (var writer = new StreamWriter(outputFile.Open(FileMode.Create), Encoding.GetEncoding(1252)))
            {
                foreach (var line in TranslationLines.OrderBy(l => l.Address))
                {
                    writer.WriteLine(line.PointerText);
                }
            }
        }
    }
}
