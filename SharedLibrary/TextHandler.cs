using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    public class TextHandler
    {
        private static readonly Dictionary<Byte, String> byteTranstbl = new Dictionary<Byte, String>()
        {
            {0x00, " "},
            {0x01, "À"},
            {0x02, "Á"},
            {0x03, "Â"},
            {0x04, "Ç"},
            {0x05, "È"},
            {0x06, "É"},
            {0x07, "Ê"},
            {0x08, "Ë"},
            {0x09, "Ì"},
            {0x0B, "Î"},
            {0x0C, "Ï"},
            {0x0D, "Ò"},
            {0x0E, "Ó"},
            {0x0F, "Ô"},
            {0x10, "Œ"},
            {0x11, "Ù"},
            {0x12, "Ú"},
            {0x13, "Û"},
            {0x14, "Ñ"},
            {0x15, "ß"},
            {0x16, "à"},
            {0x17, "á"},
            {0x19, "ç"},
            {0x1A, "è"},
            {0x1B, "é"},
            {0x1C, "ê"},
            {0x1D, "ë"},
            {0x1E, "ì"},
            {0x20, "î"},
            {0x21, "ï"},
            {0x22, "ò"},
            {0x23, "ó"},
            {0x24, "ô"},
            {0x25, "œ"},
            {0x26, "ù"},
            {0x27, "ú"},
            {0x28, "û"},
            {0x29, "ñ"},
            {0x2A, "º"},
            {0x2B, "ª"},
            {0x2D, "&"},
            {0x2E, "+"},
            {0x34, "[Lv]"},
            {0x35, ", "},
            {0x36, ";"},
            {0x51, "¿"},
            {0x52, "¡"},
            {0x53, "[pk]"},
            {0x54, "[mn]"},
            {0x55, "[po]"},
            {0x56, "[ké]"},
            {0x57, "[bl]"},
            {0x58, "[oc]"},
            {0x59, "[k]"},
            {0x5A, "Í"},
            {0x5B, "%"},
            {0x5C, "("},
            {0x5D, ")"},
            {0x68, "â"},
            {0x6F, "í"},
            {0x79, "[U]"},
            {0x7A, "[D]"},
            {0x7B, "[L]"},
            {0x7C, "[R]"},
            {0x85, "<"},
            {0x86, ">"},
            {0xA1, "0"},
            {0xA2, "1"},
            {0xA3, "2"},
            {0xA4, "3"},
            {0xA5, "4"},
            {0xA6, "5"},
            {0xA7, "6"},
            {0xA8, "7"},
            {0xA9, "8"},
            {0xAA, "9"},
            {0xAB, "!"},
            {0xAC, "?"},
            {0xAD, "."},
            {0xAE, "-"},
            {0xAF, "·"},
            {0xB0, "[...]"},
            {0xB1, "«"},
            {0xB2, "»"},
            {0xB3, "`"},
            {0xB4, "'"},
            {0xB5, "[m]"},
            {0xB6, "[f]"},
            {0xB7, "$"},
            {0xB8, ","},
            {0xB9, "*"},
            {0xBA, "/"},
            {0xBB, "A"},
            {0xBC, "B"},
            {0xBD, "C"},
            {0xBE, "D"},
            {0xBF, "E"},
            {0xC0, "F"},
            {0xC1, "G"},
            {0xC2, "H"},
            {0xC3, "I"},
            {0xC4, "J"},
            {0xC5, "K"},
            {0xC6, "L"},
            {0xC7, "M"},
            {0xC8, "N"},
            {0xC9, "O"},
            {0xCA, "P"},
            {0xCB, "Q"},
            {0xCC, "R"},
            {0xCD, "S"},
            {0xCE, "T"},
            {0xCF, "U"},
            {0xD0, "V"},
            {0xD1, "W"},
            {0xD2, "X"},
            {0xD3, "Y"},
            {0xD4, "Z"},
            {0xD5, "a"},
            {0xD6, "b"},
            {0xD7, "c"},
            {0xD8, "d"},
            {0xD9, "e"},
            {0xDA, "f"},
            {0xDB, "g"},
            {0xDC, "h"},
            {0xDD, "i"},
            {0xDE, "j"},
            {0xDF, "k"},
            {0xE0, "l"},
            {0xE1, "m"},
            {0xE2, "n"},
            {0xE3, "o"},
            {0xE4, "p"},
            {0xE5, "q"},
            {0xE6, "r"},
            {0xE7, "s"},
            {0xE8, "t"},
            {0xE9, "u"},
            {0xEA, "v"},
            {0xEB, "w"},
            {0xEC, "x"},
            {0xED, "y"},
            {0xEE, "z"},
            {0xEF, "[>]"},
            {0xF0, ":"},
            {0xF1, "Ä"},
            {0xF2, "Ö"},
            {0xF3, "Ü"},
            {0xF4, "ä"},
            {0xF5, "ö"},
            {0xF6, "ü"},
            {0xF7, "[A]"},
            {0xF8, "[V]"},
            {0xF9, "[<]"},
            {0xFA, "[nb]"},
            {0xFB, "[nb2]"},
            {0xFC, "[FC]"},
            {0xFD, "[FD]"},
            {0xFE, "[br]"},
            {0xFF, "[END]"}
        };
        private static readonly Dictionary<String, Byte> byteTransTblreversed;

        private static readonly Dictionary<String, String> stringTransTbl = new Dictionary<String, String>()
        {
            //{"[nb2]", "\r\n\r\n" }, //BLB: newline handling is not handled by this
            //{"[br]", "\r\n" }, 
            //{"[nb]", "\r\n" },
            {"[FD]À", "[PLAYER]" },
            {"[FD]É", "[RIVAL]" },
            {"[FD]Á", "[var1]" },
            {"[FD]Â", "[var2]" },
            {"[FD]Ç", "[var3]" },
            {"[FD][x1F]", "[FD1F]" },
            {"[FD] ", "[FD00]" },
            {"[FD]È", "[FD05]" },
            {"[FD]Ê", "[FD07]" },
            {"[FD]Ë", "[FD08]" },
            {"[FD]Ì", "[FD09]" },
            {"[FD]Î", "[FD0B]" },
            {"[FD]Ï", "[FD0C]" },
            {"[FD]Ò", "[FD0D]" },
            {"[FD]Ó", "[FD0E]" },
            {"[FD]Ô", "[FD0F]" },
            {"[FD]Œ", "[FD10]" },
            {"[FD]Ù", "[FD11]" },
            {"[FD]Ú", "[FD12]" },
            {"[FD]Û", "[FD13]" },
            {"[FD]Ñ", "[FD14]" },
            {"[FD]ß", "[FD15]" },
            {"[FD]à", "[FD16]" },
            {"[FD]á", "[FD17]" },
            {"[FD]ç", "[FD19]" },
            {"[FD]è", "[FD1A]" },
            {"[FD]é", "[FD1B]" },
            {"[FD]ê", "[FD1C]" },
            {"[FD]ë", "[FD1D]" },
            {"[FD]ì", "[FD1E]" },
            {"[FD]î", "[FD20]" },
            {"[FD]ï", "[FD21]" },
            {"[FD]ò", "[FD22]" },
            {"[FD]ó", "[FD23]" },
            {"[FD]ô", "[FD24]" },
            {"[FD]œ", "[FD25]" },
            {"[FD]ù", "[FD26]" },
            {"[FD]ú", "[FD27]" },
            {"[FD]û", "[FD28]" },
            {"[FD]ñ", "[FD29]" },
            {"[FD]º", "[FD2A]" },

            {"[FC]À", "[clrT]" },
            {"[FC]Á", "[clrH]" },
            {"[FC]Â", "[clrShd]" },
            {"[FC]Ç", "[clrTH]" },
            {"[FC]ÉÁ", "[small]" },
            {"[FC]É ", "[smllat]" },
            {"[FC]É", "[fnt]" },    //BLB: Important that this is handled AFTER the previous.
            {"[FC]Ë", "[pausTim]" },
            {"[FC]Ì", "[pausBtn]" },
            {"[FC]Ï", "[\\]" },     //careful only [\] in reality
            {"[FC]Ò", "[shift]" },
            {"[FC]Œ", "[music]" },
            {"[FC]ß", "[jFnt]" },
            {"[FC]à", "[iFnt]" },            
            {"[FC]á", "[pMus]" },
            {"[FC][x18]", "[rMus]" }
        };

        static TextHandler()
        {
            byteTransTblreversed = new Dictionary<String, Byte>();
            foreach(var key in byteTranstbl.Keys)
            {
                byteTransTblreversed.Add(byteTranstbl[key], key);
            }
        }

        public static String FormatEditString(String singleLineText)
        {
            var formatted = singleLineText;
            formatted = formatted.Replace("[nb2]", "\r\n\r\n");
            formatted = formatted.Replace("[nb]", "\r\n");
            formatted = formatted.Replace("[br]", "\r\n");
            return formatted;
        }

        public static String EditStringToSingleLine(String editString, Boolean specialDialog)
        {
            var formatted = editString;
            if(specialDialog)
            {
                formatted = formatted.Replace("\r\n", "[br]");
            }
            else
            {
                formatted = Regex.Replace(formatted, "(?:\r\n|\r(?!\n)|(?<!\r)\n){2,}", "[nb2]");
                var newLineIndex = formatted.IndexOf(Environment.NewLine);
                while(newLineIndex >= 0)
                {
                    var previousBr = formatted.LastIndexOf("[br]", newLineIndex);
                    var previousNb2 = formatted.LastIndexOf("[nb2]", newLineIndex);
                    if (previousBr > previousNb2)
                        formatted = formatted.Remove(newLineIndex, Environment.NewLine.Length).Insert(newLineIndex, "[nb]");
                    else
                        formatted = formatted.Remove(newLineIndex, Environment.NewLine.Length).Insert(newLineIndex, "[br]");

                    newLineIndex = formatted.IndexOf(Environment.NewLine);
                }
            }
            return formatted;
        }

        public static String TranslateBinaryToString(IEnumerable<Byte> bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                if (byteTranstbl.ContainsKey(b))
                {
                    builder.Append(byteTranstbl[b]);
                }
                else
                {
                    builder.AppendFormat("[x{0:X2}]", b);
                }
            }
            foreach (var key in stringTransTbl.Keys)
            {
                builder.Replace(key, stringTransTbl[key]);
            }
            return builder.ToString();
        }

        public static List<Byte> TranslateStringToBinary(String text)
        {
            var bytes = new List<Byte>();
            var textToWrite = text;
            foreach (var key in stringTransTbl.Keys)
            {
                textToWrite = textToWrite.Replace(stringTransTbl[key], key);
            }            
            for (int i = 0; i < textToWrite.Length; i++)
            {
                String searchValue;
                switch (textToWrite[i])
                {
                    case '[':
                        var endIndex = textToWrite.IndexOf(']', i + 1);
                        if (endIndex < 0)
                            throw new Exception(String.Format("The text {0} has an open escape sequence [ without ]", textToWrite));
                        searchValue = textToWrite.Substring(i, endIndex - i + 1);
                        i = endIndex;
                        break;
                    case ']':
                        throw new Exception(String.Format("The text {0} has an open escape sequence ] without [", textToWrite));
                    default:
                        searchValue = textToWrite[i].ToString();
                        break;
                }
                if (byteTransTblreversed.ContainsKey(searchValue))
                {
                    bytes.Add(byteTransTblreversed[searchValue]);
                }
                else
                {
                    Boolean isHex = false;
                    if (searchValue.StartsWith("[x") && searchValue.Length == 5)
                    {
                        if (Byte.TryParse(searchValue.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexValue))
                        {
                            isHex = true;
                            bytes.Add(hexValue);
                        }
                    }

                    if (!isHex)
                        throw new Exception(String.Format("The text {0} cannot be found in the table mapping and is no hex value. Full text: {1}", searchValue, textToWrite));
                }
            }
            return bytes;
        }
    }
}
