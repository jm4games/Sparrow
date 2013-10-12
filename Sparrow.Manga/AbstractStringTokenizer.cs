﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class AbstractStringTokenizer : IEnumerable<String>
    {
        private const int LARGE_ALPHA_START = (int)'A';

        private const int LARGE_ALPHA_END = (int)'Z';

        private const int SMALL_ALPHA_START = (int)'a';

        private const int SMALL_ALPHA_END = (int)'z';

        private const int NUMBER_START = (int)'0';

        private const int NUMBER_END = (int)'9';

        private const char TOKEN_MARKER = '%';

        private delegate bool TypeCheck(char value);

        private readonly List<String> tokens = new List<String>();

        public AbstractStringTokenizer(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.OriginalString = value;
            this.GenerateAbstractString();
        }

        public string OriginalString { get; private set; }

        public string AbstractString { get; private set; }

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
                if (IsAlpha(this.OriginalString[index]))
                {
                    tokenEndIndex = GetEndIndexForType(index, IsAlpha);
                }
                else if (IsNumber(this.OriginalString[index]))
                {
                    tokenEndIndex = GetEndIndexForType(index, IsNumber);
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

        private int GetEndIndexForType(int offset, TypeCheck typeCheck)
        {
            while (++offset < this.OriginalString.Length && typeCheck(this.OriginalString[offset])) ;
            return offset;
        }

        private static bool IsNumber(char value)
        {
            int raw = (int)value;
            return raw >= NUMBER_START && raw <= NUMBER_END;
        }

        private static bool IsAlpha(char value)
        {
            int raw = (int)value;
            return (raw >= LARGE_ALPHA_START && raw <= LARGE_ALPHA_END) ||
                   (raw >= SMALL_ALPHA_START && raw <= SMALL_ALPHA_END);
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
