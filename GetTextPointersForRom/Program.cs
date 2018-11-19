using PkmnAdvanceTranslation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetTextPointersForRom
{
    class Program
    {
        static void Main(string[] args)
        {
            var rom = new RomDataWrapper(new FileInfo(args[0]));

            var lines = LoadTranslationFileLines(args[1]);

            var linesWithReferences = LoadLinereferencesFromRom(rom, lines);

            var outputFile = new FileInfo(args[2]);

            PointerText.WritePointersToFile(outputFile, linesWithReferences.OrderBy(l => l.Address));
        }

        private static List<PointerText> LoadTranslationFileLines(String translationFileName)
        {
            var translationSourceFile = new FileInfo(translationFileName);
            if (!translationSourceFile.Exists)
            {
                Console.WriteLine("Translation source file {0} does not exist", translationFileName);
                return null;
            }
            return PointerText.ReadPointersFromFile(translationSourceFile);
        }

        private static List<PointerText> LoadLinereferencesFromRom(RomDataWrapper rom, List<PointerText> lines)
        {
            var linesWithReferences = new List<PointerText>();
            var lockObject = new Object();
            var numThreads = Environment.ProcessorCount;
            var numPerThread = lines.Count / numThreads;
            var tasks = new List<Task>();
            for (int i = 0; i < numThreads - 1; i++)
            {
                var translationFileLinesToHandle = lines.Skip(i * numPerThread).Take(numPerThread);
                tasks.Add(Task.Run(() => LoadLinereferencesFromRomTask(rom, translationFileLinesToHandle, linesWithReferences, lockObject, lines.Count)));
            }
            var finalTranslationFileLinesToHandle = lines.Skip((numThreads - 1) * numPerThread);
            tasks.Add(Task.Run(() => LoadLinereferencesFromRomTask(rom, finalTranslationFileLinesToHandle, linesWithReferences, lockObject, lines.Count)));

            Task.WaitAll(tasks.ToArray());

            return linesWithReferences;
        }

        private static void LoadLinereferencesFromRomTask(RomDataWrapper rom,
            IEnumerable<PointerText> lines, List<PointerText> linesWithReferences, Object lockObject, Int32 totalCount)
        {
            foreach (var line in lines)
            {
                var lineWithPointerInfo = rom.GetOriginalPointerInfo(line.Address);
                line.References = lineWithPointerInfo.References;
                lock (lockObject)
                {
                    linesWithReferences.Add(line);
                    if (linesWithReferences.Count % 100 == 0)
                    {
                        Console.Write("\rProgress: {0:##0}%   ", ((Decimal)linesWithReferences.Count / totalCount) * 100);
                    }
                }
            }
        }
    }
}
