using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger("Translator");
        private static readonly char[] hexChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f' };
        private static Boolean isDryrun = true;


        private static int PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} TranslationFile.txt RomToTranslate.gba TranslatedRom.gba Execute", Assembly.GetEntryAssembly().CodeBase);
            Console.WriteLine();
            Console.WriteLine("The Translation file format is explained in TranslationFileFormat.txt");
            Console.WriteLine("The RomToTranslate is a GBA rom, this file will NOT be modified, only read.");
            Console.WriteLine("The TranslatedRom is the output GBA rom, this file may not exist yet.");
            Console.WriteLine("Execute is literally the word \"Execute\", if this is not specified a Dry-Run is performed and no output is actually generated.");
            return 1;
        }

        static Int32 Main(string[] args)
        {
            log.InfoFormat("Translation process started, passed args [{0}]", String.Join(" ", args.Select(s => "\"" + s + "\"")));
            if (args.Length < 3)
                return PrintUsage();

            var rom = new RomDataWrapper(new FileInfo(args[0]));
            var textHandler = new TextHandler(new FileInfo("table file.tbl"));

            var translationBaseLines = LoadTranslationBaseLines(args[1]);
            if (translationBaseLines == null)
                return PrintUsage();

            if (args.Length == 4 && args[3].Equals("Execute", StringComparison.InvariantCultureIgnoreCase))
                isDryrun = false;

            //If we want to repoint as few lines as possible we must process lines bottom up.
            //This way if a line needs to be repointed, but fits due to the line afterwards being repointed already
            //the line can extend into the space previously occupied by the already repointed line.
            translationBaseLines = translationBaseLines.OrderByDescending(t => t.Address).ToList();

            log.Info("Re - fetching text references and available length from rom file.");
            Console.WriteLine("Re-fetching text references and available length from rom file.");
            List<PointerText> translatedLines = MergeTranslatedLinesWithOriginals(rom, textHandler, translationBaseLines);
            Console.WriteLine();

            log.Info("Searching lines that need repointing.");
            Console.WriteLine("Searching lines that need repointing.");

            //find lines that need repointing
            RepointLinesIfRequired(translatedLines);

            translatedLines = translatedLines.OrderBy(l => l.Address).ToList(); //When actually modifying the rom data we go in order.

            log.Info("Will repoint these lines:");
            Console.WriteLine("Will repoint these lines:");
            foreach (var translatedLine in translatedLines.Where(l => l.MustRepointReference))
            {
                log.Info(translatedLine);
                Console.WriteLine(translatedLine);
            }

            return 0;
        }

        private static List<PointerText> LoadTranslationBaseLines(String translationFileName)
        {
            var translationBaseLines = new List<PointerText>();
            var translationSourceFile = new FileInfo(translationFileName);
            if (!translationSourceFile.Exists)
            {
                log.FatalFormat("Translation source file {0} does not exist", translationFileName);
                Console.WriteLine("Translation source file {0} does not exist", translationFileName);
                return null;
            }
            using (var sourceReader = new StreamReader(translationSourceFile.OpenRead(), Encoding.GetEncoding(1252)))
            {
                var sourceLine = sourceReader.ReadLine();
                while (sourceLine != null)
                {
                    if (sourceLine.Length > 5 && hexChars.Contains(sourceLine[0]))
                    {
                        translationBaseLines.Add(PointerText.FromString(sourceLine));
                    }
                    sourceLine = sourceReader.ReadLine();
                }
            }
            return translationBaseLines;
        }

        private static List<PointerText> MergeTranslatedLinesWithOriginals(RomDataWrapper rom, TextHandler textHandler, List<PointerText> translationBaseLines)
        {
            var translatedLines = new List<PointerText>();
            var index = 0;
            foreach (var baseLine in translationBaseLines)
            {
                if (index++ % 100 == 0)
                {
                    Console.Write("\rProgress: {0:##0}%   ", ((Decimal)index / translationBaseLines.Count) * 100);
                }
                var originalLine = rom.GetTextAtPointer(baseLine.Address);
                originalLine.Text = baseLine.Text;
                originalLine.ForceRepointReference = baseLine.ForceRepointReference;
                textHandler.Translate(originalLine);
                translatedLines.Add(originalLine);
            }

            return translatedLines;
        }

        private static void RepointLinesIfRequired(List<PointerText> translatedLines)
        {
            foreach (var translatedLine in translatedLines.Where(l => l.TextBytes.Count > l.AvailableLength))
            {
                if (translatedLine.CanRepointReference)
                {
                    //If we can repoint we only check if we really need to.
                    var requiredSpace = translatedLine.TextBytes.Count - translatedLine.AvailableLength;
                    var nextTextAddress = translatedLine.Address + translatedLine.AvailableLength + 1;
                    while (requiredSpace > 0)
                    {
                        var nextLine = translatedLines.FirstOrDefault(l => l.Address == nextTextAddress);
                        if (nextLine != null && nextLine.MustRepointReference)
                        {
                            requiredSpace -= nextLine.AvailableLength;
                            nextTextAddress = nextLine.Address + nextLine.AvailableLength + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    translatedLine.MustRepointReference = requiredSpace > 0;
                }
                else
                {
                    if (translatedLine.ForceRepointReference)
                    {
                        var currentPointerValue = String.Format("{0:X6}", translatedLine.Address);
                        currentPointerValue = currentPointerValue.Substring(4) + currentPointerValue.Substring(2, 2) + currentPointerValue.Substring(0, 2) + "08";

                        translatedLine.MustRepointReference = true;
                        log.Warn("Forcing repoint of:");
                        log.Warn(translatedLine);
                        log.Warn("This can cause rom corruption, test thoroughly!");
                        log.WarnFormat("Following locations will have their indices changed from {0}", currentPointerValue);
                        log.Warn(String.Join(", ", translatedLine.References.Select(r => r.ToString("X6"))));
                        Console.WriteLine("WARNING, Forcing repoint of:");
                        Console.WriteLine(translatedLine);
                        Console.WriteLine("This can cause rom corruption, test thoroughly!");
                        Console.WriteLine("Following locations will have their indices changed from {0}", currentPointerValue);
                        Console.WriteLine(String.Join(", ", translatedLine.References.Select(r => r.ToString("X6"))));
                    }
                    else
                    {
                        //If we cannot repoint we have to find a different solution.
                        //we have a non repointable line that is longer than the allotted space. We look further to see if we can make space after.
                        //Mainly by repointing lines that did not need repointing.
                        //If this is not possible an error is thrown, the line will have to fit in the original allotted space.

                        var requiredSpace = translatedLine.TextBytes.Count - translatedLine.AvailableLength;
                        var nextTextAddress = translatedLine.Address + translatedLine.AvailableLength + 1;
                        while (requiredSpace > 0)
                        {
                            var nextLine = translatedLines.FirstOrDefault(l => l.Address == nextTextAddress);
                            if (nextLine == null || !nextLine.CanRepointReference)
                            {
                                log.WarnFormat("Warning, the translated line is too long, {0} bytes extra are required, but there is no text available to repoint afterwards:", requiredSpace);
                                log.Warn(translatedLine);
                                Console.WriteLine("Warning, the translated line is too long, {0} bytes extra are required, but there is no text available to repoint afterwards:", requiredSpace);
                                Console.WriteLine(translatedLine);
                                Console.WriteLine("Actions you can take:");
                                Console.WriteLine("- Check if there is no text directly after this one that is not yet added to the translation list, it is possible this can be moved instead.");
                                Console.WriteLine("- Modify the translation to fit within the allotted space.");
                                Console.WriteLine("- Force repointing by modifying the last digit in the translation file from |0| to |1|");
                                Console.WriteLine("\t\tWARNING: This comes at a risk of rom corruption, please test thoroughly after this.");
                                Console.WriteLine();
                                if (!isDryrun)
                                {
                                    log.Error("Translation will not continue and perform a Dry-Run instead.");
                                    Console.WriteLine("Translation will not continue and perform a Dry-Run instead.");
                                    isDryrun = true;
                                }
                                break; //Breaks out of the while, this line will not be repointed.
                            }
                            requiredSpace -= nextLine.AvailableLength;
                            nextLine.MustRepointReference = true;
                            nextTextAddress = nextLine.Address + nextLine.AvailableLength + 1;
                        }
                    }
                }
            }
        }
    }
}
