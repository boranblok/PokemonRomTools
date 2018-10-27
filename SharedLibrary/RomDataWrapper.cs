using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PkmnAdvanceTranslation
{
    public class RomDataWrapper
    {
        private static readonly Byte end = 0xFF;
        private static readonly Int32 stringPointerInt = 0x08000000;

        public Byte[] RomContents { get; private set; }

        public RomDataWrapper(FileInfo romFile)
        {
            LoadRomContents(romFile);
        }

        private void LoadRomContents(FileInfo romFile)
        {
            if (!romFile.Exists)
                throw new Exception(String.Format("Rom file {0} does not exist.", romFile.FullName));

            RomContents = new Byte[romFile.Length];
            using (var writer = new MemoryStream(RomContents))
            using (var reader = romFile.OpenRead())
            {
                reader.CopyTo(writer);
            }
        }

        public void WriteRomToFile(FileInfo file)
        {
            if (file.Exists)
                throw new Exception(String.Format("Rom file {0} already not exist.", file.FullName));
            using (var reader = new MemoryStream(RomContents))
            using (var writer = file.OpenWrite())
            {
                reader.CopyTo(writer);
            }
        }

        public PointerText GetTextAtPointer(Int32 textPointer)
        {
            var result = new PointerText();
            result.Address = textPointer;            
            result.References = FindTextReferences(textPointer);

            bool endFound = false;
            int overFlowProtection = 1000; //Max string length of 1000 to prevent an overflow into binary data.
            int index = textPointer;
            var textBytes = new List<Byte>();
            while (!endFound && overFlowProtection-- > 0 && index < RomContents.Length)
            {
                var value = RomContents[index];
                if (value == end)
                {
                    result.AvailableLength = textBytes.Count;
                    endFound = true;
                }
                else
                {
                    textBytes.Add(value);
                }
                index++;
            }
            result.TextBytes = textBytes.AsReadOnly();
            return result;
        }

        public void WriteBytes(Int32 index, Byte[] bytesToWrite)
        {
            for (int i = 0; i < bytesToWrite.Length; i++)
            {
                RomContents[i+index] = bytesToWrite[i];
            }
        }

        public void ModifyTextReferences(Int32 newTextPointer, List<Int32> references)
        {
            var newTextPointerValue = stringPointerInt + newTextPointer;
            var newTextPointerValueBytes = BitConverter.GetBytes(newTextPointerValue);
            foreach(var reference in references)
            {
                WriteBytes(reference, newTextPointerValueBytes);
            }
        }

        public Int32 GetAvailableTextLength(Int32 textPointer)
        {
            var byteLength = 0;
            bool endFound = false;
            int overFlowProtection = 1000; //Max string length of 1000 to prevent an overflow into binary data.
            int index = textPointer;
            while (!endFound && overFlowProtection-- > 0 && index < RomContents.Length)
            {
                if (RomContents[index] == end)
                {
                    byteLength = index - textPointer - 1;
                    endFound = true;
                }
            }
            return byteLength;
        }

        public List<Int32> FindTextReferences(Int32 textPointer)
        {
            var textPointerValue = stringPointerInt + textPointer;
            var textPointerBytes = BitConverter.GetBytes(textPointerValue);
            var result = ByteBinarySearcher.FindMatches(textPointerBytes, RomContents);
            return result;
        }

        public void ClearByteRange(Int32 start, Int32 length)
        {
            for(int i = start; i < start + length; i++)
            {
                RomContents[i] = end;
            }
        }
    }
}
