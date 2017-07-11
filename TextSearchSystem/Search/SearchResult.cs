using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchSystem.Search
{
    public class SearchResult
    {
        public string WordOrPhrase;
        public bool IsFound;
        public List<FoundFile> FoundInFiles;
        public SearchResult(string wordOrPhrase, bool isFound, List<FoundFile> foundInFiles)
        {
            WordOrPhrase = wordOrPhrase;
            IsFound = isFound;
            FoundInFiles = foundInFiles;
            if (isFound)
                SortFiles();
        }

        public void SortFiles()
        {
            FoundInFiles.Sort((a, b) => -1 * ((int)a.Count - (int)b.Count));
        }
    }

}
