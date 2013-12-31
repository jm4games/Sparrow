using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public interface IFileNameFactory<TMask>
    {
        string CreateNewFileName(string originalDirectory, MaskedFileName<TMask> maskedFileName);
    }
}
