using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class TokenSequenceHash : IEquatable<TokenSequenceHash>
    { 
        private const int BITS_IN_BYTES = 8;

        private const int TOKEN_COUNT_INDEX = SEQUENCE_LENGTH - 1;

        public const int SEQUENCE_LENGTH = 33;

        private static readonly char[] HEX_VALUES = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private readonly byte[] rawSequence;

        public TokenSequenceHash()
        {
            rawSequence = new byte[SEQUENCE_LENGTH];
        }
     
        public TokenSequenceHash(byte[] rawSequence)
        {
            if (rawSequence == null)
            {
                throw new ArgumentNullException("rawSeqence");
            }

            if (rawSequence.Length != SEQUENCE_LENGTH)
            {
                throw new ArgumentException("The raw sequence is not " + SEQUENCE_LENGTH + " bytes.");
            }

            this.rawSequence = rawSequence;
        }

        public int TokenCount { get { return this.rawSequence[TOKEN_COUNT_INDEX]; } }

        public byte[] GetBytes()
        {
            byte[] result = new byte[SEQUENCE_LENGTH];

            this.rawSequence.CopyTo(result, 0);

            return result;
        }

        public void MarkNextTokenAsNumber()
        {
            if (this.rawSequence[TOKEN_COUNT_INDEX] == Byte.MaxValue)
            {
                throw new InvalidOperationException("Max number of tokens reached.");
            }

            int index = this.rawSequence[TOKEN_COUNT_INDEX] / BITS_IN_BYTES;

            this.rawSequence[index] <<=  1;
            this.rawSequence[index] |= 0x01;
            this.rawSequence[TOKEN_COUNT_INDEX]++;
        }

        public void MarkNextTokenAsAlpha()
        {
            if (this.rawSequence[TOKEN_COUNT_INDEX] == Byte.MaxValue)
            {
                throw new InvalidOperationException("Max number of tokens reached.");
            }

            this.rawSequence[this.rawSequence[TOKEN_COUNT_INDEX] / BITS_IN_BYTES] <<= 1;
            this.rawSequence[TOKEN_COUNT_INDEX]++;
        }
        
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TokenSequenceHash);
        }

        public bool Equals(TokenSequenceHash other)
        {
            if (Object.ReferenceEquals(other, null) || other.rawSequence[TOKEN_COUNT_INDEX] != this.rawSequence[TOKEN_COUNT_INDEX])
            {
                return false;
            }

            int lastIndex = this.rawSequence[TOKEN_COUNT_INDEX] / BITS_IN_BYTES;
            for (int i = 0; i <= lastIndex; i++)
            {
                if (other.rawSequence[i] != this.rawSequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(TokenSequenceHash seq1, TokenSequenceHash seq2)
        {
            bool ref1 = Object.ReferenceEquals(seq1, null);
            bool ref2 = Object.ReferenceEquals(seq2, null);

            if (ref1 ^ ref2)
            {
                return false;
            }

            return (ref1 && ref2) || seq1.Equals(seq2);
        }

        public static bool operator !=(TokenSequenceHash seq1, TokenSequenceHash seq2)
        {
            return !seq1.Equals(seq2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(SEQUENCE_LENGTH * 2);

            for (int i = 0; i < SEQUENCE_LENGTH; i++)
            {
                builder.Append(HEX_VALUES[this.rawSequence[i] >> 4]);
                builder.Append(HEX_VALUES[this.rawSequence[i] & 0xF]);
            }
            
            return builder.ToString();
        }
    }
}
