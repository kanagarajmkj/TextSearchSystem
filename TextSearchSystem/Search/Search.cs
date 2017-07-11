using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using TextSearchSystem.Utilities;
using TextSearchSystem.Index;

namespace TextSearchSystem.Search
{
    public class Search: ISearch
    {
        #region Constructor

        private string IndexFilePath;
        private List<string> ConditionalOperators;

        public Search(string indexFilePath)
        {
            IndexFilePath = indexFilePath;
            ConditionalOperators = new List<string> { OperatorConstants.AND, OperatorConstants.OR, OperatorConstants.NOT };
        }

        #endregion

        #region ISearch Implementation

        public List<ParsedSearchInput> ParseSearchInput(string[] inputs)
        {
            var result = new List<ParsedSearchInput>();
            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i].Trim();
                if (input.Contains(' '))
                {
                    result.Add(new ParsedSearchInput { SearchTerm = input, Type = InputType.Phrase });
                    continue;
                }
                if (ConditionalOperators.Contains(input))
                {
                    result.Clear();
                    OperatorType type = OperatorType.And;
                    switch (input)
                    {
                        case OperatorConstants.AND:
                            type = OperatorType.And;
                            break;
                        case OperatorConstants.OR:
                            type = OperatorType.Or;
                            break;
                        case OperatorConstants.NOT:
                            type = OperatorType.Not;
                            break;
                    }
                    result.Add(PopulateConditionalSearchInput(type, ParseSearchInput(inputs.Take(i).ToArray()), ParseSearchInput(inputs.Skip(i + 1).ToArray()), string.Join(" ", inputs)));
                    return result;
                }
                else
                {
                    result.Add(new ParsedSearchInput { SearchTerm = input, Type = InputType.Word });
                }
            }
            return result;
        }

        public List<SearchResult> SearchBySentence(string input)
        {
            var words = input.Split(' ');
            List<SearchResult> result = new List<SearchResult>();
            for(int i=0;i<words.Length;i++)
            {
                result.Add(SearchByWord(words[i]));
            }
            return result;
        }
        
        public SearchResult SearchByWord(string word)
        {
            XDocument doc = XDocument.Load(IndexFilePath);
            var containingFiles = GetFileIndexListByWord(doc, word);
            if(containingFiles != null)
            {
               var foundFiles = PopulateFoundFiles(doc, containingFiles);
                if (foundFiles != null && foundFiles.Count > 0)
                    return new SearchResult(word, true, foundFiles);
            }
            return new SearchResult(word, false, null);
        }

        public SearchResult SearchByExpression(ParsedSearchInput input)
        {
            var leftResult = SearchConditions(input.LeftCondition);
            var rightResult = SearchConditions(input.RightCondition);
            var leftFiles = GetFilesFromSearchResult(leftResult);
            var rightFiles = GetFilesFromSearchResult(rightResult);
            var containingFiles = new List<FoundFile>();
            switch (input.Operator)
            {
                case OperatorType.And:
                    containingFiles = leftFiles.Intersect(rightFiles, new CompareFoundFile()).ToList();
                    break;
                case OperatorType.Or:
                    leftFiles.AddRange(rightFiles);
                    containingFiles = leftFiles.Distinct(new CompareFoundFile()).ToList();
                    break;
                case OperatorType.Not:
                    leftFiles.RemoveAll(l => rightFiles.Any(r => r.Id == l.Id));
                    containingFiles = leftFiles;
                    break;
            }
            return containingFiles.Count > 0 ? new SearchResult(input.SearchTerm, true, containingFiles) : new SearchResult(input.SearchTerm, false, null);
        }

        public SearchResult SearchByPhrase(string input)
        {
            var words = input.Replace(new string(PhraseConstants.PhraseEnclosing, 1), "").Split(' ');
            var results = new List<SearchResult>();
            if (words.Length > 0)
            {
                if (words.Length == 1)
                    return SearchByWord(words[0]);
                for (int i = 0; i < words.Length; i++)
                {
                    results.Add(SearchByWord(words[i]));
                }
                if (results.Count > 0)
                {
                    if (results.All(r => r.IsFound))
                    {
                        var distinctFiles = new Dictionary<long, List<Tuple<string, FoundFile>>>();
                        foreach (var result in results)
                        {
                            foreach (var file in result.FoundInFiles)
                            {
                                if (!distinctFiles.ContainsKey(file.Id))
                                    distinctFiles.Add(file.Id, new List<Tuple<string, FoundFile>>());
                                distinctFiles[file.Id].Add(Tuple.Create<string, FoundFile>(result.WordOrPhrase, file));
                            }
                        }
                        var finalResult = new SearchResult(input, true, new List<FoundFile>());
                        foreach (var fileId in distinctFiles.Keys)
                        {
                            var fileWords = distinctFiles[fileId];
                            if (fileWords.Count == words.Length)
                            {
                                List<long> foundPositions = new List<long>();
                                var wordPositions = fileWords[0].Item2.Positions;
                                foreach (var position in wordPositions)
                                {
                                    var targetPosition = position + fileWords[0].Item1.Length * 2 + 2;
                                    var queue = GetQueue(fileWords);
                                    bool isNotFound = false;
                                    while (queue.Count > 0)
                                    {
                                        var nextWord = queue.Dequeue();
                                        if (nextWord.Item2.Positions.Contains(targetPosition))
                                        {
                                            targetPosition += nextWord.Item1.Length * 2 + 2;
                                            continue;
                                        }
                                        else
                                        {
                                            isNotFound = true;
                                            break;
                                        }
                                    }
                                    if (isNotFound == false)
                                    {
                                        foundPositions.Add(position);
                                    }
                                }

                                if (foundPositions.Count > 0)
                                    finalResult.FoundInFiles.Add(new FoundFile() { Id = fileWords[0].Item2.Id, Name = fileWords[0].Item2.Name, Count = foundPositions.Count, Positions = foundPositions });
                            }
                        }
                        if (finalResult.FoundInFiles.Count > 0)
                        {
                            finalResult.SortFiles();
                            return finalResult;
                        }
                    }
                }
            }
            return new SearchResult(input, false, null);
        }

        #endregion

        #region Supporting private methods

        private List<FoundFile> GetFilesFromSearchResult(List<SearchResult> searchResults)
        {
            var resultFiles = new List<FoundFile>();
            for (int i = 0; i < searchResults.Count; i++)
            {
                var result = searchResults[i];
                if(result.IsFound)
                resultFiles.AddRange(result.FoundInFiles);
            }
            return resultFiles;
        }

        private List<SearchResult> SearchConditions(List<ParsedSearchInput> inputs)
        {
            List<SearchResult> result = new List<SearchResult>();
            for(int i=0;i<inputs.Count;i++)
            {
                var input = inputs[i];
                switch(input.Type)
                {
                    case InputType.Word:
                        result.Add(SearchByWord(input.SearchTerm));
                        break;
                    case InputType.Phrase:
                        result.Add(SearchByPhrase(input.SearchTerm));
                        break;
                    case InputType.Expression:
                        result.Add(SearchByExpression(input));
                        break;
                }
            }
            return result;
        }

        private Queue<Tuple<string, FoundFile>> GetQueue(List<Tuple<string, FoundFile>> fileWords)
        {
            Queue<Tuple<string, FoundFile>> queue = new Queue<Tuple<string, FoundFile>>();
            for (int i=1;i<fileWords.Count;i++)
                queue.Enqueue(fileWords[i]);
            return queue;
        }

        private ParsedSearchInput PopulateConditionalSearchInput(OperatorType type, List<ParsedSearchInput> left, List<ParsedSearchInput> right, string searchTerm)
        {
            return new ParsedSearchInput { Type = InputType.Expression, Operator = type, LeftCondition = left, RightCondition = right, SearchTerm = searchTerm };
        }

        private List<FoundFile> PopulateFoundFiles(XDocument doc, List<List<long>> containingFiles)
        {
            if(containingFiles.Count > 0)
            {
                var foundFiles = new List<FoundFile>();
                for (int i = 0; i < containingFiles.Count; i++)
                {
                    var containingFile = containingFiles[i];
                    var fileId = containingFile[0];
                    containingFile.RemoveAt(0);
                    foundFiles.Add(new FoundFile() {Id = fileId, Name = GetFileNameById(doc, fileId), Count = containingFile.Count - 1, Positions = containingFile });
                }
                return foundFiles;
            }
            return null;
        }

        private List<List<long>> GetFileIndexListByWord(XDocument doc, string word)
        {
            var wordElement = doc.Root.Descendants(IndexFileConstants.Indexes).Elements(IndexFileConstants.Index).Where(el => (string)el.Attribute(IndexFileConstants.Word).Value == word).FirstOrDefault();
            if (wordElement != null)
            {
                return JsonUtility.Deserialize<List<List<long>>>(wordElement.Value);
            }
            return null;
        }

        private string GetFileNameById(XDocument doc, long id)
        {
            var file = doc.Root.Elements(IndexFileConstants.Files).Elements(IndexFileConstants.File).Where(el => (long)el.Element(IndexFileConstants.Id) == id).FirstOrDefault();
            return file != null ? file.Element(IndexFileConstants.Name).Value : null;
        }

        #endregion
    }
}
