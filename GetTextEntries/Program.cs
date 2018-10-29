using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    class Program
    {
        private static readonly byte end = 0xFF;
        private static readonly int backSearch = 300;
        private static readonly int minStringLength = 1;        
        private static byte printableCharStart = 0xA1;
        private static byte printableCharEnd = 0xEE;
        private static int successiveNPCCutoff = 3;
        private static int totalNPCCutoff = 10;
        private static byte fc = 0xFC;
        private static byte fd = 0xFD;
        private static int startPosition = 0x172255;
        //TODO: will we skip this empty block?
        private static int skipBlockStart = 0x71A232;
        private static int skipBlockEnd = 0xCFFFFF;

        private static List<Int32> existingtranslationLines;
        private static readonly Object newTranslationLinesLockObject = new object();
        private static List<Int32> newTranslationLines = new List<Int32>();        

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        static void Main(string[] args)
        {
            if (args.Length < 3)
                throw new ArgumentException("Pass a rom file, an existing translation file and an output file.");
            var rom = new RomDataWrapper(new FileInfo(args[0]));

            existingtranslationLines = LoadTranslationBaseLines(args[1]);
            if (existingtranslationLines == null)
                existingtranslationLines = new List<Int32>();

            var outputFile = new FileInfo(args[2]);
            if (outputFile.Exists)
                throw new Exception(String.Format("The output file {0} already exists.", args[2]));

            var sw = new Stopwatch();
            sw.Start();

            var numThreads = Environment.ProcessorCount;
            var numPerThread = (rom.RomContents.Length - startPosition) / numThreads;
            var tasks = new List<Task>();
            for (int i = 0; i < numThreads - 1; i++)
            {
                var name = "FT" + (i + 1);
                var from = startPosition + numPerThread * i;
                var to = startPosition + numPerThread * (i + 1);
                tasks.Add(Task.Run(() => FindStringPointers(name, rom, from, to)));
            }
            tasks.Add(Task.Run(() => FindStringPointers("FT" + numThreads, rom, startPosition + numPerThread * (numThreads - 1), rom.RomContents.Length)));
            Task.WaitAll(tasks.ToArray());

            sw.Stop();

            Console.WriteLine("Finding text in {0} bytes took {1}", rom.RomContents.Length, sw.Elapsed);

            var linesToTranslate = LoadNewTranslationLines(rom);

            Console.Write("\rReading progress: 100%   ");
            Console.WriteLine();

            Console.WriteLine("Writing missed text entries to file.");

            PointerText.WritePointersToFile(outputFile, linesToTranslate);

            Console.WriteLine("Done, press any key to continue...");
            Console.ReadLine();
        }

        private static List<PointerText> LoadNewTranslationLines(RomDataWrapper rom)
        {
            var sw = new Stopwatch();
            sw.Start();
            var translatedLines = new List<PointerText>();
            var lockObject = new Object();
            var numThreads = Environment.ProcessorCount;
            var numPerThread = newTranslationLines.Count / numThreads;
            var tasks = new List<Task>();
            for (int i = 0; i < numThreads - 1; i++)
            {
                var linesToHandle = newTranslationLines.Skip(i * numPerThread).Take(numPerThread);
                tasks.Add(Task.Run(() => LoadNewTranslationLinesTask(rom, linesToHandle, translatedLines, lockObject, newTranslationLines.Count)));
            }
            var finalLinesToHandle = newTranslationLines.Skip((numThreads - 1) * numPerThread);
            tasks.Add(Task.Run(() => LoadNewTranslationLinesTask(rom, finalLinesToHandle, translatedLines, lockObject, newTranslationLines.Count)));

            Task.WaitAll(tasks.ToArray());

            sw.Stop();
            return translatedLines;
        }

        private static void LoadNewTranslationLinesTask(RomDataWrapper rom,
           IEnumerable<Int32> linesToGet, List<PointerText> linestotranslate, Object lockObject, Int32 totalCount)
        {
            foreach (var line in linesToGet)
            {
                var lineTotranslate = rom.GetTextAtPointer(line);
                TextHandler.TranslateBinaryToString(lineTotranslate);
                lock (lockObject)
                {
                    linestotranslate.Add(lineTotranslate);
                    if (linestotranslate.Count % 100 == 0)
                    {
                        Console.Write("\rReading progress: {0:##0}%   ", ((Decimal)linestotranslate.Count / totalCount) * 100);
                    }
                }
            }
        }

        private static List<Int32> LoadTranslationBaseLines(String translationFileName)
        {
            var translationSourceFile = new FileInfo(translationFileName);
            if (!translationSourceFile.Exists)
            {
                Console.WriteLine("Translation source file {0} does not exist", translationFileName);
                return null;
            }
            return PointerText.ReadPointersFromFile(translationSourceFile).Select(l => l.Address).ToList();
        }

        private static void FindStringPointers(String threadName, RomDataWrapper rom, int from, int to)
        {
            int prevEnd = from;
            for (int i = from; i < to ; i++)
            {
                if (i % 10000 == 0)
                {
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS); //Prevent system sleep.
                    Console.WriteLine("Finding thread {0} {1:#0}% done", threadName, ((Decimal)(i - from) / (to - from)) * 100);
                }

                var readByte = rom.RomContents[i];
                if (readByte == end)
                {
                    if (prevEnd != from && i - prevEnd > minStringLength)
                    {
                        var possibleTxt = prevEnd + 1;
                        if (existingtranslationLines.Contains(possibleTxt))
                            continue;                            

                        if(rom.IsTextReference(possibleTxt))
                        { 
                            lock (newTranslationLinesLockObject)
                            {
                                newTranslationLines.Add(possibleTxt);
                            }
                        }
                        else
                        {
                            var backSearchLimit = (i - prevEnd < backSearch) ? i - prevEnd : backSearch;
                            var successiveNPC = 0;
                            var totalNPC = 0;
                            for (int j = i - 1; j > (i - backSearchLimit + 1); j--)
                            {
                                byte value = rom.RomContents[j];
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
                                if (rom.IsTextReference(j))
                                {
                                    lock (newTranslationLinesLockObject)
                                    {
                                        newTranslationLines.Add(j);
                                    }
                                    break; //we stop after first found pointer.
                                }
                            }
                        }
                    }
                    prevEnd = i;
                }
            }
        }
    }
}
