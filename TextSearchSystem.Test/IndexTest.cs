using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextSearchSystem.Index;
using TextSearchSystem.Search;
using TextSearchSystem.Test.Helpers;

namespace TextSearchSystem.Test
{
    [TestClass]
    public class IndexTest
    {
        [TestMethod]
        public void Index_NewFile()
        {
            var indexFileName = "Index_NewFile.xml";
            TestHelper.DeleteFile(indexFileName);

            IIndexer indexer = new Indexer(indexFileName);
            var result = indexer.Index(TestHelper.TestDataPath + "test1.txt");

            Assert.AreEqual(IndexingResult.Success, result);
            Assert.IsTrue(File.Exists(indexFileName));

            ISearch search = new Search.Search(indexFileName);
            var searchResult = search.SearchByWord("what");

            Assert.IsTrue(searchResult.IsFound);
            Assert.AreEqual(1, searchResult.FoundInFiles.Count);
            Assert.AreEqual("test1.txt", searchResult.FoundInFiles[0].Name);
            Assert.AreEqual(3, searchResult.FoundInFiles[0].Positions[0]);
            Assert.AreEqual(221, searchResult.FoundInFiles[0].Positions[1]);
            TestHelper.DeleteFile(indexFileName);
        }

        [TestMethod]
        public void Index_ExistingFile()
        {
            var indexFileName = "Index_ExistingFile.xml";
            TestHelper.CopyFile(TestHelper.TestDataPath + indexFileName, indexFileName);

            IIndexer indexer = new Indexer(indexFileName);
            var result = indexer.Index(TestHelper.TestDataPath + "test2.txt");

            Assert.AreEqual(IndexingResult.Success, result);
            Assert.IsTrue(File.Exists(indexFileName));

            result = indexer.Index(TestHelper.TestDataPath + "test3.txt");
            Assert.AreEqual(IndexingResult.Success, result);
            Assert.IsTrue(File.Exists(indexFileName));

            ISearch search = new Search.Search(indexFileName);
            var searchResult = search.SearchByWord("explains");

            Assert.IsTrue(searchResult.IsFound);
            Assert.AreEqual(1, searchResult.FoundInFiles.Count);
            Assert.AreEqual("test3.txt", searchResult.FoundInFiles[0].Name);
            Assert.AreEqual(265, searchResult.FoundInFiles[0].Positions[0]);
            TestHelper.DeleteFile(indexFileName);
        }

        [TestMethod]
        public void Index_BinaryFile()
        {
            var indexFileName = "Index_BinaryFile.xml";

            IIndexer indexer = new Indexer(indexFileName);
            var result = indexer.Index(TestHelper.TestDataPath + "test_binary.txt");

            Assert.AreEqual(IndexingResult.InvalidFile, result);
            Assert.IsFalse(File.Exists(indexFileName));
        }
    }
}
