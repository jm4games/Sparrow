﻿namespace Sparrow
{
    using System;

    [Flags]
    public enum MaskRuleValueRestriction
    {
        Alphabetical = 0x1,
        Numeric = 0x2,
        Any = 0x3
    }

    public interface IMaskRule<TMask>
    {
        MaskRuleValueRestriction ValueRestriction { get; }

        TMask Mask { get; }

         System.Threading.Tasks.Task<bool> IsMatchAsync(string value);
    }
}
