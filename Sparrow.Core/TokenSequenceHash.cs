namespace Sparrow.Core
{
    using System;
    using System.Text;

    /// <summary>
    /// Builds a 66 character long hex string hash of a binary sequence.
    /// </summary>
    /// <remarks>
    /// The binary sequence has a max length of 255 bits.
    /// <para />
    /// A raw byte array representation of the sequence is 33 bytes in length. The array contains the actual binary 
    /// sequence in the first 32 indexes of the array and the last index is reserved for the actual length of the
    /// sequence.
    /// </remarks>
    public sealed class BinarySequenceHashBuilder : IEquatable<BinarySequenceHashBuilder>
    {
        public const int SEQUENCE_BYTE_LENGTH = 33;

        private const int BITS_IN_BYTES = 8;

        private const int SEQUENCE_BIT_LENGTH_INDEX = SEQUENCE_BYTE_LENGTH - 1;
        
        private static readonly char[] HEX_VALUES = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private readonly byte[] rawSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySequenceHashBuilder"/> class.
        /// </summary>
        public BinarySequenceHashBuilder()
        {
            this.rawSequence = new byte[SEQUENCE_BYTE_LENGTH];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySequenceHashBuilder"/> class.
        /// </summary>
        /// <param name="rawSequence">The raw sequence to use for the builder.</param>
        /// <exception cref="System.ArgumentNullException">When rawSequence is null.</exception>
        /// <exception cref="System.ArgumentException">When the raw sequence is not the required length.</exception>
        public BinarySequenceHashBuilder(byte[] rawSequence)
        {
            if (rawSequence == null)
            {
                throw new ArgumentNullException("rawSeqence");
            }

            if (rawSequence.Length != SEQUENCE_BYTE_LENGTH)
            {
                throw new ArgumentException("The raw sequence must constain " + SEQUENCE_BYTE_LENGTH + " bytes.");
            }

            this.rawSequence = rawSequence;
        }

        /// <summary>
        /// Gets the length of the sequence.
        /// </summary>
        public int SequenceLength { get { return this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX]; } }

        /// <summary>
        /// Gets the bytes currently in the sequence.
        /// </summary>
        /// <returns>The bytes.</returns>
        public byte[] GetBytes()
        {
            byte[] result = new byte[SEQUENCE_BYTE_LENGTH];

            this.rawSequence.CopyTo(result, 0);

            return result;
        }

        /// <summary>
        /// Appends a one to the sequence.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The max sequence length has been reached.</exception>
        public void AppendOneSequence()
        {
            if (this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX] == Byte.MaxValue)
            {
                throw new InvalidOperationException("The max sequence length has been reached.");
            }

            int index = this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX] / BITS_IN_BYTES;

            this.rawSequence[index] <<= 1;
            this.rawSequence[index] |= 0x01;
            this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX]++;
        }

        /// <summary>
        /// Appends a zero to the sequence.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The max sequence length has been reached.</exception>
        public void AppendZeroToSequence()
        {
            if (this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX] == Byte.MaxValue)
            {
                throw new InvalidOperationException("The max sequence length has been reached.");
            }

            this.rawSequence[this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX] / BITS_IN_BYTES] <<= 1;
            this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX]++;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as BinarySequenceHashBuilder);
        }

        /// <summary>
        /// Indicates whether the current sequence is equal to another sequence.
        /// </summary>
        /// <param name="other">A sequence to compare with this sequence.</param>
        /// <returns>
        /// True if the current sequence is equal to the sequence; otherwise, false.
        /// </returns>
        public bool Equals(BinarySequenceHashBuilder other)
        {
            if (Object.ReferenceEquals(other, null) || other.rawSequence[SEQUENCE_BIT_LENGTH_INDEX] != this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX])
            {
                return false;
            }

            int lastIndex = this.rawSequence[SEQUENCE_BIT_LENGTH_INDEX] / BITS_IN_BYTES;
            for (int i = 0; i <= lastIndex; i++)
            {
                if (other.rawSequence[i] != this.rawSequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Indicates whether the first sequence is equal to the second sequence.
        /// </summary>
        /// <param name="seq1">The first sequence to compare.</param>
        /// <param name="seq2">The second sequence to compare.</param>
        /// <returns>
        /// True if the sequences are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(BinarySequenceHashBuilder seq1, BinarySequenceHashBuilder seq2)
        {
            bool ref1 = Object.ReferenceEquals(seq1, null);
            bool ref2 = Object.ReferenceEquals(seq2, null);

            if (ref1 ^ ref2)
            {
                return false;
            }

            return (ref1 && ref2) || seq1.Equals(seq2);
        }

        /// <summary>
        /// Indicates whether the first sequence is not equal to the second sequence.
        /// </summary>
        /// <param name="seq1">The first sequence to compare.</param>
        /// <param name="seq2">The second sequence to compare.</param>
        /// <returns>
        /// True if the sequences not are equal; otherwise, false.
        /// </returns>
        public static bool operator !=(BinarySequenceHashBuilder seq1, BinarySequenceHashBuilder seq2)
        {
            return !seq1.Equals(seq2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a 66 character long hex <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A hex <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(SEQUENCE_BYTE_LENGTH * 2);

            for (int i = 0; i < SEQUENCE_BYTE_LENGTH; i++)
            {
                builder.Append(HEX_VALUES[this.rawSequence[i] >> 4]);
                builder.Append(HEX_VALUES[this.rawSequence[i] & 0xF]);
            }
            
            return builder.ToString();
        }
    }
}
