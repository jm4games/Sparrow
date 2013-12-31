using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    internal static class StringExtensions
    {
        public static bool IsNumber(this string value)
        {
            int number;
            return Int32.TryParse(value, out number);
        }

        public static bool IsValidFilePath(this string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return false; 
            }

            return true;
        }
    }
}
