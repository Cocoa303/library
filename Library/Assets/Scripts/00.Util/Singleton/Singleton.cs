using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util.Inherited
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>(true);
                }

                return instance;
            }

            set => instance = value;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = GetComponent<T>();
            }
            else if (instance != this)
            {
                Debug.Log("Destroy duplicated singleton [ " + gameObject.name + " ]");
                Destroy(gameObject);
            }

            if (instance != null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }

}
