using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextSearchSystem.Index;

namespace Index
{
    class Index
    {
        static string IndexFilePath = "index.xml";
        static string FailureMessagePrefix = "Indexing Failed due to";
        static Dictionary<IndexingResult, string> FailureMessages = new Dictionary<IndexingResult, string> {
          { IndexingResult.FileNotFound, "file not found" },
          { IndexingResult.FileAlreadyIndexed, "file already indexed" },
          { IndexingResult.InvalidFile, "invalid text file" },
          { IndexingResult.IndexingFailed, "unknown error" },
          { IndexingResult.CorruptIndexFile, "corrupt index file" },
        };
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Text file not specified");
                return;
            }
            
            IIndexer indexer = new Indexer(IndexFilePath);
            for (int i = 0; i < args.Length; i++)
            {
                var textFilePath = args[i];
                Console.WriteLine(string.Format("{0}. Indexing {1}", i + 1, textFilePath));
                try
                {
                    var result = indexer.Index(textFilePath);
                    if(result == IndexingResult.Success)
                    {
                        Console.WriteLine("\t Indexing Completed\n");
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(string.Format("\t{0} {1}\n", FailureMessagePrefix, FailureMessages[result]));
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(string.Format("\t{0} Exception: {0}, Rolling back index\n", FailureMessagePrefix, ex.Message));
                    indexer.Rollback();
                }
            }
        }
    }
}
