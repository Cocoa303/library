using System.Collections.Generic;
using UnityEngine;

namespace Util.Inspector
{
    //== NOTE : 빌드시에 기존 Dictionary와 성능차이가 크게 나지 않으며
    //          개발중에 Inspector내에서 데이터를 확인하는것이 주 목적이다.
    //==        Inspector내에 데이터를 수정한다 하여, 기존 Dictionary에 데이터가 변동되지는 않는다.
    [System.Serializable]
    public class UniDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
#if UNITY_EDITOR
        [SerializeField, ReadOnly] private List<UniPair<TKey, TValue>> show = new List<UniPair<TKey, TValue>>();

        //== NOTE : 접근 속도 안정화를 위하여 추가
        private Dictionary<TKey, int> showIndex = new Dictionary<TKey, int>();
#endif

        public UniDictionary() : base() { }

        public UniDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { ConstructorSynchronization(); }
        public UniDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { ConstructorSynchronization(); }
        public UniDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { ConstructorSynchronization(); }
        public UniDictionary(int capacity) : base(capacity) { ConstructorSynchronization(capacity); }
        public UniDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { ConstructorSynchronization(); }
        public UniDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { ConstructorSynchronization(); }
        public UniDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { ConstructorSynchronization(capacity); }

        private void ConstructorSynchronization(int capacity = 0)
        {
#if UNITY_EDITOR
            show = new List<UniPair<TKey, TValue>>(capacity);
            showIndex = new Dictionary<TKey, int>(capacity);

            foreach (var key in Keys)
            {
                show.Add(new UniPair<TKey, TValue>(key, this[key]));
                showIndex.Add(key, show.Count - 1);
            }
#endif
        }

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                base[key] = value;
#if UNITY_EDITOR
                if(showIndex.ContainsKey(key))
                {
                    show[showIndex[key]].value = value;
                }
#endif
            }
        }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);

#if UNITY_EDITOR
            show.Add(new UniPair<TKey, TValue>(key, value));
            showIndex.Add(key, show.Count - 1);
#endif
        }
        public new bool TryAdd(TKey key, TValue value)
        {
            bool result = base.TryAdd(key, value);
#if UNITY_EDITOR
            if (result)
            {
                show.Add(new UniPair<TKey, TValue>(key, value));
                showIndex.Add(key, show.Count - 1);
            }
#endif
            return result;
        }
        public new bool Remove(TKey key)
        {
            bool result = base.Remove(key);

#if UNITY_EDITOR
            if(result)
            {
                ConstructorSynchronization(show.Capacity);
            }
#endif

            return result;
        }
        public new bool Remove(TKey key, out TValue value)
        {
            bool result = base.Remove(key, out value);

#if UNITY_EDITOR
            if (result)
            {
                ConstructorSynchronization(show.Capacity);
            }
#endif

            return result;
        }
        public new void Clear()
        {
            base.Clear();

#if UNITY_EDITOR
            show.Clear();
#endif
        }
    }
}