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

namespace iDecider
{
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
}