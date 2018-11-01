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
        private String editedMultiLineText;
        private Boolean _hasChangesInMemory;

        public TranslationItemViewModel(PointerText pointerText)
        {
            PointerText = pointerText;
        }

        public Boolean HasChangesInMemory
        {
            get
            {
                return _hasChangesInMemory;
            }
            set
            {
                _hasChangesInMemory = value;
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
                OnPropertyChanged("HasChangesInEditor");
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
                return PointerText.TextMode == TextMode.Into;
            }
            set
            {
                if (value)
                    PointerText.TextMode = TextMode.Into;
                else
                    PointerText.TextMode = TextMode.Dialog;
                OnPropertyChanged("IsSpecialDialog");
            }
        }

        public Boolean HasChangesInEditor
        {
            get
            {
                return editedMultiLineText != null;
            }
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
            return HasChangesInEditor;
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
            TranslatedSingleLine = TextHandler.EditStringToSingleLine(editedMultiLineText, IsSpecialDialog);
            TranslatedMultiLine = null;
            HasChangesInMemory = true;
        }
    }
}
