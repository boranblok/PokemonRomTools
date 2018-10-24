using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GrabPokemonTextEntries
{
    class Program
    {
        private static readonly byte end = 0xFF;
        private static readonly byte StringPointer = 0x08;
        private static readonly int stringPointerInt = 0x08000000;
        private static readonly int backSearch = 300;
        private static readonly int minStringLength = 1;

        private static Dictionary<Int32, List<Int32>> foundPointers = new Dictionary<Int32, List<Int32>>();

        static void Main(string[] args)
        {
            if (args.Length < 1)
                throw new ArgumentException("Pass a rom file to parse.");
            var romFile = new FileInfo(args[0]);
            if (!romFile.Exists)
                throw new Exception(String.Format("Rom file {0} does not exist.", args[0]));

            var romContents = new byte[romFile.Length];
            using(var writer = new MemoryStream(romContents))
            using (var reader = romFile.OpenRead())
            {
                reader.CopyTo(writer);
            }

            var bpre = new FileInfo(@"C:\Users\benb\Dropbox\Persoonlijk\Pokemon FireRed Translation project\AdvanceText\ini\BPRE.ini");
            String bpreContents;
            using (var reader = bpre.OpenText())
            {
                bpreContents = reader.ReadToEnd();
            }
            var expr = new Regex("[0-9A-F]{6}");
            var exprMatches = expr.Matches(bpreContents);
            var notFound = 0;
            var sw = new Stopwatch();
            sw.Start();
            foreach (Match match in exprMatches)
            {
                //var endianSwappedMatch = match.Value.Substring(4) + match.Value.Substring(2, 2) + match.Value.Substring(0, 2);
                if (int.TryParse(match.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int intValue))
                {
                    if (foundPointers.ContainsKey(intValue))
                    {
                        Console.WriteLine("Pointer {0:X6} appears multiple times in BPRE.", intValue);
                    }
                    else
                    {
                        if (!FindTextPointers(romContents, intValue))
                        {
                            Console.WriteLine("Pointer {0:X6} was not found in rom", intValue);
                            notFound++;
                        }
                    }
                }
            }

            var multipleFound = 0;
            
            foreach(var match in foundPointers)
            {
                if(match.Value.Count != 1)
                {
                    Console.WriteLine("Pointer {0:X6} has {1} matches.", match.Key, match.Value.Count);
                    multipleFound++;
                }
            }
            sw.Stop();
            Console.WriteLine("On {0} searches, {1} were not found and {2} had more than one match. Searching took {3}", exprMatches.Count, notFound, multipleFound, sw.Elapsed);
            
            //var length = romContents.Length / 1000;

            
            //int prevEnd = 0;
            //for (int i = 0;  i < length; i++)
            //{
            //    if (i % 100 == 0) Console.WriteLine("Processing byte {0}", i);

            //    var readByte = romContents[i];
            //    if(readByte == end)
            //    {
            //        if(prevEnd != 0 && i - prevEnd > minStringLength)
            //        {
            //            if(!FindTextPointers(romContents, prevEnd + 1))
            //            {
            //                var backSearchLimit = (i - prevEnd < backSearch)? i - prevEnd : backSearch;
            //                for(int j = i - 1; j > (i - backSearchLimit + 1); j--)
            //                {
            //                    FindTextPointers(romContents, j);
            //                }
            //            }
            //        }
            //        prevEnd = i;
            //    }
            //}
            
            //Console.WriteLine("Processing {0} bytes took {1}", length, sw.Elapsed);
            Console.ReadLine();
        }

        private static Boolean FindTextPointers(byte[] romContents, int possibleStringStart)
        {
            var sw = new Stopwatch();
            sw.Start();
            var textPointerValue = stringPointerInt + possibleStringStart;
            var textPointerBytes = BitConverter.GetBytes(textPointerValue);
            //var searcher = new BoyerMooreBinarySearch(textPointerBytes);
            //var result = searcher.GetMatchIndexes(romContents);
            var result = ByteBinarySearcher.FindMatches(textPointerBytes, romContents);
            sw.Stop();
            
            if (result.Count > 0)
            {
                foundPointers.Add(possibleStringStart, new List<int>(result.Select(l => (Int32)l)));
                return true;
            }

            return false;
        }
    }
}
