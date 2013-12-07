using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class MaskedFileNameEntity
    {
        public uint Id { get; private set; }

        public string MaskedFileName { get; private set; }

        public BinarySequenceHashBuilder TokenSequenceHash { get; private set; }

        public IList<string> AbstractFileNames { get; private set; }
    }
}
