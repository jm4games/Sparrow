using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow
{
    internal static class CharacterTypeHelper
    {
        private const int LARGE_ALPHA_START = (int)'A';

        private const int LARGE_ALPHA_END = (int)'Z';

        private const int SMALL_ALPHA_START = (int)'a';

        private const int SMALL_ALPHA_END = (int)'z';

        private const int NUMBER_START = (int)'0';

        private const int NUMBER_END = (int)'9';

        public delegate bool TypeCheck(char value);

        public static bool IsNumber(char value)
        {
            int raw = (int)value;
            return raw >= NUMBER_START && raw <= NUMBER_END;
        }

        public static bool IsAlpha(char value)
        {
            int raw = (int)value;
            return (raw >= LARGE_ALPHA_START && raw <= LARGE_ALPHA_END) ||
                   (raw >= SMALL_ALPHA_START && raw <= SMALL_ALPHA_END);
        }
    }
}
