﻿using Microsoft.Win32;
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
        protected TextHandler _textHandler;

        public MainWindowViewModel(IOService ioService, TextHandler textHandler)
        {
            _ioService = ioService;
            _textHandler = textHandler;
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
                LoadtranslationFile(new FileInfo(fileName));
            }
        }

        private void LoadtranslationFile(FileInfo translationSourceFile)
        {
            TranslationLines.Clear();
            using (var sourceReader = new StreamReader(translationSourceFile.OpenRead(), Encoding.GetEncoding(1252)))
            {
                var sourceLine = sourceReader.ReadLine();
                while (sourceLine != null)
                {
                    if (sourceLine.Length > 5 && PointerText.HexChars.Contains(sourceLine[0]))
                    {
                        TranslationLines.Add(new TranslationItemViewModel(sourceLine, _textHandler));
                    }
                    sourceLine = sourceReader.ReadLine();
                }
            }
        }
    }
}
