using GalaSoft.MvvmLight.CommandWpf;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class TranslationItemViewModel : ViewModelBase
    {
        public PointerText PointerText { get; private set; }
        private ILineLengthService LineLengthService { get; set; }
        private IDialogService DialogService { get; set; }
        private String editedMultiLineText;
        private Boolean _hasChangesInMemory;
        private Boolean _hasChangesInEditor;
        private Int32 _untranslatedLineLength;
        private Int32 _translatedLineLength;

        public TranslationItemViewModel(PointerText pointerText, ILineLengthService lineLengthService, IDialogService dialogService)
        {
            PointerText = pointerText;
            LineLengthService = lineLengthService;
            DialogService = dialogService;
            CalculateInitialLineLengths();
        }

        public Boolean HasChangesInMemory
        {
            get
            {
                return _hasChangesInMemory;
            }
            set
            {
                if (value == _hasChangesInMemory)
                    return;

                _hasChangesInMemory = value;
                OnPropertyChanged("HasChangesInMemory");
            }
        }

        public Boolean HasChangesInEditor
        {
            get
            {
                return _hasChangesInEditor;
            }
            set
            {
                if (value == _hasChangesInEditor)
                    return;
                _hasChangesInEditor = value;
                OnPropertyChanged("HasChangesInEditor");
            }
        }

        public Int32 UntranslatedLineLength
        {
            get
            {
                return _untranslatedLineLength;
            }
            set
            {
                if (value == _untranslatedLineLength)
                    return;
                _untranslatedLineLength = value;
                OnPropertyChanged("UntranslatedLineLength");
                OnPropertyChanged("LineLengthDifference");
            }
        }

        public Int32 TranslatedLineLength
        {
            get
            {
                return _translatedLineLength;
            }
            set
            {
                if (value == _translatedLineLength)
                    return;
                _translatedLineLength = value;
                OnPropertyChanged("TranslatedLineLength");
                OnPropertyChanged("LineLengthDifference");
            }
        }

        public Int32 LineLengthDifference
        {
            get
            {
                return TranslatedLineLength - UntranslatedLineLength;
            }
        }

        public String Address
        {
            get
            {
                return PointerText.Address.ToString("X6");
            }
        }

        public Int32 ReferenceCount
        {
            get
            {
                return PointerText.ReferenceCount;
            }
        }

        public List<ReferenceViewModel> References
        {
            get
            {
                return PointerText.References.Select(r => new ReferenceViewModel(r)).ToList();
            }
        }

        public Int32 AvailableLength
        {
            get
            {
                return PointerText.AvailableLength;
            }
        }

        public Int32 RemainingLength
        {
            get
            {
                return PointerText.RemainingLength;
            }
        }

        public Boolean IsTranslated
        {
            get
            {
                return PointerText.IsTranslated;
            }
        }

        public String Group
        {
            get
            {
                return PointerText.Group;
            }
            set
            {
                PointerText.Group = value;
                HasChangesInMemory = true;
                OnPropertyChanged("Group");
            }
        }

        public String TranslatedSingleLine
        {
            get
            {
                return PointerText.TranslatedSingleLine;
            }
            set
            {
                PointerText.TranslatedSingleLine = value;
                OnPropertyChanged("TranslatedSingleLine");
                OnPropertyChanged("RemainingLength");
                OnPropertyChanged("IsTranslated");
            }
        }

        public String TranslatedMultiLine
        {
            get
            {
                if (editedMultiLineText == null)
                    return TextHandler.FormatEditString(TranslatedSingleLine);
                return editedMultiLineText;
            }
            set
            {
                editedMultiLineText = value;
                OnPropertyChanged("TranslatedMultiLine");
                HasChangesInEditor = value != null;
            }
        }

        public String UnTranslatedSingleLine
        {
            get
            {
                return PointerText.UntranslatedSingleLine;
            }
        }

        public String UnTranslatedMultiLine
        {
            get
            {
                return TextHandler.FormatEditString(PointerText.UntranslatedSingleLine);
            }
        }

        public Boolean IsSpecialDialog
        {
            get
            {
                return PointerText.TextMode == TextMode.Intro;
            }
            set
            {
                if (value)
                    PointerText.TextMode = TextMode.Intro;
                else
                    PointerText.TextMode = TextMode.Dialog;
                OnPropertyChanged("IsSpecialDialog");                
                HasChangesInEditor = true;
            }
        }

        public RelayCommand EditReferencesCommand
        {
            get
            {
                return new RelayCommand(EditReferences);
            }
        }

        private void EditReferences()
        {
            var dialog = new ReferencesDialogViewModel();
            foreach (var reference in References)
                dialog.References.Add(reference);
            DialogService.DialogViewModel = dialog;
            dialog.ShowDialog = true;
        }

        public RelayCommand CopyUntranslatedToTranslatedCommand
        {
            get
            {
                return new RelayCommand(CopyUntranslatedToTranslated);
            }
        }

        private void CopyUntranslatedToTranslated()
        {
            TranslatedMultiLine = UnTranslatedMultiLine;
        }

        public RelayCommand RestoreMultiLineTextCommand
        {
            get
            {
                return new RelayCommand(RestoreMultiLineText, CanRestoreMultiLineText);
            }
        }

        private Boolean CanRestoreMultiLineText()
        {
            return editedMultiLineText != null;
        }

        private void RestoreMultiLineText()
        {
            TranslatedMultiLine = null;
        }

        public RelayCommand SaveMultiLineTextCommand
        {
            get
            {
                return new RelayCommand(SaveMultiLineText, CanSaveMultiLineText);
            }
        }

        private Boolean CanSaveMultiLineText()
        {
            return HasChangesInEditor;
        }

        private void SaveMultiLineText()
        {
            if (editedMultiLineText == null) //BLB we're re-saving (textmode change)
                editedMultiLineText = TranslatedMultiLine;
            TranslatedLineLength = GetMaxLineLength(editedMultiLineText);
            TranslatedSingleLine = TextHandler.EditStringToSingleLine(editedMultiLineText, IsSpecialDialog);
            TranslatedMultiLine = null;
            HasChangesInMemory = true;
        }

        private void CalculateInitialLineLengths()
        {
            _untranslatedLineLength = GetMaxLineLength(UnTranslatedMultiLine);
            _translatedLineLength = GetMaxLineLength(TranslatedMultiLine);
        }

        private Int32 GetMaxLineLength(String multiLine)
        {
            double maxLength = 0;
            var lines = multiLine.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var length = LineLengthService.MeasureStringWidth(line);
                if (length > maxLength)
                    maxLength = length;
            }
            return (Int32)Math.Ceiling(maxLength);
        }
    }
}
