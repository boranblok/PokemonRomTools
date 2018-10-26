using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    public class TextHandler
    {
        private static Dictionary<Byte, String> translationTable;
        private static Dictionary<String, Byte> reverseTranslationTable;

        public TextHandler(FileInfo tableFile)
        {
            Loadtable(tableFile);
        }

        private static void Loadtable(FileInfo tableFile)
        {
            if (!tableFile.Exists)
                throw new Exception(String.Format("Table file {0} does not exist.", tableFile.FullName));
            translationTable = new Dictionary<Byte, String>();
            reverseTranslationTable = new Dictionary<String, Byte>();
            var table = File.ReadAllText(tableFile.FullName, Encoding.GetEncoding(1252));
            var lines = table.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var indexOfEquals = line.IndexOf('=');
                if (indexOfEquals == 2)
                {
                    if (Byte.TryParse(line.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Byte byteValue))
                    {
                        var value = line.Substring(3);
                        translationTable.Add(byteValue, value);
                        reverseTranslationTable.Add(value, byteValue);
                    }
                }
            }
        }

        public void Translate(PointerText text)
        {
            if(!text.IsTranslated)
            {
                if (text.Text == null)
                    TranslateBinaryToString(text);
                else if (text.TextBytes == null)
                    TranslateStringToBinary(text);
            }
        }

        public void TranslateBinaryToString(PointerText text)
        {
            var builder = new StringBuilder();
            foreach (var b in text.TextBytes)
            {
                if (translationTable.ContainsKey(b))
                {
                    builder.Append(translationTable[b]);
                }
                else
                {
                    builder.AppendFormat("[x{0:X2}]", b);
                }
            }
            text.SetTranslatedText(builder.ToString());
        }

        public void TranslateStringToBinary(PointerText text)
        {
            var bytes = new List<Byte>();
            for(int i = 0; i < text.Text.Length; i++)
            {
                String searchValue;
                switch(text.Text[i])
                {
                    case '[':
                        var endIndex = text.Text.IndexOf(']', i + 1);
                        if (endIndex < 0)
                            throw new Exception(String.Format("The text {0} has an open escape sequence [ without ]", text));
                        searchValue = text.Text.Substring(i, endIndex - i + 1);
                        i = endIndex;
                        break;
                    case ']':
                        throw new Exception(String.Format("The text {0} has an open escape sequence ] without [", text));
                    default:
                        searchValue = text.Text[i].ToString();
                        break;
                }
                if (reverseTranslationTable.ContainsKey(searchValue))
                {
                    bytes.Add(reverseTranslationTable[searchValue]);
                }
                else
                {
                    Boolean isHex = false;
                    if(searchValue.StartsWith("[x") && searchValue.Length == 5)
                    {
                        if(Byte.TryParse(searchValue.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexValue))
                        {
                            isHex = true;
                            bytes.Add(hexValue);                            
                        }
                    }

                    if(!isHex)
                        throw new Exception(String.Format("The text {0} cannot be found in the table mapping and is no hex value. Full text: {1}", searchValue, text));
                }
            }
            text.SetTranslatedBytes(bytes.AsReadOnly());
        }
    }
}
