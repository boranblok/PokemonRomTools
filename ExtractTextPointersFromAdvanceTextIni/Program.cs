using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    class Program
    {
        private static readonly Boolean test = false;
        static void Main(string[] args)
        {
            if (args.Length < 3)
                throw new ArgumentException("Pass a rom file, advance text ini to parse and output file.");

            var outputFile = new FileInfo(args[2]);
            if (outputFile.Exists)
                throw new Exception("Output file already exists. Please specify another name.");

            var foundText = new Dictionary<Int32, PointerText>();

            var rom = new RomDataWrapper(new FileInfo(args[0]));

            var bpre = new FileInfo(args[1]);
            String bpreContents;
            using (var reader = bpre.OpenText())
            {
                bpreContents = reader.ReadToEnd();
            }            

            var expr = new Regex("[0-9A-F]{6}");
            var exprMatches = expr.Matches(bpreContents);
            var sw = new Stopwatch();
            sw.Start();
            foreach (Match match in exprMatches)
            {
                if (Int32.TryParse(match.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int intValue))
                {
                    if (foundText.ContainsKey(intValue))
                    {
                        Console.WriteLine("Pointer {0:X6} appears multiple times in BPRE, will be skipped.", intValue);
                    }
                    else
                    {
                        var text = rom.GetTextAtPointer(intValue);
                        //if (text.AvailableLength > 0)
                        {
                            TextHandler.TranslateBinaryToString(text);
                            if (test)
                            {
                                var bytes = new List<Byte>(text.TextBytes);
                                var textValue = text.SingleLineText;
                                text.SingleLineText = null;
                                text.SingleLineText = textValue;
                                TextHandler.TranslateStringToBinary(text);
                                var bytes2 = new List<Byte>(text.TextBytes);
                                Debug.Assert(bytes.Count == bytes2.Count, "Roundtrip byte translation failed, arrays are different length.");
                                for (int i = 0; i < bytes.Count; i++)
                                {
                                    Debug.Assert(bytes[i] == bytes2[i], "Roundtrip byte translation failed, arrays have different content.",
                                        "byte[{0}] has value {1:X2} and byte2[{0}] has value {2:X2}.", i, bytes[i], bytes2[i]);
                                }
                            }
                            foundText.Add(intValue, text);
                        }
                    }
                }
            }

            using (var writer = new StreamWriter(outputFile.OpenWrite(), Encoding.GetEncoding(1252)))
            {
                foreach (var key in foundText.Keys)
                {                    
                    writer.WriteLine(foundText[key]);
                }
            }
            sw.Stop();

            Console.WriteLine("On {0} searches searching took {1}", exprMatches.Count, sw.Elapsed);
        }
    }
}
