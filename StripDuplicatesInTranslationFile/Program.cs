using PkmnAdvanceTranslation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StripDuplicatesInTranslationFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var pointers = LoadTranslationFileLines(args[0]);
            var strippedPointers = new Dictionary<Int32, PointerText>();
            foreach(var pointer in pointers)
            {
                if(strippedPointers.ContainsKey(pointer.Address))
                {
                    Console.WriteLine("Double pointer found at address {0:X6}", pointer.Address);
                    if (String.IsNullOrEmpty(strippedPointers[pointer.Address].TranslatedSingleLine) && !String.IsNullOrEmpty(pointer.TranslatedSingleLine))
                    {
                        strippedPointers[pointer.Address].TranslatedSingleLine = pointer.TranslatedSingleLine;
                    }
                }
                else
                {
                    strippedPointers.Add(pointer.Address, pointer);
                }
            }
            var outputFile = new FileInfo(args[1]);
            PointerText.WritePointersToFile(outputFile, strippedPointers.Values.OrderBy(p => p.Address));

            Console.WriteLine("Done...");
            Console.ReadLine();
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
    }
}
