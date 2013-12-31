using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow
{
    public static class ListExtensions
    {
        public static bool IsSequential(this IList<int> list)
        {
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i - 1] != (list[i] - 1))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
