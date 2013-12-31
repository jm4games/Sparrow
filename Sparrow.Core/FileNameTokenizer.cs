﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow
{
    public sealed class FileNameTokenizer : IEnumerable<String>
    {
        public const string DefaultTokenDelimiter = " ";

        private const int MAX_FILENAME_SIZE = 255;

        internal const char TOKEN_MARKER = '%';

        private readonly List<String> tokens = new List<String>();

        public FileNameTokenizer(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (fileName.Length > MAX_FILENAME_SIZE)
            {
                throw new ArgumentException("File name is longer than " + MAX_FILENAME_SIZE + " characters.");
            }

            this.OriginalFileName = fileName;
            this.TokenSequenceHash = new BinarySequenceHashBuilder();
            this.GenerateTokenizedFileName();
        }

        public string OriginalFileName { get; private set; }

        public string TokenizedFileName { get; private set; }

        public BinarySequenceHashBuilder TokenSequenceHash { get; private set; }

        public int TokenCount { get { return this.tokens.Count; } }
        
        public string this[int tokenIndex] { get { return this.tokens[tokenIndex]; } }

        private void GenerateTokenizedFileName()
        {
            const int NO_END_INDEX = -1;
            StringBuilder builder = new StringBuilder();
            int tokenEndIndex = NO_END_INDEX;
            int tokenCount = 0;
            int index = 0;

            while (index < this.OriginalFileName.Length)
            {
                if (CharacterTypeHelper.IsAlpha(this.OriginalFileName[index]))
                {
                    tokenEndIndex = GetEndIndexForType(index, CharacterTypeHelper.IsAlpha);
                    this.TokenSequenceHash.AppendZeroToSequence();
                }
                else if (CharacterTypeHelper.IsNumber(this.OriginalFileName[index]))
                {
                    tokenEndIndex = GetEndIndexForType(index, CharacterTypeHelper.IsNumber);
                    this.TokenSequenceHash.AppendOneSequence();
                }
                else if (this.OriginalFileName[index] == TOKEN_MARKER)
                {
                    builder.AppendFormat("{0}{0}", TOKEN_MARKER);
                }
                else
                {
                    builder.Append(this.OriginalFileName[index]);
                }

                if (tokenEndIndex != NO_END_INDEX)
                {
                    tokens.Add(this.OriginalFileName.Substring(index, tokenEndIndex - index));
                    builder.AppendFormat("{0}{1}", TOKEN_MARKER, tokenCount++);

                    index = tokenEndIndex;
                    tokenEndIndex = NO_END_INDEX;
                }
                else
                {
                    index++;
                }
            }

            this.TokenizedFileName = builder.ToString();
        }

        private int GetEndIndexForType(int offset, CharacterTypeHelper.TypeCheck typeCheck)
        {
            while (++offset < this.OriginalFileName.Length && typeCheck(this.OriginalFileName[offset])) ;
            return offset;
        }

        public string GetTokenSequence(int start, int end)
        {
            return this.GetTokenSequence(start, end, DefaultTokenDelimiter);
        }

        public string GetTokenSequence(int start, int end, string delimiter)
        {
            if (start < 0 || start >= this.tokens.Count)
            {
                throw new IndexOutOfRangeException("start");
            }

            if (end < 0 || end >= this.tokens.Count)
            {
                throw new IndexOutOfRangeException("end");
            }

            if (start > end)
            {
                throw new ArgumentException("End is not greater than or equal to start.");
            }
            
            StringBuilder builder = new StringBuilder();
            delimiter = delimiter ?? DefaultTokenDelimiter;

            for (int i = start; i <= end; i++)
            {
                builder.AppendFormat("{0}{1}", this.tokens[i], delimiter);
            }

            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - delimiter.Length, delimiter.Length); // remove trailing delimiter
            }

            return builder.ToString();
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
            return String.Format("Original: {0}{1}Tokenized: {2}", this.OriginalFileName, Environment.NewLine, this.TokenizedFileName);
        }
    }
}
