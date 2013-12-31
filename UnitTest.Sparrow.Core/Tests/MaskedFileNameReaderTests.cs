namespace Sparrow.Core
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionThrownWhenAllMasksHaveBeenRead()
        {
            MaskedFileNameReader<TestMask> reader = new MaskedFileNameReader<TestMask>(MaskedFileNameFactory.CreateWithEmptyFileName());
            TestMask mask;

            reader.ReadNextMaskValue(out mask);
        }

        [TestMethod]
        public void ReadValueHasAllTokensWhenTokensHaveSameMask()
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
        }

        //TODO: Add more test for read value scenarios
    }
}
