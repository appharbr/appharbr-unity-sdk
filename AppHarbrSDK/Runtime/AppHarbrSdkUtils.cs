using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

namespace AppHarbrSDK
{
    public static class AppHarbrSdkUtils
    {
        public static string GetStringFromDictionary(IDictionary<string, object> dictionary, string key, string defaultValue = "")
        {
            if (dictionary != null && dictionary.TryGetValue(key, out object value) && value != null)
            {
                return value.ToString();
            }

            return defaultValue;
        }

        public static int GetIntFromDictionary(IDictionary<string, object> dictionary, string key, int defaultValue = 0)
        {
            if (dictionary != null && dictionary.TryGetValue(key, out object obj) && obj != null)
            {
                if (int.TryParse(InvariantCultureToString(obj), NumberStyles.Any, CultureInfo.InvariantCulture, out int value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        public static bool GetBoolFromDictionary(IDictionary<string, object> dictionary, string key, bool defaultValue = false)
        {
            if (dictionary != null && dictionary.TryGetValue(key, out object obj) && obj != null)
            {
                if (obj is bool boolValue)
                {
                    return boolValue;
                }

                if (bool.TryParse(InvariantCultureToString(obj), out bool parsedValue))
                {
                    return parsedValue;
                }
            }

            return defaultValue;
        }

        public static List<object> GetListFromDictionary(IDictionary<string, object> dictionary, string key, List<object> defaultValue = null)
        {
            if (dictionary != null && dictionary.TryGetValue(key, out object value) && value is List<object> list)
            {
                return list;
            }

            return defaultValue;
        }

        public static string InvariantCultureToString(object obj)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", obj);
        }
    }
}