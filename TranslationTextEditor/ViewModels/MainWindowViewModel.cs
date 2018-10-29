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
using System.Windows.Data;

namespace PkmnAdvanceTranslation
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IOService _ioService;
        private ObservableCollection<TranslationItemViewModel> _translationLines;
        private ICollectionView _translationLinesView;
        private FileInfo translationFile;

        public MainWindowViewModel(IOService ioService)
        {
            _ioService = ioService;
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
                    if (_translationLinesView != null && _translationLinesView.CanGroup == true)
                    {
                        _translationLinesView.GroupDescriptions.Clear();
                        _translationLinesView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
                    }
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
            return true;

            //TODO: Later we apply filters here.
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
                translationFile = new FileInfo(fileName);
                LoadtranslationFile(translationFile);
            }
        }

        private void LoadtranslationFile(FileInfo translationSourceFile)
        {
            TranslationLines.Clear();
            foreach(var line in PointerText.ReadPointersFromFile(translationSourceFile))
            {
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

        private void SavetranslationFileAs()
        {
            var proposedName = String.Format("Translation_{0:yyyy-MM-dd_HH-mm}.txt", DateTime.Now);
            var newFileName = _ioService.SaveFileDialog(null, proposedName, "Select where to save the translation file.", "*.txt");
            if(!String.IsNullOrWhiteSpace(newFileName))
            {
                var newFile = new FileInfo(newFileName);
                WriteTranslationFile(newFile);
                if (newFile.Exists)
                    translationFile = newFile;
            }
        }

        private void SavetranslationFile()
        {
            WriteTranslationFile(translationFile);
        }

        private bool CanSaveTranslationFile()
        {
            return translationFile != null;
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
