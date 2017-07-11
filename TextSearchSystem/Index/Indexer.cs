using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using TextSearchSystem.Utilities;

namespace TextSearchSystem.Index
{
    public class Indexer : IIndexer
    {
        #region Constructor

        private string IndexFilePath;
        private string IndexFileProcessingPath;
        private int MaxChunkSize;
        private Dictionary<string, IndexedWordBuffer> IndexChunkDic;
        private long CheckTextFileByteLenght = 1024;
        private List<char> Punctuations;
        private List<int> SkipBytes;

        public Indexer(string indexFilePath, int indexChunkSize = 10000, long checkTextFileByteLength = 1024, List<char> punctuations = null)
        {
            IndexFilePath = indexFilePath;
            MaxChunkSize = indexChunkSize;
            IndexChunkDic = new Dictionary<string, IndexedWordBuffer>(MaxChunkSize);
            CheckTextFileByteLenght = checkTextFileByteLength;
            Punctuations = punctuations != null && punctuations.Count > 0 ? punctuations : new List<char> { ',', '.', ':', ';', '?', '!', '_', '-' };
            SkipBytes = Enumerable.Range(0, 32).ToList();
            SkipBytes.AddRange(Enumerable.Range(127, 255));
        }

        #endregion

        #region IIndex Implementation
        public IndexingResult Index(string filePath)
        {
            if (!File.Exists(filePath))
                return IndexingResult.FileNotFound;
            if (!FileUtility.IsTextFile(filePath, CheckTextFileByteLenght))
                return IndexingResult.InvalidFile;
            FileInfo fi = new FileInfo(filePath);
            var fileInIndex = GetFileIdInIndexes(IndexFilePath, fi);
            if (fileInIndex.Item1)
                return IndexingResult.FileAlreadyIndexed;
            using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                var bomLength = sr.CurrentEncoding.GetPreamble().Length;
                sr.BaseStream.Seek(bomLength > 0 ? bomLength - 1 : 0, SeekOrigin.Begin);
                while (sr.BaseStream.Position < sr.BaseStream.Length)
                {
                    var word = ReadWord(sr.BaseStream);
                    if (word.Word.Length > 0)
                        AddWordToChunk(word, fi, fileInIndex.Item2);
                }
            }
            CommitIndexChunk(fi, true);
            Commit();
            return IndexingResult.Success;
        }

        public void Commit()
        {
            if (File.Exists(IndexFileProcessingPath))
            {
                if (File.Exists(IndexFilePath))
                    File.Delete(IndexFilePath);
                File.Move(IndexFileProcessingPath, IndexFilePath);
            }
        }

        public void Rollback()
        {
            if (File.Exists(IndexFileProcessingPath))
                File.Delete(IndexFileProcessingPath);
        }

        #endregion

        #region Processing
        private WordInStream ReadWord(Stream stream)
        {
            long position = -1;
            var ch = stream.ReadByte();
            StringBuilder sb = new StringBuilder();
            while (ch != -1)
            {
                if (ch == ' ' || ch == '\n' || ch == '\r' || Punctuations.Contains((char)ch))
                    break;
                else if (SkipBytes.Contains(ch))
                {
                    ch = stream.ReadByte();
                    continue;
                }
                if (position == -1)
                    position = stream.Position;
                sb.Append((Char)ch);
                ch = stream.ReadByte();
            }
            return new WordInStream() { Word = sb.ToString().ToLower(), Index = position };
        }

        private void AddWordToChunk(WordInStream word, FileInfo fileInfo, long fileId)
        {
            IndexedWordBuffer wordBuffer;
            if (word.Word.Length > 0)
            {
                if (IndexChunkDic.ContainsKey(word.Word))
                    wordBuffer = IndexChunkDic[word.Word];
                else
                    wordBuffer = new IndexedWordBuffer() { Word = word.Word };

                if (wordBuffer.Indexes == null)
                {
                    wordBuffer.Indexes = new List<long>();
                    wordBuffer.Indexes.Add(fileId);
                }
                wordBuffer.Indexes.Add(word.Index);
                IndexChunkDic[word.Word] = wordBuffer;
            }
            CommitIndexChunk(fileInfo);
        }

        private void CommitIndexChunk(FileInfo fileInfo, bool force = false)
        {
            if (force || IndexChunkDic.Count >= MaxChunkSize)
            {
                var indexedWords = new List<IndexedWord>();
                foreach (var wordKey in IndexChunkDic.Keys)
                {
                    var wordBuff = IndexChunkDic[wordKey];
                    var indexedWord = new IndexedWord(wordBuff);
                    indexedWords.Add(indexedWord);
                }
                SaveIndexes(indexedWords, fileInfo);
                IndexChunkDic = new Dictionary<string, IndexedWordBuffer>();
            }
        }

