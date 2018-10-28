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
        private TextHandler _textHandler;

        public TranslationItemViewModel(String translationFileLine, TextHandler textHandler)
        {
            _textHandler = textHandler;
            PointerText = PointerText.FromString(translationFileLine);
            _textHandler.Translate(PointerText);
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

        public String Text
        {
            get
            {
                return PointerText.Text;
            }
            set
            {
                PointerText.Text = value;
                OnPropertyChanged("RemainingLength");
            }
        }        
    }
}
