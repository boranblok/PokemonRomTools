using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    public class TextReference
    {
        public Int32 Address { get; set; }
        public Boolean Repoint { get; set; }

        public override string ToString()
        {
            return (Repoint ? "1" : "0") + "," + Address.ToString("X6");
        }

        public static TextReference FromString(String textReferenceString)
        {
            var parts = textReferenceString.Split(',');
            if (parts.Length != 2)
                throw new Exception("A textReference is in the format 1,99AACC");

            var textReference = new TextReference();

            switch(parts[0])
            {
                case "0":
                    textReference.Repoint = false;
                    break;
                case "1":
                    textReference.Repoint = true;
                    break;
                default:
                    throw new Exception("A textReference is in the format 1,99AACC");
            }

            if (Int32.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var address))
                textReference.Address = address;
            else
                throw new Exception("A textReference is in the format 1,99AACC");

            return textReference;
        }
    }
}
