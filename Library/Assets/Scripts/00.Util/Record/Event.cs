using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Cocoa.Util.Record
{
    public class Event<TKey, TValue>
    {
        private struct Node
        {
            public readonly double start;
            public readonly TValue value;

            public Node(double start, TValue value)
            {
                this.start = start;
                this.value = value;
            }
        }

        private float duration;     //== 기록 시간 : -1은 무제한
        private Stopwatch timer;
        private Dictionary<TKey, List<Node>> record;
        private Dictionary<TKey, System.Action<TValue>> onInsertComplate;
        
        public const float endless = -1.0f;

        public float Duration 
        { 
            get => duration; set
            {
                if (duration != value)
                {
                    duration = value;
                    NodeCleanup();
                }
                else
                {
                    duration = value;
                }
            }
        }

        public Event(float duration = endless)
        {
            this.duration = duration;
            record = new Dictionary<TKey, List<Node>>();
            onInsertComplate = new Dictionary<TKey, System.Action<TValue>>();
            timer = Stopwatch.StartNew();
        }

        public void Insert(TKey key, TValue value)
        {
            if (!record.ContainsKey(key))
            {
                record.Add(key, new List<Node>());
                onInsertComplate.Add(key, null);
            }

            record[key].Add(new Node(timer.Elapsed.TotalSeconds, value));
            onInsertComplate[key].Invoke(value);
        }
        public void Insert(TKey key, System.Action<TValue> onComplate)
        {
            if (!record.ContainsKey(key))
            {
                onInsertComplate.Add(key, null);
            }

            onInsertComplate[key] += onComplate;
        }

        public bool Contains(TKey key)
        {
            if (duration == endless)
            {
                return record.ContainsKey(key);
            }
            else
            {
                if (record.ContainsKey(key)) 
                {
                    NodeCleanup(key);

                    return (0 < record[key].Count);
                }
                return false;
            }
        }

        public int Count(TKey key)
        {
            if (record.ContainsKey(key))
            {
                NodeCleanup(key);
                return record[key].Count;
            }
            else
            {
                return 0;
            }
        }

        public void NodeCleanup()
        {
            foreach (TKey key in record.Keys)
            {
                NodeCleanup(key);
            }
        }

        public void NodeCleanup(TKey key)
        {
            if (duration != endless)
            {
                if (record.ContainsKey(key))
                {
                    double activateTime = timer.Elapsed.TotalSeconds - duration;

                    //== [ NOTE ] 24.10.10
                    //== Remove All은 리스트를 결국 전부 순회하기 때문에 O(n)이다.
                    //== 연산량을 최소화 하기 위하여 아래 방식을 채택
                    for(int i = 0; i < record[key].Count; i++)
                    {
                        if (record[key][i].start < activateTime)
                        {
                            record[key].RemoveAt(i);
                            i--;
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }                    
                }
            }
        }

        public void Foreach(TKey key,System.Action<TValue> action)
        {
            if(record.ContainsKey(key))
            {
                foreach(Node node in record[key])
                {
                    action(node.value);
                }
            }
        }
    }
}
