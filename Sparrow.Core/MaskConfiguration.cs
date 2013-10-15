using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public class MaskConfiguration
    {
        public MaskConfiguration(string maskString)
            : this(true, maskString)
        {
        }

        public MaskConfiguration(bool isMergable, string maskString)
        {
            this.IsMergable = isMergable;
            this.MaskString = maskString;
        }

        public bool IsMergable { get; set; }

        public string MaskString { get; set; }
    }
}
