using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow
{
    public struct MaskMapping<T>
    {
        public MaskMapping(T mask, IList<int> tokenIndexes)
            : this()
        {
            this.Mask = mask;
            this.TokenIndexes = tokenIndexes;
        }

        public T Mask { get; private set; }

        public IList<int> TokenIndexes { get; private set; }
    }
}
