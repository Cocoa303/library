using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    [System.Serializable]
    public class Factory<TKey>
    {
        [SerializeField] List<Inherited.FactoryObject<TKey>> database;

        Dictionary<TKey /* id */, Queue<Inherited.FactoryObject<TKey>>> pool;
        Dictionary<TKey /* id */, Inherited.FactoryObject<TKey>> prefabs;

        public List<Inherited.FactoryObject<TKey>> Database
        {
            get
            {
                if (database == null) database = new List<Inherited.FactoryObject<TKey>>();
                return database;
            }
        }
        public void InsertNewData(TKey key, Inherited.FactoryObject<TKey> obj)
        {
            if (pool == null || prefabs == null) Init();

            if (!pool.ContainsKey(key))
            {
                pool.Add(key, new Queue<Inherited.FactoryObject<TKey>>());
                Database.Add(obj);
            }
            if (!prefabs.ContainsKey(key))
            {
                prefabs.Add(key, obj);
            }
        }

        public T Create<T>(TKey id, Transform parent = null, bool resetScale = false, bool rotationSet = false,
            System.Action<T> createSuccess = null) where T : Inherited.FactoryObject<TKey>
        {
            if (this.pool == null || prefabs == null) Init();

            if (!this.pool.ContainsKey(id))
            {
                //Debug.LogError($"{id} is not contains db");
                return null;
            }

            Inherited.FactoryObject<TKey> obj = null;
            Queue<Inherited.FactoryObject<TKey>> selectPool = pool[id];

            if (0 < selectPool.Count)
            {
                obj = selectPool.Dequeue();
                obj.gameObject.SetActive(true);
            }
            //= 새로 생성
            else
            {
                obj = GameObject.Instantiate(prefabs[id]);
                obj.gameObject.SetActive(true);
                obj.returnFactory += ReturnEffect;
            }

            //== 24.07.18 Added verification code as SetParent is quite heavy
            if (obj.transform.parent != parent) obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            if (resetScale) obj.transform.localScale = Vector3.one;
            else obj.transform.localScale = prefabs[id].transform.localScale;

            if (rotationSet) obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.isReturned = false;

            createSuccess?.Invoke((T)obj);

            return (T)obj;
        }

        public bool Exist(TKey id)
        {
            if (pool != null) return pool.ContainsKey(id);

            return false;
        }

        public void Destroy(TKey id)
        {
            if (pool.ContainsKey(id))
            {
                while (pool[id].Count > 0)
                {
                    var @object = pool[id].Dequeue();
                    if (@object.isReturned)
                    {
                        Object.Destroy(@object);
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < database.Count; i++)
            {
                try
                {
                    Destroy(database[i].Key);
                }
                catch { }
            }
            database.Clear();
        }

        void Init()
        {
            if (pool != null) pool.Clear();
            if (prefabs != null) prefabs.Clear();

            pool = new Dictionary<TKey, Queue<Inherited.FactoryObject<TKey>>>();
            prefabs = new Dictionary<TKey, Inherited.FactoryObject<TKey>>();

            for (int i = 0; i < database.Count; i++)
            {
                if (!pool.ContainsKey(database[i].Key))
                {
                    pool.Add(database[i].Key, new Queue<Inherited.FactoryObject<TKey>>());
                }
                else
                {
                    Debug.LogError("Contains key : " + database[i].Key + " " + database[i].name);
                }
                if (!prefabs.ContainsKey(database[i].Key))
                {
                    prefabs.Add(database[i].Key, database[i]);
                }
                else
                {
                    Debug.LogError("Contains key : " + database[i].Key + " " + database[i].name);
                }
            }
        }

        bool ReturnEffect(Inherited.FactoryObject<TKey> obj)
        {
            if (pool == null) Init();

            if (obj.isReturned)
            {
                return false;
            }
            else
            {
                obj.isReturned = true;
                obj.gameObject.SetActive(false);
                pool[obj.Key].Enqueue(obj);

                return true;
            }
        }
    }
}
