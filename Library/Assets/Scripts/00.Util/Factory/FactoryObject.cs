using UnityEngine;

namespace Util.Inherited
{
    [System.Serializable]
    public class FactoryObject<T> : SingleFactoryObject<FactoryObject<T>>
    {
        [SerializeField] protected T key;

        public T Key { get => key; set => key = value; }
        public override void Return()
        {
            returnFactory?.Invoke(this);
        }
    }
}
