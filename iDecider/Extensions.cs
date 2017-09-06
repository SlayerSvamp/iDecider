using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace iDecider
{
    public static class Extensions
    {
        public static string AsJson<T>(this T obj) where T : new()
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);
            return JsonConvert.DeserializeObject<T>(json) ;
        }
    }
}