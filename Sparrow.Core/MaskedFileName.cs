namespace Sparrow
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A mask for a file name.
    /// </summary>
    /// <typeparam name="TMask">The type of mask to apply on the file name.</typeparam>
    public sealed class MaskedFileName<TMask>
    {
        private readonly FileNameTokenizer tokenizer;

        private readonly TMask defaultMask;

        private readonly TMask[] maskMappings;

        private readonly Dictionary<TMask, MaskConfiguration> availableMasks;

        private readonly bool isReadOnly = false;

        private string maskedString;

        private bool isDirty = false;

        /// <summary>
        /// Private creater for default instance of the <see cref="MaskedFileName{TMask}"/> class from being created.
        /// </summary>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <exception cref="System.ArgumentNullException">tokenizer</exception>
        private MaskedFileName(FileNameTokenizer tokenizer)
        {
            if (tokenizer == null)
            {
                throw new ArgumentNullException("tokenizer");
            }

            this.tokenizer = tokenizer;
            this.maskMappings = new TMask[tokenizer.TokenCount];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaskedFileName{TMask}"/> class.
        /// </summary>
        /// <param name="tokenizer">The tokenizer to create a mask from.</param>
        /// <param name="tokenMasks">The masks to apply on tokens.</param>
        /// <exception cref="System.ArgumentNullException">When tokenizer or tokenMasks null.</exception>
        /// <exception cref="System.ArgumentException">A mask string does not exist for provided default mask.</exception>
        public MaskedFileName(FileNameTokenizer tokenizer, IDictionary<TMask, MaskConfiguration> tokenMasks)
            : this(tokenizer)
        {
            if (tokenMasks == null)
            {
                throw new ArgumentNullException("tokenMasks");
            }

            if (!tokenMasks.ContainsKey(default(TMask)))
            {
                throw new ArgumentException("A mask string does not exist for provided default mask.");
            }

            this.availableMasks = new Dictionary<TMask, MaskConfiguration>(tokenMasks);
            this.defaultMask = default(TMask);
            
            for (int i=0; i < this.maskMappings.Length; i++)
            {
                this.maskMappings[i] = defaultMask;
            }
        }

        /// <summary>
        /// Initializes a new (readonly) instance of the <see cref="MaskedFileName{TMask}"/> class.
        /// </summary>
        /// <param name="tokenizer">The tokenizer to create a mask from.</param>
        /// <param name="maskedString">The masked string for the file.</param>
        /// <param name="maskMappings">The mappings of masks to token indexes.</param>
        /// <exception cref="System.ArgumentNullException">When tokenizer, maskedString, or maskMappings null.</exception>
        /// <exception cref="System.IndexOutOfRangeException">When a token index in a mask mapping is not valid.</exception>
        public MaskedFileName(FileNameTokenizer tokenizer, string maskedString, IList<MaskMapping<TMask>> maskMappings)
            : this(tokenizer)
        {
            if (String.IsNullOrEmpty(maskedString))
            {
                throw new ArgumentNullException("maskedString");
            }

            if (maskMappings == null)
            {
                throw new ArgumentNullException("maskedMappings");
            }

            this.maskedString = maskedString;
            this.isReadOnly = true;

            // TODO: verify the masked string is valid for the mask mappings provided.

            this.LoadMaskMappings(maskMappings);
        }

        /// <summary>
        /// Gets the tokenizer used by the <see cref="MaskedFileName{TMask}"/>.
        /// </summary>
        public FileNameTokenizer Tokenizer 
        { 
            get { return this.tokenizer; } 
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="MaskedFileName{TMask}"/> is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        /// <summary>
        /// Gets the mask configurations used by the <see cref="MaskedFileName{TMask}"/>.
        /// </summary>
        internal IReadOnlyDictionary<TMask, MaskConfiguration> MaskConfigurations
        {
            get { return this.availableMasks; }
        }
        
        private void LoadMaskMappings(IList<MaskMapping<TMask>> maskMappings)
        {
            foreach (MaskMapping<TMask> mapping in maskMappings)
            {
                for (int i = 0; i < mapping.TokenIndexes.Count; i++)
                {
                    int tokenIndex = mapping.TokenIndexes[i];

                    if (tokenIndex < 0 || tokenIndex >= tokenizer.TokenCount)
                    {
                        throw new IndexOutOfRangeException(tokenIndex + " is not a valid token index.");
                    }

                    this.maskMappings[tokenIndex] = mapping.Mask;
                }
            }
        }

        /// <summary>
        /// Sets the mask for a sequential range of tokens.
        /// </summary>
        /// <param name="startIndex">The start index (inclusive).</param>
        /// <param name="length">The length of the sequence.</param>
        /// <param name="mask">The mask to set for each token.</param>
        /// <exception cref="System.IndexOutOfRangeException">When the start index or length is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">When the <see cref="MaskedFileName{TMask}"/> is readonly.</exception>
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

        /// <summary>
        /// Sets the mask for a list of token indexes.
        /// </summary>
        /// <param name="tokenIndexes">The token indexes.</param>
        /// <param name="mask">The mask to set for each token.</param>
        /// <exception cref="System.ArgumentNullException">When tokenIndexes null.</exception>
        /// <exception cref="System.InvalidOperationException">When the <see cref="MaskedFileName{TMask}"/> is readonly.</exception>
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

        /// <summary>
        /// Sets the mask for the token at specified index.
        /// </summary>
        /// <param name="tokenIndex">Index of the token.</param>
        /// <param name="mask">The mask to set for the token.</param>
        /// <exception cref="System.IndexOutOfRangeException">When the token index is not valid.</exception>
        /// <exception cref="System.ArgumentException">When string value for mask could not be found.</exception>
        /// <exception cref="System.InvalidOperationException">When the <see cref="MaskedFileName{TMask}"/> is readonly.</exception>
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

        /// <summary>
        /// Internal method that all public "SetTokenMask" methods should use when setting an actual token mask.
        /// </summary>
        /// <param name="tokenIndex">Index of the token.</param>
        /// <param name="mask">The mask to set for the token.</param>
        private void SetTokenMaskInternal(int tokenIndex, TMask mask)
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException("The masked file name is readonly.");
            }

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
    }
}
