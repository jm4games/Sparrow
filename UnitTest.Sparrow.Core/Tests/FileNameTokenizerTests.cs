namespace Sparrow.Core
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileNameTokenizerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenValueNull()
        {
            new FileNameTokenizer(null);
        }

        [TestMethod]
        public void TokenCountZeroWhenFileNameEmpty()
        {
            FileNameTokenizer tokenizer = new FileNameTokenizer("");

            Assert.AreEqual(tokenizer.OriginalFileName, tokenizer.TokenizedFileName);
            Assert.AreEqual(0, tokenizer.TokenCount, "No tokens created.");
        }

        [TestMethod]
        public void NoTokensCreatedWhenNumbersOrAlphaCharsExist()
        {
            FileNameTokenizer tokenizer = new FileNameTokenizer("^#*@(#*");

            Assert.AreEqual(tokenizer.OriginalFileName, tokenizer.TokenizedFileName);
            Assert.AreEqual(0, tokenizer.TokenCount, "No tokens created.");
        }

        [TestMethod]
        public void TokenMarkerEscapedWhenUsedInOriginalString()
        {
            FileNameTokenizer tokenizer = new FileNameTokenizer("%");

            Assert.AreEqual("%%", tokenizer.TokenizedFileName);
        }

        [TestMethod]
        public void TokenUsedWhenFileNameIsWord()
        {
            const string token = "Test";
            FileNameTokenizer tokenizer = new FileNameTokenizer(token);

            Assert.AreEqual(1, tokenizer.TokenCount);
            Assert.AreEqual(token, tokenizer[0]);
            Assert.AreEqual("%0", tokenizer.TokenizedFileName);
        }

        [TestMethod]
        public void TokenUsedWhenFileNameIsNumber()
        {
            const string token = "Test";
            FileNameTokenizer tokenizer = new FileNameTokenizer(token);

            Assert.AreEqual(1, tokenizer.TokenCount);
            Assert.AreEqual(token, tokenizer[0]);
            Assert.AreEqual("%0", tokenizer.TokenizedFileName);
        }

        [TestMethod]
        public void MultipleTokensCreatedWhenNumbersAndWordsExist()
        {
            const string token = "Test";
            const string token2 = "21";
            FileNameTokenizer tokenizer = new FileNameTokenizer(token + token2);

            Assert.AreEqual(2, tokenizer.TokenCount);
            Assert.AreEqual(token, tokenizer[0]);
            Assert.AreEqual(token2, tokenizer[1]);
            Assert.AreEqual("%0%1", tokenizer.TokenizedFileName);
        }

        [TestMethod]
        public void FileNameInCorrectFormatWhenManySymbolsWordsAndNumbersUsed()
        {
            FileNameTokenizer tokenizer = new FileNameTokenizer("this is *** a t3st. 201 ok!");

            Assert.AreEqual("%0 %1 *** %2 %3%4%5. %6 %7!", tokenizer.TokenizedFileName);
        }

        [TestMethod]
        public void ToStringInCorrectFormat()
        {
            const string token = "Test";
            const string token2 = "21";
            FileNameTokenizer tokenizer = new FileNameTokenizer(token + token2);

            Assert.AreEqual(String.Format("Original: {0}{1}Tokenized: {2}", token + token2, Environment.NewLine, "%0%1"), tokenizer.ToString());
        }

        [TestMethod]
        public void EnumeratorEnumeratesInCurrectOrder()
        {
            const string token = "Test";
            const string token2 = "21";
            FileNameTokenizer tokenizer = new FileNameTokenizer(token + token2);
            string expected = token;

            foreach (string tokenValue in tokenizer)
            {
                Assert.AreEqual(expected, tokenValue);
                expected = token2;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExceptionThrownWhenStartIndexNotValidForTokenSequence()
        {
            FileNameTokenizer tokenizer = new FileNameTokenizer("test");

            tokenizer.GetTokenSequence(-1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExceptionThrownWhenEndIndexNotValidForTokenSequence()
        {
            FileNameTokenizer tokenizer = new FileNameTokenizer("test");

            tokenizer.GetTokenSequence(0, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionThrownWhenStartIndexGreaterThenEndIndexForTokenSequence()
        {
            FileNameTokenizer tokenizer = new FileNameTokenizer("test name");

            tokenizer.GetTokenSequence(1, 0);
        }

        [TestMethod]
        public void TokenSequenceHasAllTokensWhenTokenSequenceRequested()
        {
            const string filename = "File_Name_Test_32B";
            FileNameTokenizer tokenizer = new FileNameTokenizer(filename);

            Assert.AreEqual("File Name Test 32 B", tokenizer.GetTokenSequence(0, 4));
        }

        [TestMethod]
        public void TokenSequenceHasCorrectDelimiterWhenTokenSequenceDelimiterSpecified()
        {
            const string filename = "File_Name_Test";
            FileNameTokenizer tokenizer = new FileNameTokenizer(filename);

            Assert.AreEqual("File*Name*Test", tokenizer.GetTokenSequence(0, 2, "*"));
        }

        [TestMethod]
        public void DefaultTokenDelimiterUsedWhenTokenSequenceSpecifiesNullDelimiter()
        {
            const string filename = "File_Name_Test_32B";
            FileNameTokenizer tokenizer = new FileNameTokenizer(filename);

            Assert.AreEqual("File Name Test 32 B", tokenizer.GetTokenSequence(0, 4, null));
        }

        [TestMethod]
        public void OneTokenReturnedWhenTokenSequencesStartAndEndIndexTheSame()
        {
            const string expected = "test";
            FileNameTokenizer tokenizer = new FileNameTokenizer(expected);

            Assert.AreEqual(expected, tokenizer.GetTokenSequence(0, 0));
        }
    }
}
