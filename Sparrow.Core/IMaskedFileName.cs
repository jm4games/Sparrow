﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public interface IMaskedFileName
    {
        FileNameTokenizer Tokenizer { get; }

        bool IsMaskSetForToken(int tokenIndex);

        string ToString();
    }
}