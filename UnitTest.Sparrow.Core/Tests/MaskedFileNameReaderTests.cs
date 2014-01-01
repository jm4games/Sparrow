namespace Sparrow
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class MaskedFileNameReaderTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenMaskedFileNameNull()
        {
            new MaskedFileNameReader<TestMask>(null);
        }

        [TestMethod]
        public void EndOfMaskedFileNameTrueWhenMaskedFileNameHasNoMasks()
        {
            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(MaskedFileNameFactory.CreateWithEmptyFileName());

            Assert.IsTrue(reader.EndOfMaskedFileName);
        }

        [TestMethod]
        public void EndOfMaskedFileNameTrueWhenAllValuesHaveBeenRead()
        {
            MaskedFileName<TestMask> maskedFileName = MaskedFileNameFactory.CreateWithFileName("Some Test");

            maskedFileName.SetTokenMask(0, TestMask.Any);
            maskedFileName.SetTokenMask(1, TestMask.Alpha);

            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(maskedFileName);
            TestMask mask;

            reader.ReadNextMaskValue(out mask);
            reader.ReadNextMaskValue(out mask);

            Assert.IsTrue(reader.EndOfMaskedFileName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionThrownWhenAllMasksHaveAlreadyBeenRead()
        {
            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(MaskedFileNameFactory.CreateWithEmptyFileName());
            TestMask mask;

            reader.ReadNextMaskValue(out mask);
        }

        [TestMethod]
        public void ReadValueHasAllTokensInOneSequenceWhenTokensHaveSameMask()
        {
            const string expected = "Some Test File";
            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(MaskedFileNameFactory.CreateWithFileName(expected));
            TestMask mask;

            Assert.AreEqual(expected, reader.ReadNextMaskValue(out mask));
        }

        [TestMethod]
        public void ReadValueHasTokensForCurrentMaskSequenceOnlyWhenMultipleMasksExist()
        {
            MaskedFileName<TestMask> maskedFileName = MaskedFileNameFactory.CreateWithFileName("Some Test File");
            
            maskedFileName.SetTokenMask(0, TestMask.Any);
            maskedFileName.SetTokenMask(1, TestMask.Alpha);
            maskedFileName.SetTokenMask(2, TestMask.Any);

            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(maskedFileName);
            TestMask mask;

            Assert.AreEqual("Some", reader.ReadNextMaskValue(out mask));
            Assert.AreEqual("Test", reader.ReadNextMaskValue(out mask));
            Assert.AreEqual("File", reader.ReadNextMaskValue(out mask));
        }

        [TestMethod]
        public void ReadValueHasCurrentMaskOnlyWhenMultipleMasksExist()
        {
            MaskedFileName<TestMask> maskedFileName = MaskedFileNameFactory.CreateWithFileName("Some Test 2");

            maskedFileName.SetTokenMask(0, TestMask.Any);
            maskedFileName.SetTokenMask(1, TestMask.Alpha);
            maskedFileName.SetTokenMask(2, TestMask.Numeric);

            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(maskedFileName);
            TestMask mask;

            reader.ReadNextMaskValue(out mask);
            Assert.AreEqual(TestMask.Any, mask);

            reader.ReadNextMaskValue(out mask);
            Assert.AreEqual(TestMask.Alpha, mask);

            reader.ReadNextMaskValue(out mask);
            Assert.AreEqual(TestMask.Numeric, mask);
        }

        [TestMethod]
        public void ReadValueHasCorrectSequencesWhenMultipleMasksExist()
        {
            MaskedFileName<TestMask> maskedFileName = MaskedFileNameFactory.CreateWithFileName("Some Test File Bob");
            
            maskedFileName.SetTokenMask(0, TestMask.Any);
            maskedFileName.SetTokenMask(1, TestMask.Any);
            maskedFileName.SetTokenMask(2, TestMask.Alpha);
            maskedFileName.SetTokenMask(3, TestMask.Alpha);

            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(maskedFileName);
            TestMask mask;

            Assert.AreEqual("Some Test", reader.ReadNextMaskValue(out mask));
            Assert.AreEqual("File Bob", reader.ReadNextMaskValue(out mask));
        }

        [TestMethod]
        public void ReadValueHasUnmergedSequenceWhenMaskValuesNotMergable()
        {
            MaskedFileName<TestMask> maskedFileName = MaskedFileNameFactory.CreateWithFileName("1 2");

            maskedFileName.SetTokenMask(0, TestMask.Numeric);
            maskedFileName.SetTokenMask(1, TestMask.Numeric);

            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(maskedFileName);
            TestMask mask;

            Assert.AreEqual("1", reader.ReadNextMaskValue(out mask));
            Assert.AreEqual("2", reader.ReadNextMaskValue(out mask));
        }
    }
}
