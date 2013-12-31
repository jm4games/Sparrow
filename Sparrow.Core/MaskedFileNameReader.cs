using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow
{
    public sealed class MaskedFileNameReader<TMask>
    {
        private readonly MaskedFileName<TMask> maskedFileName;

        private readonly TMask[] masks;

        private readonly string tokenDelimiter;

        private int currentIndex = 0;

        public MaskedFileNameReader(MaskedFileName<TMask> maskedFileName)
            : this (maskedFileName, FileNameTokenizer.DefaultTokenDelimiter)
        {
        }

        public MaskedFileNameReader(MaskedFileName<TMask> maskedFileName, string tokenDelimeter)
        {
            if (maskedFileName == null)
            {
                throw new ArgumentNullException("maskedFileName");
            }

            this.maskedFileName = maskedFileName;
            this.masks = maskedFileName.GetMaskMappings();
            this.tokenDelimiter = tokenDelimiter ?? FileNameTokenizer.DefaultTokenDelimiter;
        }

        public bool EndOfMaskedFileName 
        {
            get { return this.currentIndex == this.masks.Length; }
        }

        public string ReadNextMaskValue(out TMask mask)
        {
            if (this.EndOfMaskedFileName)
            {
                throw new InvalidOperationException("Masked file name has been fully read.");
            }

            int maskStart = currentIndex;
            mask = this.masks[currentIndex];

            for (currentIndex++; currentIndex < this.masks.Length; currentIndex++)
            {
                if (!EqualityComparer<TMask>.Default.Equals(mask, this.masks[currentIndex]))
                {
                    break;
                }
            }

            return this.maskedFileName.Tokenizer.GetTokenSequence(maskStart, currentIndex - 1, this.tokenDelimiter);
        }
    }
}
