using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextSearchSystem.Search;

namespace Search
{
    class Search
    {
        static string IndexFilePath = "index.xml";
        static string ErrorMessageTemplate = "Exception: {0}";
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("No search term entered\n");
                return;
            }
            ISearch search = new TextSearchSystem.Search.Search(IndexFilePath);
            var parsedInputs = new List<ParsedSearchInput>();
            try
            {
                parsedInputs = search.ParseSearchInput(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format(ErrorMessageTemplate, ex.Message));
                return;
            }
            Console.WriteLine(string.Format("$ Search {0}", string.Join(" ", args)));
            for (int i = 0; i < parsedInputs.Count; i++)
            {
                var input = parsedInputs[i];
                Console.WriteLine(string.Format("{0}. Searching for '{1}'...", i + 1, input.SearchTerm));
                try
                {
                    PrintResult(input.ExecuteSearch(search));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(string.Format(ErrorMessageTemplate, ex.Message));
                    return;
                }
            }
        }

        static void PrintResult(SearchResult result)
        {
            if (result != null)
            {
                if (result.IsFound)
                {
                    Console.WriteLine("   Found in:\n");
                    foreach (var file in result.FoundInFiles)
                        Console.WriteLine(string.Format("   \t{0} ({1})", file.Name, file.Count));
                    Console.Write("\n");
                }
                else
                {
                    Console.WriteLine("   No matches found.\n\n");
                }
            }
        }
    }
}
