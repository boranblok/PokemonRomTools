using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    public class PointerText
    {
        private string _text;
        private ReadOnlyCollection<byte> _textBytes;
        private List<int> _references;

        public Int32 Address { get; set; }

        public List<Int32> References { get => _references; set { _references = value; ReferenceCount = value.Count;  } }
        public Int32 ReferenceCount { get; set; }
        public String Text
        {
            get => _text;
            set { _text = value; _textBytes = null; }
        }
        public ReadOnlyCollection<Byte> TextBytes
        {
            get => _textBytes;
            set { _textBytes = value; _text = null; }
        }
        public Boolean ForceRepointReference { get; set; }
        public Int32 AvailableLength { get; set; }

        public Boolean MustRepointReference { get; set; }

        internal void SetTranslatedText(String translatedText)
        {
            _text = translatedText;
        }

        internal void SetTranslatedBytes(ReadOnlyCollection<Byte> translatedBytes)
        {
            _textBytes = translatedBytes;
        }

        public Boolean CanRepointReference
        {
            get
            {
                return ReferenceCount == 1;
            }
        }

        public Boolean IsTranslated
        {
            get
            {
                return Text != null && TextBytes != null;
            }
        }

        public override String ToString()
        {
            return String.Format("{0:X6}|{1,2:#0}|{2,3:##0}|{3}|{4}", Address, ReferenceCount, AvailableLength, 0, Text);
        }

        public static PointerText FromString(String pointerTextString)
        {
            var result = new PointerText();

            var parts = pointerTextString.Split('|');
            if (parts.Length != 5)
                throw new Exception(String.Format("A PointerText value has 5 segments separated by a | char. {0} is not valid", pointerTextString));

            if (Int32.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var address))
                result.Address = address;
            else
                throw new Exception(String.Format("{0} is not a valid hexadecimal integer. {1} is not valid.", parts[0], pointerTextString));

            if (Int32.TryParse(parts[1], out var referenceCount))
                result.ReferenceCount = referenceCount;
            else
                throw new Exception(String.Format("{0} is not a valid integer. {1} is not valid", parts[1], pointerTextString));

            if (Int32.TryParse(parts[2], out var availableLength))
                result.AvailableLength = availableLength;
            else
                throw new Exception(String.Format("{0} is not a valid integer. {1} is not valid", parts[2], pointerTextString));

            switch (parts[3])
            {
                case "0":
                    result.ForceRepointReference = false;
                    break;
                case "1":
                    result.ForceRepointReference = true;
                    break;
                default:
                    throw new Exception(String.Format("{0} is not a valid boolean. {1} is not valid", parts[3], pointerTextString));
            }

            result.Text = parts[4];

            return result;
        }
    }
}
