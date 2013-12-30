﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class FileNameEnvironmentContext<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        public FileNameEnvironmentContext(MaskedFileName<TMask> maskedString, TKnow knowledgeBase)
        {
            if (maskedString == null)
            {
                throw new ArgumentNullException("maskedString");
            }

            if (knowledgeBase == null)
            {
                throw new ArgumentNullException("knowledgeBase");
            }

            this.MaskedString = maskedString;
            this.KnowledgeBase = knowledgeBase;
            this.MasksResolvedCount = 0;
        }

        public MaskedFileName<TMask> MaskedString { get; private set; }

        public TKnow KnowledgeBase { get; private set; }

        public bool AllTokensHaveMask { get { return this.MasksResolvedCount == MaskedString.Tokenizer.TokenCount; } }

        internal int MasksResolvedCount { get; set; }
    }
}
