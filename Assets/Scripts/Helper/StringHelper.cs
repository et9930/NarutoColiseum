using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Helper
{
    public class StringHelper
    {
        private static readonly StringHelper instance = new StringHelper();

        private readonly Dictionary<int, string> StringDictionary;
        private int _index;

        private StringHelper()
        {
#if DEBUG_OUTPUT
            Debug.Log(GetType() + " initialize");
#endif
            StringDictionary = new Dictionary<int, string>();
            _index = 0;
        }

        public static StringHelper GetInstance()
        {
            return instance;
        }

        public string GetStringByIndex(int index)
        {
            StringDictionary.TryGetValue(index, out var value);
#if DEBUG_OUTPUT
            Debug.Log(GetType() + " read index " + index + " result " + (value == null ? "NULL" : "\"" + value + "\""));
#endif
            return value;
        }

        public int SetString(string value, int index)
        {
            StringDictionary[index] = value;
#if DEBUG_OUTPUT
            Debug.Log(GetType() + " store \"" + value + "\" at index " + index);
#endif
            return index;
        }

        public int SetString(string value)
        {
            if (StringDictionary.ContainsValue(value))
                return StringDictionary.FirstOrDefault(x => x.Value == value).Key;
            return SetString(value, _index++);
        }
    }
}