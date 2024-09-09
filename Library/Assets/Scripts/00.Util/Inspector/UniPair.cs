using System.Collections.Generic;

namespace Util.Inspector
{
    [System.Serializable]
    public class UniPair<T1,T2>
    {
        public T1 key;
        public T2 value;

        public UniPair(T1 key, T2 value)
        {
            this.key = key;
            this.value = value;
        }
        public UniPair(UniPair<T1, T2> other)
        {
            this.key = other.key;
            this.value = other.value;
        }
    }
}
