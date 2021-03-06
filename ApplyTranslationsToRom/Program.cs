﻿using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
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
        private static Boolean isDryrun = true;
        private static Boolean forceRepointing = false;

        private static int PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} TranslationFile.txt RomToTranslate.gba TranslatedRom.gba Execute Force", Assembly.GetEntryAssembly().CodeBase);
            Console.WriteLine();
            Console.WriteLine("The Translation file format is explained in TranslationFileFormat.txt");
            Console.WriteLine("The RomToTranslate is a GBA rom, this file will NOT be modified, only read.");
            Console.WriteLine("The TranslatedRom is the output GBA rom, this file may not exist yet.");
            Console.WriteLine("Execute is literally the word \"Execute\", if this is not specified a Dry-Run is performed and no output is actually generated.");
            Console.WriteLine("Force is literally the word \"Force\" if this is specified ALL text that needs to be repointed is repointed.");
            Console.WriteLine("Careful with this as unintended pointers could be modified. If possible please avoid this and only use manual overrides.");
            return 1;
        }

        static Int32 Main(string[] args)
        {
            log.InfoFormat("Translation process started, passed args [{0}]", String.Join(" ", args.Select(s => "\"" + s + "\"")));
            if (args.Length < 3)
                return PrintUsage();

            var rom = new RomDataWrapper(new FileInfo(args[0]));

            var translationFileLines = LoadTranslationFileLines(args[1]);
            if (translationFileLines == null)
                return PrintUsage();

            var outputRom = new FileInfo(args[2]);
            
            if (args.Length > 3 && args[3].Equals("Execute", StringComparison.InvariantCultureIgnoreCase))
                isDryrun = false;

            if (args.Length > 4 && args[4].Equals("Force", StringComparison.InvariantCultureIgnoreCase))
                forceRepointing = true;

            log.Info("Preparing translated lines.");
            Console.WriteLine("Preparing translated lines.");
            List<PointerText> translatedLines = PrepareTranslatedLines(translationFileLines);
            Console.WriteLine();

            log.Info("Searching lines that need repointing.");
            Console.WriteLine("Searching lines that need repointing.");
            //If we want to repoint as few lines as possible we must process lines bottom up.
            //This way if a line needs to be repointed, but fits due to the line afterwards being repointed already
            //the line can extend into the space previously occupied by the already repointed line.
            translatedLines = translatedLines.OrderByDescending(t => t.Address).ToList();
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

            ApplyTranslation(rom, translatedLines);

            if(!isDryrun)
            {
                rom.WriteRomToFile(outputRom);
            }

            return 0;
        }

        private static List<PointerText> LoadTranslationFileLines(String translationFileName)
        {
            var translationSourceFile = new FileInfo(translationFileName);
            if (!translationSourceFile.Exists)
            {
                log.FatalFormat("Translation source file {0} does not exist", translationFileName);
                Console.WriteLine("Translation source file {0} does not exist", translationFileName);
                return null;
            }
            return PointerText.ReadPointersFromFile(translationSourceFile);
        }

        private static List<PointerText> PrepareTranslatedLines(List<PointerText> translationFileLines)
        {
            var translatedLines = new List<PointerText>();

            foreach (var line in translationFileLines)
            {
                if (!line.IsTranslated)
                {
                    line.TranslatedSingleLine = line.UntranslatedSingleLine;
                    if (line.ReferenceCount == 0)
                        line.AvailableLength = line.TranslatedSingleLineBytes.Length;
                    log.Info("Using original language text for line because no translation was provided:");
                    log.Info(line);
                }
                translatedLines.Add(line);
            }
            
            return translatedLines;
        }

        private static void RepointLinesIfRequired(List<PointerText> translatedLines)
        {
            foreach (var translatedLine in translatedLines.Where(l => l.TranslatedSingleLineBytes.Length > l.AvailableLength))
            {
                if (translatedLine.CanRepointReference)
                {
                    //If we can repoint we only check if we really need to.
                    var requiredSpace = translatedLine.TranslatedSingleLineBytes.Length - translatedLine.AvailableLength;
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
                    if (translatedLine.ReferenceCount == 0)
                        throw new Exception("Cannot repoint a line with 0 references, this is probably a table entry and not a string that is referenced from elsewhere. stay within the size limit.");
                    if (translatedLine.ForceRepointReference || forceRepointing)
                    {
                        translatedLine.ForceRepointReference = true; //in case the global forcerepoint was set, this way we can identify the lines that were forced to repoint later on.
                        var currentPointerValue = String.Format("{0:X6}", translatedLine.Address);
                        currentPointerValue = currentPointerValue.Substring(4) + currentPointerValue.Substring(2, 2) + currentPointerValue.Substring(0, 2) + "08";

                        translatedLine.MustRepointReference = true;
                        log.Warn("Forcing repoint of:");
                        log.Warn(translatedLine);
                        log.Warn("This can cause rom corruption, test thoroughly!");
                        log.WarnFormat("Following locations will have their indices changed from {0}: {1}", currentPointerValue, String.Join(", ", translatedLine.References.Select(r => r.Address.ToString("X6"))));
                        Console.WriteLine("WARNING, Forcing repoint of:");
                        Console.WriteLine(translatedLine);
                        Console.WriteLine("This can cause rom corruption, test thoroughly!");
                        Console.WriteLine("Following locations will have their indices changed from {0}: {1}", currentPointerValue, String.Join(", ", translatedLine.References.Select(r => r.Address.ToString("X6"))));
                    }
                    else
                    {
                        //If we cannot repoint we have to find a different solution.
                        //we have a non repointable line that is longer than the allotted space. We look further to see if we can make space after.
                        //Mainly by repointing lines that did not need repointing.
                        //If this is not possible an error is thrown, the line will have to fit in the original allotted space.

                        var requiredSpace = translatedLine.TranslatedSingleLineBytes.Length - translatedLine.AvailableLength;
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

        private static void ApplyTranslation(RomDataWrapper rom, List<PointerText> translatedLines)
        {
            var repointLocation = Int32.Parse(ConfigurationManager.AppSettings["EmptySpacestart"], NumberStyles.HexNumber);

            foreach(var translatedLine in translatedLines.Where(l => l.MustRepointReference))
            {
                rom.ClearByteRange(translatedLine.Address, translatedLine.AvailableLength);
                rom.ModifyTextReferences(repointLocation, translatedLine.References);                
                rom.WriteBytes(repointLocation, translatedLine.TranslatedSingleLineBytes);
                repointLocation += translatedLine.TranslatedSingleLineBytes.Length + 1;
            }
            foreach(var translatedLine in translatedLines.Where(l => !l.MustRepointReference))
            {
                rom.WriteBytes(translatedLine.Address, translatedLine.TranslatedSingleLineBytes);                
                rom.ClearByteRange(translatedLine.Address + translatedLine.TranslatedSingleLineBytes.Length, 1);                
            }
        }

        
    }
}
