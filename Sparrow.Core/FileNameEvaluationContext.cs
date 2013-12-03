using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class FileNameEvaluationContext<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        public FileNameEvaluationContext(MaskedFileName<TMask> maskedString, TKnow knowledgeBase)
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
            this.Rules = new List<MaskRule<TMask, TKnow>>();
            this.SetMaskCount = 0;

            //TODO: add code for increasing mask count
        }

        public MaskedFileName<TMask> MaskedString { get; private set; }

        public TKnow KnowledgeBase { get; private set; }

        public bool AllTokensHaveMask { get { return this.SetMaskCount == MaskedString.Tokenizer.TokenCount; } }

        internal int SetMaskCount { get; set; }

        internal IList<MaskRule<TMask, TKnow>> Rules { get; private set; }
    }
}
