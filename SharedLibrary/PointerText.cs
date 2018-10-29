using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{  
    public class PointerText
    {
        public static List<PointerText> ReadPointersFromFile(FileInfo file)
        {
            var lines = new List<PointerText>();
            using (var sourceReader = new StreamReader(file.OpenRead(), Encoding.GetEncoding(1252)))
            {
                var sourceLine = sourceReader.ReadLine();
                while (sourceLine != null)
                {
                    if (sourceLine.Length > 5 && PointerText.HexChars.Contains(sourceLine[0]))
                    {
                        lines.Add(PointerText.FromString(sourceLine));
                    }
                    sourceLine = sourceReader.ReadLine();
                }
            }
            return lines;
        }

        public static void WritePointersToFile(FileInfo file, IEnumerable<PointerText> lines)
        {
            using (var writer = new StreamWriter(file.Open(FileMode.Create), Encoding.GetEncoding(1252)))
            {
                foreach (var line in lines.OrderBy(l => l.Address))
                {
                    writer.WriteLine(line);
                }
            }
        }

        public static readonly Char[] HexChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f' };
        private string _singleLineText;
        private ReadOnlyCollection<byte> _textBytes;
        private List<int> _references;

        public Int32 Address { get; set; }

        public List<Int32> References { get => _references; set { _references = value; ReferenceCount = value.Count;  } }
        public Int32 ReferenceCount { get; set; }
        public String SingleLineText
        {
            get => _singleLineText;
            set { _singleLineText = value; _textBytes = null; }
        }
        public ReadOnlyCollection<Byte> TextBytes
        {
            get => _textBytes;
            set { _textBytes = value; _singleLineText = null; }
        }
        public Boolean ForceRepointReference { get; set; }
        public Int32 AvailableLength { get; set; }

        public Boolean MustRepointReference { get; set; }
        public Boolean IsTranslated { get; set; }
        public TextMode TextMode { get; set; }

        internal void SetSingleLineText(String translatedText)
        {
            _singleLineText = translatedText;
        }

        internal void SetTextBytes(ReadOnlyCollection<Byte> translatedBytes)
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

        public override String ToString()
        {
            return String.Format("{0:X6}|{1,2:#0}|{2,3:##0}|{3}|{4}|{5}|{6}", 
                Address, ReferenceCount, AvailableLength, ForceRepointReference ? 1 : 0, IsTranslated ? "Y" : "N", TextMode.ToString()[0], SingleLineText);
        }

        public static PointerText FromString(String pointerTextString)
        {
            var result = new PointerText();

            var parts = pointerTextString.Split('|');
            if (parts.Length != 7)
                throw new Exception(String.Format("A PointerText value has 7 segments separated by a | char. {0} is not valid", pointerTextString));

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

            switch (parts[4])
            {
                case "N":
                    result.IsTranslated = false;
                    break;
                case "T":
                    result.IsTranslated = true;
                    break;
                default:
                    throw new Exception(String.Format("{0} is not a valid translated Value, expected Y or N. {1} is not valid", parts[4], pointerTextString));
            }

            switch (parts[5])
            {
                case "I":
                    result.TextMode = TextMode.Into;
                    break;
                case "D":
                    result.TextMode = TextMode.Dialog;
                    break;
                default:
                    throw new Exception(String.Format("{0} is not a valid textmode Value, expected I or N. {1} is not valid", parts[5], pointerTextString));
            }

            result.SingleLineText = parts[6];

            return result;
        }
    }
}
