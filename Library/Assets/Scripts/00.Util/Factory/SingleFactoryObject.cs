using UnityEngine;

namespace Util.Inherited
{
    public abstract class SingleFactoryObject<T> : MonoBehaviour
    {
        public delegate bool ReturnFactory(T thisObject);
        public ReturnFactory returnFactory;

        public bool isReturned;

        public abstract void Return();
    }
}
