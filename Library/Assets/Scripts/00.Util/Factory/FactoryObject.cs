using UnityEngine;

namespace Util
{
    public class FactoryObject : MonoBehaviour
    {
        [SerializeField] protected string id = string.Empty;

        public string ID { get => id; protected set => id = value; }
        public virtual void Initialize() { }

        public bool IsReturned;
        public delegate void OnReturnCallback(FactoryObject fObject);
        public OnReturnCallback OnReturn;

        public virtual void Return()
        {
            OnReturn?.Invoke(this);
        }

        protected virtual void OnValidate()
        {
            if (ID.CompareTo(string.Empty) == 0)
            {
                ID = GetType().ToString();
            }
        }
    }

}