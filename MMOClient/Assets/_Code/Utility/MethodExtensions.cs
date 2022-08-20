using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Utility
{
    public static class MethodExtensions
    {
        public static string RemoveQuotes(this string Value)
        {
            return Value.Replace("\"", "");
        }

        public static float JSONObjectToFloat(this JSONObject Value)
        {
            return System.Convert.ToSingle(Value.ToString().Replace(".", ",").RemoveQuotes());
        }

        public static int JSONObjectToInt(this JSONObject Value)
        {
            return System.Convert.ToInt16(Value.ToString().Replace(".", ",").RemoveQuotes());
        }
    }
}