using PkmnAdvanceTranslation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargettedSearch
{
    class Program
    {
        static Int32 Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Pass a rom file, output file, start address (HEX) and length to search for (Base 10 Integer)");
                return 1;
            }
            var rom = new RomDataWrapper(new FileInfo(args[0]));

            var outputFile = new FileInfo(args[1]);

            var from = Int32.Parse(args[2], NumberStyles.HexNumber);
            var distance = Int32.Parse(args[3]);

            var foundPointers = new List<PointerText>();

            for(int i = 0; i < distance; i++)
            {
                var foundPointer = rom.GetOriginalPointerInfo(from + i);
                if (foundPointer.ReferenceCount > 0)
                    foundPointers.Add(foundPointer);
            }

            PointerText.WritePointersToFile(outputFile, foundPointers);

            return 0;
        }
    }
}
