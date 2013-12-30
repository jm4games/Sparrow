namespace Sparrow.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A mask for a file name.
    /// </summary>
    /// <typeparam name="TMask">The type of mask.</typeparam>
    public sealed class MaskedFileName<TMask> : IMaskedFileName
    {
        private readonly FileNameTokenizer tokenizer;

        private readonly TMask defaultMask;

        private readonly TMask[] maskMappings;

        private readonly IDictionary<TMask, MaskConfiguration> availableMasks;

        private string maskedString;

        private bool isDirty = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaskedFileName{TMask}"/> class.
        /// </summary>
        /// <param name="tokenizer">The tokenizer to create a mask from..</param>
        /// <param name="tokenMasks">The masks to apply on tokens.</param>
        /// <exception cref="System.ArgumentNullException">When tokenizer or tokenMasks null.</exception>
        /// <exception cref="System.ArgumentException">A mask string does not exist for provided default mask.</exception>
        public MaskedFileName(FileNameTokenizer tokenizer, IDictionary<TMask, MaskConfiguration> tokenMasks)
        {
            if (tokenizer == null)
            {
                throw new ArgumentNullException("tokenizer");
            }

            if (tokenMasks == null)
            {
                throw new ArgumentNullException("tokenMasks");
            }

            if (!tokenMasks.ContainsKey(default(TMask)))
            {
                throw new ArgumentException("A mask string does not exist for provided default mask.");
            }

            this.tokenizer = tokenizer;
            this.availableMasks = new Dictionary<TMask, MaskConfiguration>(tokenMasks);
            this.defaultMask = default(TMask);
            this.maskMappings = new TMask[tokenizer.TokenCount];

            for (int i=0; i < this.maskMappings.Length; i++)
            {
                this.maskMappings[i] = defaultMask;
            }
        }
        
        public FileNameTokenizer Tokenizer { get { return this.tokenizer; } }
                
        private void GenerateMaskedString()
        {
            StringBuilder builder = new StringBuilder();
            int tokenIndex = 0;
            int index = 0;

            while (index < this.tokenizer.TokenizedFileName.Length)
            {
                if (this.tokenizer.TokenizedFileName[index] == FileNameTokenizer.TOKEN_MARKER)
                {
                    if (this.tokenizer.TokenizedFileName[index + 1] == FileNameTokenizer.TOKEN_MARKER) // escaped token marker ie %% -> %
                    {
                        index += 2;
                    }
                    else
                    {
                        MaskConfiguration maskInfo = this.availableMasks[this.maskMappings[tokenIndex]];

                        if (!maskInfo.IsMergable ||
                            tokenIndex == 0 || 
                            !EqualityComparer<TMask>.Default.Equals(this.maskMappings[tokenIndex - 1], this.maskMappings[tokenIndex]))
                        {
                            builder.Append(maskInfo.MaskString);
                        }

                        tokenIndex++;
                        index = GetEndIndexForType(index, CharacterTypeHelper.IsNumber);
                    }
                }
                else
                {
                    index++;
                }
            }

            this.maskedString = builder.ToString();
            this.isDirty = false;
        }

        private int GetEndIndexForType(int offset, CharacterTypeHelper.TypeCheck typeCheck)
        {
            while (++offset < this.tokenizer.TokenizedFileName.Length && typeCheck(this.tokenizer.TokenizedFileName[offset])) ;
            return offset;
        }

        public void SetTokenMask(int startIndex, int length, TMask mask)
        {
            if (startIndex < 0 || startIndex >= this.maskMappings.Length)
            {
                throw new IndexOutOfRangeException(startIndex + " is not a valid token index.");
            }

            int lastIndex = startIndex + length;
            if (lastIndex > this.maskMappings.Length)
            {
                throw new IndexOutOfRangeException("length '" + length + "' is to large.");
            }

            for (int i = startIndex; i < lastIndex; i++)
            {
                SetTokenMaskInternal(i, mask);
            }
        }

        public void SetTokenMask(IList<int> tokenIndexes, TMask mask)
        {
            if (tokenIndexes == null)
            {
                throw new ArgumentNullException("tokenIndexes");
            }
            
            for (int i = 0; i < tokenIndexes.Count; i++)
            {
                SetTokenMask(tokenIndexes[i], mask);
            }
        }

        public void SetTokenMask(int tokenIndex, TMask mask)
        {
            if (tokenIndex < 0 || tokenIndex >= this.maskMappings.Length)
            {
                throw new IndexOutOfRangeException(tokenIndex + " is not a valid token index.");
            }

            if (!this.availableMasks.ContainsKey(mask))
            {
                throw new ArgumentException("Could not find string value for mask '" + this.maskMappings[tokenIndex].ToString() + "'.");
            }

            this.SetTokenMaskInternal(tokenIndex, mask);
        }

        private void SetTokenMaskInternal(int tokenIndex, TMask mask)
        {
            maskMappings[tokenIndex] = mask;
            isDirty = true;
        }

        public string GetFirstMaskValue(TMask mask)
        {
            for (int i = 0; i < this.maskMappings.Length; i++)
            {
                if (EqualityComparer<TMask>.Default.Equals(this.maskMappings[i], mask))
                {
                    return this.tokenizer[i];
                }
            }

            return null;
        }

        public string GetMaskValue(TMask mask, string delimiter)
        {
            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < this.maskMappings.Length; i++)
            {
                if (EqualityComparer<TMask>.Default.Equals(this.maskMappings[i], mask))
                {
                    builder.AppendFormat("{0}{1}", this.tokenizer[i], delimiter);
                }
            }

            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - delimiter.Length, delimiter.Length); // remove trailing delimiter
            }

            return builder.ToString();
        }

        public TMask[] GetMaskMappings()
        {
            TMask[] mappings = new TMask[this.maskMappings.Length];

            this.maskMappings.CopyTo(mappings, 0);
            return mappings;
        }

        public bool IsMaskSetForToken(int tokenIndex)
        {
            return EqualityComparer<TMask>.Default.Equals(this.maskMappings[tokenIndex], this.defaultMask);
        }

        public override string ToString()
        {
            if (this.maskedString == null | this.isDirty)
            {
                GenerateMaskedString();
            }

            return this.maskedString;
        }
    }
}