        private Tuple<bool, long> GetFileIdInIndexes(string indexFilePath, FileInfo fileInfo)
        {
            bool isFileExistsInIndex = false;
            if (!File.Exists(IndexFilePath))
            {
                CreateIndexFile(IndexFilePath);
            }

            CreateProcessingFile(IndexFilePath);

            XDocument doc = XDocument.Load(IndexFileProcessingPath);
            var file = doc.Root.Elements(IndexFileConstants.Files).Elements(IndexFileConstants.File).Where(el => (string)el.Element(IndexFileConstants.Path) == fileInfo.FullName).FirstOrDefault();
            long fileId;

            if (file != null)
            {
                isFileExistsInIndex = true;
                fileId = (long)file.Element(IndexFileConstants.Id);
            }
            else
            {
                fileId = doc.Root.Descendants(IndexFileConstants.File).Count();
                var files = doc.Root.Elements(IndexFileConstants.Files).FirstOrDefault();
                files.Add(CreateFileElement(fileInfo, fileId));
            }
            using (var fs = new FileStream(IndexFileProcessingPath, FileMode.Open))
            {
                doc.Save(fs);
            }
            return Tuple.Create<bool, long>(isFileExistsInIndex, fileId);
        }

        private string GetProcessingFilePath(string sourcePath)
        {
            return string.Format(@"{0}\{1}{2}", Path.GetDirectoryName(sourcePath), Path.GetFileNameWithoutExtension(sourcePath) + ProcessingFileConstants.Suffix, Path.GetExtension(sourcePath));
        }

        private void CreateProcessingFile(string sourcePath)
        {
            IndexFileProcessingPath = GetProcessingFilePath(sourcePath);
            if (File.Exists(IndexFileProcessingPath))
                File.Delete(IndexFileProcessingPath);
            File.Copy(sourcePath, IndexFileProcessingPath);
        }

        private void SaveIndexes(List<IndexedWord> wordIndexes, FileInfo fileInfo)
        {
            XDocument doc = XDocument.Load(IndexFileProcessingPath);
            for (int i = 0; i < wordIndexes.Count; i++)
            {
                var word = wordIndexes[i];
                var docIndexes = doc.Root.Element(IndexFileConstants.Indexes);
                var wordElement = doc.Root.Descendants(IndexFileConstants.Indexes).Elements(IndexFileConstants.Index).Where(el => (string)el.Attribute(IndexFileConstants.Word).Value == word.Word).FirstOrDefault();
                if (wordElement != null)
                {
                    var fileIndexes = JsonUtility.Deserialize<List<List<long>>>(wordElement.Value);
                    bool isExisting = false;
                    for (int j = 0; j < fileIndexes.Count; j++)
                    {
                        var fileIndex = fileIndexes[j];
                        if (fileIndex[0] == word.ContainedInFiles[0][0])
                        {
                            word.ContainedInFiles[0].RemoveAt(0);
                            fileIndex.AddRange(word.ContainedInFiles[0]);
                            isExisting = true;
                            break;
                        }
                    }
                    if (!isExisting)
                        fileIndexes.Add(word.ContainedInFiles[0]);
                    wordElement.Value = JsonUtility.Serialize(fileIndexes);
                }
                else
                {
                    docIndexes.Add(CreateIndexElement(word));
                }
            }
            using (var fs = new FileStream(IndexFileProcessingPath, FileMode.Open))
            {
                doc.Save(fs);
            }
        }

        #endregion

        #region XML Element Supporting Methods        
        private void CreateIndexFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                new XDocument(
                    new XElement(IndexFileConstants.IndexRoot,
                       new[] { new XElement(IndexFileConstants.Files, ""), new XElement(IndexFileConstants.Indexes, "") }
                                )
                            ).Save(fs);
            }
        }

        private XElement CreateIndexElement(IndexedWord word)
        {
            var indexElement = new XElement(IndexFileConstants.Index, JsonUtility.Serialize(word.ContainedInFiles));
            indexElement.Add(new XAttribute(IndexFileConstants.Word, word.Word));
            return indexElement;
        }

        private XElement CreateFileElement(FileInfo fileInfo, long id)
        {
            return new XElement(IndexFileConstants.File, CreateFileElements(fileInfo, id));
        }
        private List<XElement> CreateFileElements(FileInfo fileInfo, long id)
        {
            var fileName = fileInfo.Name;
            var filePath = fileInfo.FullName;
            return new List<XElement>
            {
                new XElement(IndexFileConstants.Id, id),
                new XElement(IndexFileConstants.Name, fileName),
                new XElement(IndexFileConstants.Path, filePath),
            };
        }

        #endregion

    }

}
