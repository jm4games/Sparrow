using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class MaskedStringEntity
    {
        public uint Id { get; private set; }

        public string MaskedString { get; private set; }

        public TokenSequenceHash TokenSequenceHash { get; private set; }

        public IList<string> AbstractStrings { get; private set; }
    }
}
