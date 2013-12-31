using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    internal sealed class PriorityQueue<T>
    {
        public Task EnqueueAsync(T item)
        {
            throw new NotImplementedException();
        }

        public Task<T> RemoveAsync(T item)
        {
            throw new NotImplementedException();
        }

        public Task<T> DequeueAsync()
        {
            throw new NotImplementedException();
        }
    }
}
