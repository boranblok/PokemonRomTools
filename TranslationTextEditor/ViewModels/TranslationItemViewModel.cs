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
        private PointerText _pointerText;
        private TextHandler _textHandler;
        public TranslationItemViewModel(String translationFileLine, TextHandler textHandler)
        {
            _textHandler = textHandler;
            _pointerText = PointerText.FromString(translationFileLine);
            _textHandler.Translate(_pointerText);
        }

        public String Address
        {
            get
            {
                return _pointerText.Address.ToString("X6");
            }
        }

        public Int32 ReferenceCount
        {
            get
            {
                return _pointerText.ReferenceCount;
            }
        }

        public Int32 AvailableLength
        {
            get
            {
                return _pointerText.AvailableLength;
            }
        }

        public Int32 RemainingLength
        {
            get
            {
                return _pointerText.AvailableLength - _pointerText.TextBytes.Count;
            }
        }

        public String Text
        {
            get
            {
                return _pointerText.Text;
            }
            set
            {
                _pointerText.Text = value;
                OnPropertyChanged("RemainingLength");
            }
        }
    }
}
