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

        public Int32 Position { get; set; }
        public Int32 ReferenceCount { get; set; }
        public String Text
        {
            get => _text;
            set { _text = value; TextBytes = null; }
        }
        public ReadOnlyCollection<Byte> TextBytes { get; set; }
        public Boolean HasHex { get; set; }
        public Int32 AvailableLength { get; set; }

        public Boolean MustRepointReference { get; set; }

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
            return String.Format("{0:X6}|{1,2:#0}|{2,3:##0}|{3}|{4}", Position, ReferenceCount, AvailableLength, HasHex ? 1 : 0, Text);
        }

        public static PointerText FromString(String pointerTextString)
        {
            var result = new PointerText();

            var parts = pointerTextString.Split('|');
            if (parts.Length != 5)
                throw new Exception(String.Format("A PointerText value has 5 segments separated by a | char. {0} is not valid", pointerTextString));

            if (Int32.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var position))
                result.Position = position;
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

            switch(parts[3])
            {
                case "0":
                    result.HasHex = false;
                    break;
                case "1":
                    result.HasHex = true;
                    break;
                default:
                    throw new Exception(String.Format("{0} is not a valid boolean. {1} is not valid", parts[3], pointerTextString));
            }

            result.Text = parts[4];

            return result;
        }
    }
}
