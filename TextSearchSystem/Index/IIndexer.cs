using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchSystem.Index
{
    public interface IIndexer
    {
        IndexingResult Index(string filePath);
        void Commit();
        void Rollback();
    }
}
