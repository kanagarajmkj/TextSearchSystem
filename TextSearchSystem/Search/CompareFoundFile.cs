using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchSystem.Search
{
    public class CompareFoundFile : IEqualityComparer<FoundFile>
    {
        public bool Equals(FoundFile left, FoundFile right)
        {
            return left.Id == right.Id;
        }
        public int GetHashCode(FoundFile file)
        {
            return file.Id.GetHashCode();
        }
    }
}
