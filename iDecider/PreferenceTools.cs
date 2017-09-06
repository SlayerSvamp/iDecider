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
    class PreferenceTools
    {
        private static ISharedPreferences GetPreferences { get { return Application.Context.GetSharedPreferences(Application.Context.PackageName, FileCreationMode.Private); } }
        public static void Save<T>(string key, T value)
        {
            var prefEditor = GetPreferences.Edit();
            if (typeof(T) == typeof(string))
                prefEditor.PutString(key, (string)(dynamic)value);
            else if (typeof(T) == typeof(bool))
                prefEditor.PutBoolean(key, (bool)(dynamic)value);
            else if (typeof(T) == typeof(int))
                prefEditor.PutInt(key, (int)(dynamic)value);
            else if (typeof(T) == typeof(float))
                prefEditor.PutFloat(key, (float)(dynamic)value);
            else
                prefEditor.PutString(key, JsonConvert.SerializeObject(value));
            prefEditor.Commit();

        }
        public static T Load<T>(string key, T defaultValue = default(T))
        {
            var prefs = GetPreferences;
            if (typeof(T) == typeof(string))
                return (T)(dynamic)prefs.GetString(key, (string)(dynamic)defaultValue);
            else if (typeof(T) == typeof(bool))
                return (T)(dynamic)prefs.GetBoolean(key, (bool)(dynamic)defaultValue);
            else if (typeof(T) == typeof(int))
                return (T)(dynamic)prefs.GetInt(key, (int)(dynamic)defaultValue);
            else if (typeof(T) == typeof(float))
                return (T)(dynamic)prefs.GetFloat(key, (float)(dynamic)defaultValue);
            else
                return prefs.GetString(key, string.Empty).FromJson<T>();
        }
        public static void RemovePreference(string key)
        {
            var prefEditor = GetPreferences.Edit();
            prefEditor.Remove(key);
            prefEditor.Commit();

        }
    }
}