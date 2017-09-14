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
    [Serializable()]
    public class Item
    {
        public string Text { get; set; }
        public bool Active { get; set; } = true;
    }
}