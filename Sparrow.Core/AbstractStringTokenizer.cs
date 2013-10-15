using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class AbstractStringTokenizer : IEnumerable<String>
    {
        private const int MAX_FILENAME_SIZE = 255;

        internal const char TOKEN_MARKER = '%';

        private readonly List<String> tokens = new List<String>();

        public AbstractStringTokenizer(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.Length > MAX_FILENAME_SIZE)
            {
                throw new ArgumentException("value is longer than " + MAX_FILENAME_SIZE + " characters.");
            }

            this.OriginalString = value;
            this.TokenSequenceHash = new TokenSequenceHash();
            this.GenerateAbstractString();
        }

        public string OriginalString { get; private set; }

        public string AbstractString { get; private set; }

        public TokenSequenceHash TokenSequenceHash { get; private set; }

        public int TokenCount { get { return this.tokens.Count; } }
        
        public string this[int tokenIndex] { get { return this.tokens[tokenIndex]; } }

        private void GenerateAbstractString()
        {
            const int NO_END_INDEX = -1;
            StringBuilder builder = new StringBuilder();
            int tokenEndIndex = NO_END_INDEX;
            int tokenCount = 0;
            int index = 0;

            while (index < this.OriginalString.Length)
            {
                if (CharacterTypeHelper.IsAlpha(this.OriginalString[index]))
                {
                    tokenEndIndex = GetEndIndexForType(index, CharacterTypeHelper.IsAlpha);
                    this.TokenSequenceHash.MarkNextTokenAsAlpha();
                }
                else if (CharacterTypeHelper.IsNumber(this.OriginalString[index]))
                {
                    tokenEndIndex = GetEndIndexForType(index, CharacterTypeHelper.IsNumber);
                    this.TokenSequenceHash.MarkNextTokenAsNumber();
                }
                else if (this.OriginalString[index] == TOKEN_MARKER)
                {
                    builder.AppendFormat("{0}{0}", TOKEN_MARKER);
                }
                else
                {
                    builder.Append(this.OriginalString[index]);
                }

                if (tokenEndIndex != NO_END_INDEX)
                {
                    tokens.Add(this.OriginalString.Substring(index, tokenEndIndex - index));
                    builder.AppendFormat("{0}{1}", TOKEN_MARKER, tokenCount++);

                    index = tokenEndIndex;
                    tokenEndIndex = NO_END_INDEX;
                }
                else
                {
                    index++;
                }
            }

            this.AbstractString = builder.ToString();
        }

        private int GetEndIndexForType(int offset, CharacterTypeHelper.TypeCheck typeCheck)
        {
            while (++offset < this.OriginalString.Length && typeCheck(this.OriginalString[offset])) ;
            return offset;
        }

        public IEnumerator<String> GetEnumerator()
        {
            return tokens.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return tokens.GetEnumerator();
        }

        public override string ToString()
        {
            return String.Format("Original: {0}{1}Abstract: {2}", this.OriginalString, Environment.NewLine, this.AbstractString);
        }
    }
}
