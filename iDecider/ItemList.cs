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
using SQLite;
using Newtonsoft.Json;

namespace iDecider
{
    public class ItemList
    {
        List<Item> items;
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }
        public int version { get; set; } = 1; //database model version
        public string Name { get; set; }
        public string JsonItems { get; set; }
        [Ignore]
        [JsonIgnore]
        public List<Item> Items { get { return items ?? (items = JsonItems.FromJson<List<Item>>() ?? new List<Item>()); } }
        [Ignore]
        public Item Selected { get; set; }
        [Ignore]
        [JsonIgnore]
        public bool HasActiveItems { get { return Items.Any(x => x.Active); } }
        public void Select()
        {
            using (var RNG = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var items = Items.Where(x => x.Active);
                var bytes = new byte[8];
                RNG.GetBytes(bytes);
                var rand = (BitConverter.ToUInt64(bytes, 0) / (1 << 11)) / (double)(1UL << 53);
                var index = (int)(items.Count() * rand);
                Selected = items.ElementAtOrDefault(index);
            }
        }
        public void SaveChanges()
        {
            Selected = null;
            JsonItems = Items.AsJson();
            using (var db = new SQLiteConnection(Database.Path))
            {
                db.Update(this);
            }
        }
    }
}