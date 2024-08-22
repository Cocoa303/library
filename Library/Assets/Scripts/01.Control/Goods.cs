using System.Collections.Generic;

namespace Control
{
    public class Goods
    {
        public Dictionary<string, long> record;
        public delegate void OnChange(long value);
        public Dictionary<string, OnChange> observer;

        public void Init()
        {
            record = new Dictionary<string, long>();
            observer = new Dictionary<string, OnChange>();
        }

        public void InsertCallback(string key, OnChange callback)
        {
            if (observer == null) Init();

            if (!observer.ContainsKey(key))
            {
                observer.Add(key, callback);
            }
            else
            {
                observer[key] += callback;
            }
        }

        public bool RemoveCallback(string key, OnChange callback)
        {
            if (observer == null) Init();

            if (!observer.ContainsKey(key))
            {
                return false;
            }

            observer[key] -= callback;

            return true;
        }

        private void Callback(string key, long value)
        {
            if (observer.ContainsKey(key))
            {
                observer[key]?.Invoke(value);
            }
        }

        public void Add(string key, long value)
        {
            if (record == null) Init();

            if (!record.ContainsKey(key))
            {
                record.Add(key,value);
                Callback(key,value);
            }
            else
            {
                record[key] += value;
                Callback(key, record[key]);
            }
        }

        public bool Use(string key, long value)
        {
            if (record == null) Init();

            if (!record.ContainsKey(key))
            {
                return false;
            }
            else
            {
                if (record[key] < value) return false;

                record[key] -= value;
                Callback(key, record[key]);

                return true;
            }
        }

        public bool Exists(string key)
        {
            if (record == null) Init();

            if (!record.ContainsKey(key)) return false;
            else return true;
        }

        public bool Exists(string key, long value)
        {
            if (record == null) Init();

            if (!record.ContainsKey(key)) return false;
            else
            {
                if (record[key] < value) return false;
                else return true;
            }
        }
    }
}
