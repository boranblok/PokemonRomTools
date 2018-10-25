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
        private static readonly byte end = 0xFF;
        private static readonly byte StringPointer = 0x08;
        private static readonly int stringPointerInt = 0x08000000;
        private static readonly int backSearch = 300;
        private static readonly int minStringLength = 1;        
        private static Dictionary<byte, String> translationTable;
        private static byte printableCharStart = 0xA1;
        private static byte printableCharEnd = 0xEE;
        private static int successiveNPCCutoff = 3;
        private static int totalNPCCutoff = 10;
        private static byte fc = 0xFC;
        private static byte fd = 0xFD;
        private static int startPosition = 0x172255;


        private static readonly Object foundPointersLockObject = new Object();
        private static Dictionary<Int32, List<Int32>> foundPointers = new Dictionary<Int32, List<Int32>>();

        static void Main(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Pass a rom file and translation table to parse.");
            var romFile = new FileInfo(args[0]);
            if (!romFile.Exists)
                throw new Exception(String.Format("Rom file {0} does not exist.", args[0]));

            var tableFile = new FileInfo(args[1]);
            if (!tableFile.Exists)
                throw new Exception(String.Format("Table file {0} does not exist.", args[1]));

            Loadtable(tableFile);

            var romContents = new byte[romFile.Length];
            using (var writer = new MemoryStream(romContents))
            using (var reader = romFile.OpenRead())
            {
                reader.CopyTo(writer);
            }

            var sw = new Stopwatch();
            sw.Start();

            var searchSpace = (romContents.Length - startPosition) / 4;

            var t1 = Task.Run(() => FindStrings(romContents, startPosition, searchSpace));
            var t2 = Task.Run(() => FindStrings(romContents, startPosition + searchSpace, searchSpace * 2));
            var t3 = Task.Run(() => FindStrings(romContents, startPosition + searchSpace * 2, searchSpace * 3));
            var t4 = Task.Run(() => FindStrings(romContents, startPosition + searchSpace * 3, romContents.Length));
            Task.WaitAll(t1, t2, t3, t4);
            
            sw.Stop();

            Console.WriteLine("Processing {0} bytes took {1}", romContents.Length, sw.Elapsed);

            sw.Reset();
            sw.Start();
            var outputFile = new FileInfo("FoundStrings.txt");
            using (var writer = new StreamWriter(outputFile.OpenWrite(), Encoding.GetEncoding(1252)))
            {
                foreach (var pointer in foundPointers)
                {
                    var stringValue = GetTextAtPointer(romContents, pointer.Key);
                    writer.WriteLine("{0:X6},{1:##}: {2}", pointer.Key, pointer.Value.Count, stringValue);
                }
            }
            sw.Stop();

            Console.WriteLine("Writing {0} lines took {1}", foundPointers.Count, sw.Elapsed);

            Console.ReadLine();
        }

        private static void FindStrings(byte[] romContents, int from, int to)
        {
            int prevEnd = from;
            for (int i = from; i < to ; i++)
            {
                if (i % 100 == 0) Console.WriteLine("Processing byte {0}, {1}% done", i, ((i - from) / (to -from))*100);

                var readByte = romContents[i];
                if (readByte == end)
                {
                    if (prevEnd != from && i - prevEnd > minStringLength)
                    {
                        if (!FindTextPointers(romContents, prevEnd + 1))
                        {
                            var backSearchLimit = (i - prevEnd < backSearch) ? i - prevEnd : backSearch;
                            var successiveNPC = 0;
                            var totalNPC = 0;
                            for (int j = i - 1; j > (i - backSearchLimit + 1); j--)
                            {
                                byte value = romContents[j];
                                if (value != fc && value != fd && (value < printableCharStart || value > printableCharEnd))
                                {
                                    successiveNPC++;
                                    totalNPC++;
                                    if (successiveNPC > 2 || totalNPC > totalNPCCutoff) break;
                                }
                                else
                                {
                                    if (successiveNPC > 0)
                                    {
                                        if (value == fc || value == fd)
                                            successiveNPC = 0;
                                        else
                                            break;
                                    }
                                    else
                                    {
                                        if (value == fc || value == fd)
                                            break;
                                    }
                                }
                                if (FindTextPointers(romContents, j))
                                    break; //we stop after first found pointer.
                            }
                        }
                    }
                    prevEnd = i;
                }
            }
        }

        private static void Loadtable(FileInfo tableFile)
        {
            translationTable = new Dictionary<byte, string>();
            String table = File.ReadAllText(tableFile.FullName, Encoding.GetEncoding(1252));
            var lines = table.Split( new char[] { '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries );
            foreach(var line in lines)
            {
                Int32 indexOfEquals = line.IndexOf('=');
                if(indexOfEquals == 2)
                {
                    if (Byte.TryParse(line.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte byteValue))
                    {
                        var value = line.Substring(3);
                        translationTable.Add(byteValue, value);                        
                    }
                }
            }
        }

        private static String GetTextAtPointer(byte[] romContents, int stringStart)
        {
            var builder = new StringBuilder();
            bool endFound = false;
            int overFlowProtection = 1000;
            int index = stringStart;
            while(!endFound && overFlowProtection-- > 0 && index < romContents.Length)
            {
                var value = romContents[index];
                if(value == end)
                {
                    endFound = true;
                }
                else
                {
                    if(translationTable.ContainsKey(value))
                    {
                        builder.Append(translationTable[value]);
                    }
                    else
                    {
                        builder.AppendFormat("{0:X2}", value);
                    }
                }
                index++;
            }
            return builder.ToString();
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
                lock (foundPointersLockObject)
                {
                    foundPointers.Add(possibleStringStart, new List<int>(result.Select(l => (Int32)l)));
                }
                return true;
            }

            return false;
        }
    }
}
