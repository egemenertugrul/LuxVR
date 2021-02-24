using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.Extensions
{
    public class Ref<T>
    {
        private T backing;
        public T Value { get { return backing; } }
        public Ref(T reference)
        {
            backing = reference;
        }
    }
}
