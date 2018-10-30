using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceTextIndexes
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
                throw new Exception("Give source translation file, target index file and output translation file names.");

            var indexSourceFile = new FileInfo(args[0]);
            var indexTargetFile = new FileInfo(args[1]);
            var combinedFile = new FileInfo(args[2]);

            if (!indexSourceFile.Exists)
                throw new Exception("Index source file does not exist.");
            if(!indexTargetFile.Exists)
                throw new Exception("Index target file does not exist.");
            if (combinedFile.Exists)
                throw new Exception("The combined file exists. This has to be a new file.");

            using (var sourceReader = new StreamReader(indexSourceFile.OpenRead(), Encoding.GetEncoding(1252)))
            using (var targetReader = new StreamReader(indexTargetFile.OpenRead(), Encoding.GetEncoding(1252)))
            using (var writer = new StreamWriter(combinedFile.OpenWrite(), Encoding.GetEncoding(1252)))
            {
                var stop = false;
                while (!stop)
                {
                    var sourceLine = sourceReader.ReadLine();
                    var targetLine = targetReader.ReadLine();
                    if (sourceLine != null && targetLine != null)
                    {
                        var sourceEntries = sourceLine.Split('|');
                        var targetEntries = targetLine.Split('|');
                        if (sourceEntries[6] != targetEntries[6])
                        {
                            targetEntries[7] = sourceEntries[6];
                        }
                        writer.WriteLine( String.Join("|", targetEntries));
                    }
                    else
                    {
                        stop = true;
                    }
                }
            }
        }
    }
}
