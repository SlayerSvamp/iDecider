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

namespace iDecider
{
    public static class Database
    {
        static string path;
        public static string Path
        {
            get
            {
                if (path == null)
                {
                    var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    var filename = "iDecider.db3";
                    path = System.IO.Path.Combine(folder, filename);
                }
                return path;
            }
        }
    }
}