using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public class MaskInfo
    {
        public MaskInfo(string maskString)
            : this(true, maskString)
        {
        }

        public MaskInfo(bool isMergable, string maskString)
        {
            this.IsMergable = isMergable;
            this.MaskString = maskString;
        }

        public bool IsMergable { get; set; }

        public string MaskString { get; set; }
    }
}
