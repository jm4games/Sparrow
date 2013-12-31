using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class MaskedFileNameReader<T>
    {
        public MaskedFileNameReader(MaskedFileName<T> maskedFileName)
        {

        }

        public bool EndOfMaskedFileName { get { return true; } }

        public string ReadNextMaskValue(out T mask)
        {
            throw new  NotImplementedException();
        }
    }
}
