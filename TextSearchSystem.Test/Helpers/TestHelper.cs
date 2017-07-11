using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TextSearchSystem.Search;

namespace TextSearchSystem.Test.Helpers
{
    static class TestHelper
    {

        public static string TestDataPath = @"..\..\TestData\";
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public static void CopyFile(string source, string target)
        {
            DeleteFile(target);
            if (File.Exists(source))
                File.Copy(source, target);
        }

        public static List<Tuple<string[], List<ParsedSearchInput>>> GetInputParsingTestData()
        {
            var data = new List<Tuple<string[], List<ParsedSearchInput>>>();
            data.Add(Tuple.Create<string[], List<ParsedSearchInput>>(new string[] { "what", "the" }, new List<ParsedSearchInput>() {
                new ParsedSearchInput{ SearchTerm = "what", Type = InputType.Word, IsValid = true },
                new ParsedSearchInput{ SearchTerm = "the", Type = InputType.Word, IsValid = true },
            }));

            data.Add(Tuple.Create<string[], List<ParsedSearchInput>>(new string[] { "what is the" }, new List<ParsedSearchInput>() {
                new ParsedSearchInput{ SearchTerm = "what is the", Type = InputType.Phrase},
            }));

            data.Add(Tuple.Create<string[], List<ParsedSearchInput>>(new string[] { "the", "AND", "printing" }, new List<ParsedSearchInput>() {
                new ParsedSearchInput{ SearchTerm = "the AND printing", Type = InputType.Expression, IsValid = true,
                    LeftCondition =  new List<ParsedSearchInput>{ new ParsedSearchInput { SearchTerm = "the", Type = InputType.Word} },
                    RightCondition =  new List<ParsedSearchInput>{ new ParsedSearchInput { SearchTerm = "printing", Type = InputType.Word} },
                    },
            }));

            return data;
        }
    }
}
