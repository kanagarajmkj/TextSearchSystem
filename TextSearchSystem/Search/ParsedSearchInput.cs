using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSearchSystem.Search
{
    public class ParsedSearchInput
    {
        public InputType Type;
        public OperatorType Operator;
        public bool IsValid;
        public string SearchTerm;
        public List<ParsedSearchInput> LeftCondition;
        public List<ParsedSearchInput> RightCondition;
        public SearchResult ExecuteSearch(ISearch search)
        {
            switch(Type)
            {
                case InputType.Word:
                    return search.SearchByWord(SearchTerm);
                case InputType.Expression:
                    return search.SearchByExpression(this);
                case InputType.Phrase:
                    return search.SearchByPhrase(SearchTerm);
            }
            return null;
        }
    }
}
