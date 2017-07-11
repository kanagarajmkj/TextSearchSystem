using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchSystem.Search
{
    public interface ISearch
    {
        List<ParsedSearchInput> ParseSearchInput(string[] inputs);
        List<SearchResult> SearchBySentence(string input);
        SearchResult SearchByWord(string input);
        SearchResult SearchByExpression(ParsedSearchInput input);
        SearchResult SearchByPhrase(string input);
    }
}
