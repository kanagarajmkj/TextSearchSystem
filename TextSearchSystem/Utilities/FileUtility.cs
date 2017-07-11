using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TextSearchSystem.Utilities
{
   internal static class FileUtility
    {
        public static bool IsTextFile(string filePath, long checkBytesLength)
        {
            bool isBinary = false;
            using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                checkBytesLength = sr.BaseStream.Length > checkBytesLength ? checkBytesLength : sr.BaseStream.Length;
                int maxBomLength = 6;
                sr.BaseStream.Seek(maxBomLength, SeekOrigin.Begin);
                for (int i = maxBomLength; i < checkBytesLength; i++)
                {
                    int b = sr.BaseStream.ReadByte();
                    if (!(b >= -1 && b <= 127))
                    {
                        isBinary = true;
                        break;
                    }
                }
            }
            return !isBinary;
        }
    }
}
