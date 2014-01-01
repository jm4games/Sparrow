namespace Sparrow
{
    using System.Collections.Generic;
    
    public enum TestMask
    {
        Any,
        Alpha,
        Numeric
    }

    public static class MaskedFileNameFactory
    {
        public static readonly Dictionary<TestMask, MaskConfiguration> MaskConfigurations = new Dictionary<TestMask, MaskConfiguration>()
                                                                                       {
                                                                                           { TestMask.Any, new MaskConfiguration("A")},
                                                                                           { TestMask.Alpha, new MaskConfiguration("B")},
                                                                                           { TestMask.Numeric, new MaskConfiguration("C") { IsMergable = false } }
                                                                                       };

        public static MaskedFileName<TestMask> CreateWithEmptyFileName()
        {
            return new MaskedFileName<TestMask>(new FileNameTokenizer(""), MaskConfigurations);
        }

        public static MaskedFileName<TestMask> CreateWithFileName(string fileName)
        {
            return new MaskedFileName<TestMask>(new FileNameTokenizer(fileName), MaskConfigurations);
        }
    }
}
