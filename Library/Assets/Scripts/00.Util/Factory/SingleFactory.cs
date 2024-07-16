using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    [System.Serializable]
    public class SingleFactory<T> where T : Inherited.SingleFactoryObject<T>
    {
        [SerializeField] Queue<T> pool;
        [SerializeField] T prefabInfo;

        public T PrefabInfo
        {
            get => prefabInfo;
            set
            {
                if (pool != null && pool.Count != 0)
                {
                    Destroy();
                }
                prefabInfo = value;
            }
        }

        public T Create(Transform parent = null, float scale = 1, bool rotationSet = false, System.Action<T> createSuccess = null)
        {
            if (pool == null) pool = new Queue<T>();

            T obj = null;
            if (0 < pool.Count)
            {
                obj = pool.Dequeue();
                obj.gameObject.SetActive(true);
            }
            //= 새로 생성
            else
            {
                obj = GameObject.Instantiate<T>(prefabInfo);
                obj.gameObject.SetActive(true);
                obj.returnFactory += ReturnEffect;
            }

            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one * scale;
            if (rotationSet) obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.isReturned = false;

            createSuccess?.Invoke(obj);

            return obj;
        }
        bool ReturnEffect(T obj)
        {
            if (pool == null)
            {
                pool = new Queue<T>();
            }
            if (obj.isReturned)
            {
                return false;
            }
            else
            {
                obj.isReturned = true;
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);

                return true;
            }
        }

        public void Destroy()
        {
            foreach (T obj in pool)
            {
                GameObject.Destroy(obj);
            }
            pool.Clear();
        }
    }

}
