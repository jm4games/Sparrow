namespace Sparrow
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Reads the values from a MaskedFileName by mask type.
    /// </summary>
    /// <typeparam name="TMask">The type of the mask used by the MaskFileName.</typeparam>
    public sealed class MaskedFileNameReader<TMask>
    {
        private readonly MaskedFileName<TMask> maskedFileName;

        private readonly TMask[] masks;

        private readonly string tokenDelimiter;

        private int currentIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaskedFileNameReader{TMask}"/> class.
        /// </summary>
        /// <param name="maskedFileName">The <see cref="MaskedFileName{TMask}"/> to read.</param>
        /// <exception cref="System.ArgumentNullException">When maskedFileName null.</exception>
        public MaskedFileNameReader(MaskedFileName<TMask> maskedFileName)
            : this(maskedFileName, FileNameTokenizer.DefaultTokenDelimiter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaskedFileNameReader{TMask}"/> class.
        /// </summary>
        /// <param name="maskedFileName">The <see cref="MaskedFileName{TMask}"/> to read.</param>
        /// <param name="tokenDelimiter">The token delimiter to use when concatenating mask values.</param>
        /// <exception cref="System.ArgumentNullException">When maskedFileName null.</exception>
        public MaskedFileNameReader(MaskedFileName<TMask> maskedFileName, string tokenDelimiter)
        {
            if (maskedFileName == null)
            {
                throw new ArgumentNullException("maskedFileName");
            }

            this.maskedFileName = maskedFileName;
            this.masks = maskedFileName.GetMaskMappings();
            this.tokenDelimiter = tokenDelimiter ?? FileNameTokenizer.DefaultTokenDelimiter;
        }

        /// <summary>
        /// Gets a value indicating whether all masked values have been read from the <see cref="MaskedFileName{TMask}"/>.
        /// </summary>
        public bool EndOfMaskedFileName 
        {
            get { return this.currentIndex == this.masks.Length; }
        }

        /// <summary>
        /// Reads the next mask value.
        /// </summary>
        /// <param name="mask">The mask associated with the value.</param>
        /// <returns>The value associated with the mask.</returns>
        /// <exception cref="System.InvalidOperationException">When <see cref="MaskedFileName{TMask}"/> has already been fully read.</exception>
        /// <remarks>The value could be a sequence of values joined by the delimiter specified when the reader was created.</remarks>
        public string ReadNextMaskValue(out TMask mask)
        {
            if (this.EndOfMaskedFileName)
            {
                throw new InvalidOperationException("Masked file name has been fully read.");
            }

            int maskStart = this.currentIndex;
            mask = this.masks[this.currentIndex];

            for (this.currentIndex++; this.currentIndex < this.masks.Length; this.currentIndex++)
            {
                if (!this.maskedFileName.MaskConfigurations[mask].IsMergable ||
                    !EqualityComparer<TMask>.Default.Equals(mask, this.masks[this.currentIndex]))
                {
                    break;
                }
            }

            return this.maskedFileName.Tokenizer.GetTokenSequence(maskStart, this.currentIndex - 1, this.tokenDelimiter);
        }
    }
}
