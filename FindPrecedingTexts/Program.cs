using PkmnAdvanceTranslation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindPrecedingTexts
{
    class Program
    {
        private static readonly Int32 maxBackSearch = 200;

        private static readonly Object newTranslationLinesLockObject = new object();
        private static List<Int32> newTranslationLines = new List<Int32>();
        private static readonly Object processedBlocksLockObject = new object();
        private static Int32 processedBlocks;

        private static int startPosition = 0x172255;
        private static int skipBlockStart = 0x71A23D;
        private static int skipBlockEnd = 0xCFFFFF;

        static void Main(string[] args)
        {
            if(args.Length < 3)
            {
                Console.WriteLine("Pass at elast one rom file, output file, and existing translation file");
            }
            var rom = new RomDataWrapper(new FileInfo(args[0]));

            var tmpRomData = new Byte[rom.RomContents.Length - (skipBlockEnd - skipBlockStart)];
            Array.Copy(rom.RomContents, tmpRomData, skipBlockStart);
            Array.Copy(rom.RomContents, skipBlockEnd, tmpRomData, skipBlockStart, rom.RomContents.Length - skipBlockEnd);

            var romWithHole = new RomDataWrapper(tmpRomData);

            FileInfo outputFile = new FileInfo(args[1]);

            var pointers = new List<PointerText>();
            for(int i = 2; i < args.Length; i++)
            {
                pointers.AddRange(PointerText.ReadPointersFromFile(new FileInfo(args[i])));
            }

            pointers = pointers.OrderBy(p => p.Address).ToList();

            foreach (var pointer in pointers)
            {
                if (pointer.Address > skipBlockEnd)
                    pointer.Address -= (skipBlockEnd - skipBlockStart);
            }

            var blocks = new List<ValueTuple<Int32, Int32>>();
            var block = (pointers[0].Address, pointers[0].Address + pointers[0].AvailableLength);
            foreach(var pointer in pointers.Skip(1))
            {
                if (block.Item2 + 1 == pointer.Address)
                {
                    block = (block.Item1, pointer.Address + pointer.AvailableLength);
                }
                else
                {
                    blocks.Add(block);
                    block = (pointer.Address, pointer.Address + pointer.AvailableLength);
                }
            }

            ProcessBlocks(blocks, romWithHole);

            for (int i = 0; i < newTranslationLines.Count; i++)
            {
                if (newTranslationLines[i] > skipBlockStart)
                    newTranslationLines[i] += (skipBlockEnd - skipBlockStart);
            }

            var linesToTranslate = LoadNewTranslationLines(rom);

            Console.Write("\rReading progress: 100%   ");
            Console.WriteLine();

            Console.WriteLine("Writing missed text entries to file.");

            PointerText.WritePointersToFile(outputFile, linesToTranslate.OrderBy(l => l.Address));

            Console.WriteLine("Done, press any key to continue...");
            Console.ReadLine();
        }

        private static void ProcessBlocks(List<ValueTuple<Int32, Int32>> blocks, RomDataWrapper rom)
        {
            processedBlocks = 0;

            var numThreads = Environment.ProcessorCount;
            var numPerThread = blocks.Count / numThreads;
            var tasks = new List<Task>();
            for (int i = 0; i < numThreads - 1; i++)
            {
                var blocksToHandle = blocks.Skip((i * numPerThread) - 1).Take(numPerThread);
                tasks.Add(Task.Run(() => ProcessBlocksTask(rom, blocksToHandle, blocks.Count)));
            }
            var finalBlocksToHandle = blocks.Skip(((numThreads - 1) * numPerThread)-1);
            tasks.Add(Task.Run(() => ProcessBlocksTask(rom, finalBlocksToHandle, blocks.Count)));

            Task.WaitAll(tasks.ToArray());
        }

        private static void ProcessBlocksTask(RomDataWrapper rom, IEnumerable<ValueTuple<Int32, Int32>> blocksToHandle, Int32 totalBlocks)
        {
            var previousBlock = (0,0);
            foreach (var block in blocksToHandle)
            {
                var searchFrom = block.Item1 - 2;
                var searchTo = searchFrom - maxBackSearch;
                if (previousBlock.Item2 > searchTo)
                {
                    searchTo = previousBlock.Item2;
                }

                Console.WriteLine("From {0,6} To {1,6} Length {2,4}", searchFrom, searchTo, searchTo - searchFrom);

                for(int i = searchFrom; i > searchTo; i--)
                {
                    var pointerText = rom.GetOriginalPointerInfo(i);
                    if(pointerText.ReferenceCount > 0 && (pointerText.Address + pointerText.AvailableLength) <= searchFrom)
                    { 
                        lock (newTranslationLinesLockObject)
                        {
                            newTranslationLines.Add(i);
                        }
                        break; //we stop after first found pointer.
                    }
                }

                lock(processedBlocksLockObject)
                {
                    processedBlocks++;
                }

                previousBlock = block;

                Console.Write("\rScanning progress: {0:##0}%   ", ((Decimal)processedBlocks / totalBlocks) * 100);
            }
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
                var lineTotranslate = rom.GetOriginalPointerInfo(line);
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
    }
}
