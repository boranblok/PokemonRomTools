using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    public class TranslationItemViewModel : ViewModelBase
    {
        public PointerText PointerText { get; private set; }
        private String editedMultiLineText;

        public TranslationItemViewModel(PointerText pointerText)
        {
            PointerText = pointerText;
            TextHandler.TranslateStringToBinary(PointerText);
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
                return PointerText.AvailableLength - PointerText.TextBytes.Count;
            }
        }

        public String SingleLineText
        {
            get
            {
                return PointerText.SingleLineText;
            }
            set
            {
                PointerText.SingleLineText = value;
                TextHandler.TranslateStringToBinary(PointerText);
                OnPropertyChanged("SingleLineText");
                OnPropertyChanged("RemainingLength");
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
            }
        }

        public Boolean HasUnsavedChanges
        {
            get
            {
                return editedMultiLineText != null;
            }
        }

        public String MultiLineText
        {
            get
            {
                if(editedMultiLineText == null)
                    return TextHandler.FormatEditString(SingleLineText);
                return editedMultiLineText;
            }
            set
            {
                editedMultiLineText = value;
                OnPropertyChanged("HasUnsavedChanges");
            }
        }

        public RelayCommand SaveMultiLineTextCommand
        {
            get
            {
                return new RelayCommand(param => SaveMultiLineText(), param => CanSaveMultiLineText());
            }
        }

        private Boolean CanSaveMultiLineText()
        {
            return HasUnsavedChanges;
        }

        private void SaveMultiLineText()
        {
            SingleLineText = TextHandler.EditStringToSingleLine(editedMultiLineText, IsSpecialDialog);
            editedMultiLineText = null;
            OnPropertyChanged("MultiLineText");
        }
    }
}
