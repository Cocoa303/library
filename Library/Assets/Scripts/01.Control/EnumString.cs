using System.Collections.Generic;

namespace Control
{
    //== C# 7.3 base
    public static class EnumString<T> where T : struct, System.Enum
    {
        private static readonly Dictionary<T, string> record = new();

        //== This involves boxing and unboxing, which may slow down performance.
        //   Use is not recommended.
        public static void AllCaching()
        {
            var values = System.Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                record.Add((T)value, value.ToString());
            }
        }

        public static string ToString(T value)
        {
            if (!record.ContainsKey(value))
            {
                record.Add(value, value.ToString());
            }

            return record[value];
        }
    }

}
