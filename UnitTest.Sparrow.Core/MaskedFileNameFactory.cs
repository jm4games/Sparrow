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
                                                                                           { TestMask.Any, new MaskConfiguration("A") { IsMergable = true } },
                                                                                           { TestMask.Alpha, new MaskConfiguration("B") { IsMergable = true } },
                                                                                           { TestMask.Numeric, new MaskConfiguration("C") }
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
