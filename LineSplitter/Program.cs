using PkmnAdvanceTranslation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            //Usage: TranslationFile levels outputDir
            var translationFileLines = LoadTranslationFileLines(args[0]);
            var levels = Int32.Parse(args[1]);
            var outDir = new DirectoryInfo(args[2]);
            SplitFile(translationFileLines, levels, 0, 100, outDir);
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

        private static void SplitFile(IEnumerable<PointerText> lines, Int32 level, Decimal from, Decimal to, DirectoryInfo outDir)
        {
            var newName = String.Format("{0:0.00}-{1:0.00}.txt", from, to);
            var newFile = new FileInfo(Path.Combine(outDir.FullName, newName));
            PointerText.WritePointersToFile(newFile, lines);

            level -= 1;
            if (level == 0)
                return;

            var halfwayLines = lines.Count() / 2;
            var halfway = (from + to) / 2;

            SplitFile(lines.Take(halfwayLines), level, from, halfway, outDir);
            SplitFile(lines.Skip(halfwayLines), level, halfway, to, outDir);
        }
    }
}
