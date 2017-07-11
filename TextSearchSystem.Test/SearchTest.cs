using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextSearchSystem.Test.Helpers;
using TextSearchSystem.Search;

namespace TextSearchSystem.Test
{
    [TestClass]
    public class SearchTest
    {
        [TestMethod]
        public void Search_ParseInput()
        {
            var indexFileName = "Seach_TestIndex.xml";
            var testData = TestHelper.GetInputParsingTestData();
            ISearch search = new Search.Search(indexFileName);
            foreach(var data in testData)
            {
                var inputs = search.ParseSearchInput(data.Item1);
                Assert.AreEqual(data.Item2.Count, inputs.Count);
                for (int i = 0; i < inputs.Count; i++)
                {
                    Assert.AreEqual(data.Item2[i].SearchTerm, inputs[i].SearchTerm);
                    Assert.AreEqual(data.Item2[i].Type, inputs[i].Type);
                }
            }
        }
        [TestMethod]
        public void Search_Word()
        {
            var indexFileName = "Seach_TestIndex.xml";
            TestHelper.CopyFile(TestHelper.TestDataPath + indexFileName, indexFileName);
            ISearch search = new Search.Search(indexFileName);

            var result = search.SearchByWord("modules");
            Assert.IsTrue(result.IsFound);
            Assert.AreEqual(1, result.FoundInFiles.Count);
            Assert.AreEqual("test3.txt", result.FoundInFiles[0].Name);

            TestHelper.DeleteFile(indexFileName);
        }

        [TestMethod]
        public void Search_Expression()
        {
            var indexFileName = "Seach_TestIndex.xml";
            TestHelper.CopyFile(TestHelper.TestDataPath + indexFileName, indexFileName);
            ISearch search = new Search.Search(indexFileName);

            var inputs = search.ParseSearchInput(new string[] { "the", "AND", "printing" });

            var result = search.SearchByExpression(inputs[0]);
            Assert.IsTrue(result.IsFound);
            Assert.AreEqual(2, result.FoundInFiles.Count);
            Assert.AreEqual("test2.txt", result.FoundInFiles[0].Name);

            TestHelper.DeleteFile(indexFileName);
        }

        [TestMethod]
        public void Search_Phrase()
        {
            var indexFileName = "Seach_TestIndex.xml";
            TestHelper.CopyFile(TestHelper.TestDataPath + indexFileName, indexFileName);
            ISearch search = new Search.Search(indexFileName);

            var inputs = search.ParseSearchInput(new string[] { "what is the" });

            var result = search.SearchByPhrase(inputs[0].SearchTerm);
            Assert.IsTrue(result.IsFound);
            Assert.AreEqual(2, result.FoundInFiles.Count);
            Assert.AreEqual("test1.txt", result.FoundInFiles[1].Name);

            TestHelper.DeleteFile(indexFileName);
        }
    }
}
