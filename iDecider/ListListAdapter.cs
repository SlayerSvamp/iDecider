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