using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchSystem.Index
{
    public enum IndexingResult
    {
        Success = 0,
        FileNotFound = 1,
        InvalidFile = 2,
        FileAlreadyIndexed = 3,
        IndexingFailed = 4,
        CorruptIndexFile = 5
    }
}
