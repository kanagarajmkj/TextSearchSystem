using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchSystem.Index
{

    class WordInStream
    {
        public string Word;
        public long Index;
    }

    class IndexedWordBuffer
    {
        public string Word;
        public List<long> Indexes;
    }

    class IndexedWord
    {
        public IndexedWord() { }
        public IndexedWord(IndexedWordBuffer wordBuffer)
        {
            Word = wordBuffer.Word;
            ContainedInFiles = new List<List<long>>();
            ContainedInFiles.Add(wordBuffer.Indexes);
        }
        public string Word;
        public List<List<long>> ContainedInFiles;
    }
}
