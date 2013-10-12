using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sparrow.Core
{
    [TestClass]
    public class AbstractStringTokenizerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenValueNull()
        {
            new AbstractStringTokenizer(null);
        }

        [TestMethod]
        public void EmptyAbstractStringWhenOriginalStringEmpty()
        {
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer("");

            Assert.AreEqual(tokenizer.OriginalString, tokenizer.AbstractString);
            Assert.AreEqual(0, tokenizer.TokenCount, "No tokens created.");
        }

        [TestMethod]
        public void NoTokensCreatedWhenNumbersOrAlphaCharsExist()
        {
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer("^#*@(#*");

            Assert.AreEqual(tokenizer.OriginalString, tokenizer.AbstractString);
            Assert.AreEqual(0, tokenizer.TokenCount, "No tokens created.");
        }

        [TestMethod]
        public void TokenMarkerEscapedWhenUsedInOriginalString()
        {
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer("%");

            Assert.AreEqual("%%", tokenizer.AbstractString);
        }

        [TestMethod]
        public void TokenUsedWhenStringIsWord()
        {
            const string token = "Test";
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer(token);

            Assert.AreEqual(1, tokenizer.TokenCount);
            Assert.AreEqual(token, tokenizer[0]);
            Assert.AreEqual("%0", tokenizer.AbstractString);
        }

        [TestMethod]
        public void TokenUsedWhenStringIsNumber()
        {
            const string token = "Test";
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer(token);

            Assert.AreEqual(1, tokenizer.TokenCount);
            Assert.AreEqual(token, tokenizer[0]);
            Assert.AreEqual("%0", tokenizer.AbstractString);
        }

        [TestMethod]
        public void MultipleTokensCreatedWhenNumbersAndWordsExist()
        {
            const string token = "Test";
            const string token2 = "21";
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer(token + token2);

            Assert.AreEqual(2, tokenizer.TokenCount);
            Assert.AreEqual(token, tokenizer[0]);
            Assert.AreEqual(token2, tokenizer[1]);
            Assert.AreEqual("%0%1", tokenizer.AbstractString);
        }

        [TestMethod]
        public void AbstractStringInCorrectFormatWhenManySymbolsWordsAndNumbersUsed()
        {
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer("this is *** a t3st. 201 ok!");

            Assert.AreEqual("%0 %1 *** %2 %3%4%5. %6 %7!", tokenizer.AbstractString);
        }

        [TestMethod]
        public void ToStringInCorrectFormat()
        {
            const string token = "Test";
            const string token2 = "21";
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer(token + token2);

            Assert.AreEqual(String.Format("Original: {0}{1}Abstract: {2}", token + token2, Environment.NewLine, "%0%1"), tokenizer.ToString());
        }

        [TestMethod]
        public void EnumeratorEnumeratesInCurrectOrder()
        {
            const string token = "Test";
            const string token2 = "21";
            AbstractStringTokenizer tokenizer = new AbstractStringTokenizer(token + token2);
            string expected = token;

            foreach (string tokenValue in tokenizer)
            {
                Assert.AreEqual(expected, tokenValue);
                expected = token2;
            }
        }
    }
}
