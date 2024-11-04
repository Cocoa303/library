using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Util
{
    [System.Serializable]
    public class Factory<T> where T : FactoryObject
    {
        [SerializeField] List<T> innerDatas = new();
        [SerializeField, ReadOnly] Inspector.UniDictionary<string, T> database;

        //== Init 함수의 자동 호출을 위하여 초기화하지 않습니다.
        Dictionary<string, Queue<T>> pool;

        private void Init()
        {
            if (pool != null) pool.Clear();
            if (database != null) database.Clear();

            pool = new();
            database = new();

            foreach (var fObject in innerDatas)
            {
                if (!pool.ContainsKey(fObject.ID))
                {
                    pool.Add(fObject.ID, new Queue<T>());
                }
                else
                {
                    Debug.LogError($"Contatins Key {fObject.ID} in pool");
                    continue;
                }

                if (!database.ContainsKey(fObject.ID))
                {
                    database.Add(fObject.ID, fObject);
                }
                else
                {
                    Debug.LogError($"Contatins Key {fObject.ID} in prefabs");
                    continue;
                }
            }
        }

        public void InsertNewData(T data)
        {
            if (pool == null) Init();

            if (!pool.ContainsKey(data.ID))
            {
                pool.Add(data.ID, new Queue<T>());
                innerDatas.Add(data);
            }
            if (!database.ContainsKey(data.ID))
            {
                database.Add(data.ID, data);
            }
        }

        public T Create(string id, Transform parent = null, System.Action<T> OnCreateSuccess = null)
        {
            if (pool == null) Init();

            if (!pool.ContainsKey(id))
            {
                Debug.LogError($"Creating a FactoryObject {id} that is not registered.");
                return null;
            }

            T fObject = null;
            Queue<T> selectPool = pool[id];

            if (0 < selectPool.Count)
            {
                fObject = selectPool.Dequeue();
                fObject.gameObject.SetActive(true);
            }
            //== new Create
            else
            {
                fObject = GameObject.Instantiate(database[id]);
                fObject.gameObject.SetActive(true);
                fObject.OnReturn += ReturnMethod;
            }

            if (fObject.transform.parent != parent) fObject.transform.parent = parent;
            fObject.transform.localPosition = Vector3.zero;
            fObject.transform.localScale = database[id].transform.localScale;
            fObject.transform.localRotation = Quaternion.identity;
            fObject.IsReturned = false;

            OnCreateSuccess?.Invoke(fObject);

            return fObject;
        }

        public (List<string> ids, System.Action UseEndCallback) GetIDs(System.Type type)
        {
            List<string> ids = ListPool<string>.Get();

            foreach (T ui in innerDatas)
            {
                if (ui.GetType() == type)
                {
                    {
                        ids.Add(ui.ID);
                    }
                }
            }

            return (ids, () =>
            {
                ListPool<string>.Release(ids);
            }
            );
        }

        public bool IsExist(string id)
        {
            if (pool == null) Init();

            if (!database.ContainsKey(id))
            {
                return false;
            }

            return true;
        }

        private bool ReturnMethod(FactoryObject data)
        {
            if (pool == null) Init();

            if (data.IsReturned)
            {
                return false;
            }
            else
            {
                data.IsReturned = true;
                data.gameObject.SetActive(false);
                data.OnReturn -= ReturnMethod;
                pool[data.ID].Equals(data);

                return true;
            }
        }
    }

}
