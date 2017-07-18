using System;
using System.Collections.Generic;

namespace WebApp.Extensions
{
    public static class DictionaryExtensions
    {
        public static void EnhancedAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value, Func<TValue, TValue, TValue> function)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = function(value, dic[key]);
            }
            else
            {
                dic.Add(key, value);
            }
        }
    }
}