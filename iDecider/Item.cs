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
using Android.Graphics;
using Newtonsoft.Json;
using SQLite;

namespace iDecider
{
    [Serializable()]
    public class Item
    {
        public string Text { get; set; }
        public bool Active { get; set; } = true;
    }
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
        public ItemList Select()
        {
            Selected = Items.Where(x => x.Active).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            return this;
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

    class ItemListAdapter : BaseAdapter<Item>
    {
        List<Item> items { get { return itemList.Items; } }
        ItemList itemList { get; set; }
        Activity context;
        public ItemListAdapter(Activity context, ItemList itemList) : base()
        {
            this.context = context;
            this.itemList = itemList;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override Item this[int position]
        {
            get { return items[position]; }
        }
        public void Add(Item item)
        {
            items.Add(item);
        }
        public override int Count
        {
            get { return items?.Count ?? 0; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            var txt = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            txt.TextSize = 18;
            var item = items[position];
            txt.Text = item.Text;
            var selected = itemList.Selected == item;

            var color = item.Active ? (selected ? Color.ForestGreen : Color.Black) : Color.LightGray;

            txt.SetTextColor(color);

            return view;
        }
    }
    class ListListAdapter : BaseAdapter<ItemList>
    {
        List<ItemList> lists { get; set; }
        Activity context;
        public ListListAdapter(Activity context, List<ItemList> lists) : base()
        {
            this.context = context;
            this.lists = lists;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override ItemList this[int position]
        {
            get { return lists[position]; }
        }
        public void Add(ItemList list)
        {
            lists.Add(list);
        }
        public override int Count
        {
            get { return lists?.Count ?? 0; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            var txt = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            txt.TextSize = 18;
            var list = lists[position];
            txt.Text = list.Name;

            return view;
        }
    }
}