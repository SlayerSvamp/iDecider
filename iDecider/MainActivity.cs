using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using Android.Views;
using System.Linq;

namespace iDecider
{
    public static class Persistents
    {
        public static List<Item> items = new List<Item>();
    }
    public class Item
    {
        public string Text { get; set; }
        public bool Active { get; set; } = true;
    }
    class ItemListAdapter : BaseAdapter<Item>
    {
        List<Item> items { get { return Persistents.items; } }
        Activity context;
        public ItemListAdapter(Activity context, List<Item> items) : base()
        {
            this.context = context;
            Persistents.items = items;
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
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = items[position].Text;
            return view;
        }
    }


    [Activity(Label = "iDecider", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        List<Item> items { get { return Persistents.items; } }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "iDecider";
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var btnGo = FindViewById<Button>(Resource.Id.btnGo);
            var btnAdd = FindViewById<Button>(Resource.Id.btnAddItem);
            ListView list = FindViewById<ListView>(Android.Resource.Id.List);

            list.Adapter = new ItemListAdapter(this, items);
            

            btnGo.Click += (obj, args) =>
            {
                if (items.Count > 0)
                {
                    var chosen = items.OrderBy(x => Guid.NewGuid()).First().Text;
                    Toast.MakeText(this, chosen, ToastLength.Short).Show();
                }
                else
                    Toast.MakeText(this, new string[] { "Yes", "No" }.OrderBy(x => Guid.NewGuid()).First(), ToastLength.Short).Show();
            };
            btnAdd.Click += (x, y) =>
            {
                PromptAddItem(list);
            };
        }
        public void PromptAddItem(ListView list)
        {
            var txtItem = new EditText(this);
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Enter alternatives:");
            alert.SetPositiveButton("Done", (senderAlert, args) =>
            {
                string text = txtItem.Text.Trim();
                string toastMessage;
                if (text.Length > 0)
                {
                    items.Add(new Item { Text = text});
                    (list.Adapter as ItemListAdapter).NotifyDataSetChanged();
                    toastMessage = $"'{text}' added";
                }
                else
                {
                    toastMessage = "Input cannot be empty";
                }

                Toast.MakeText(this, toastMessage, ToastLength.Short).Show();
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args) => Toast.MakeText(this, $"Cancelled", ToastLength.Short).Show());
            alert.SetView(txtItem);
            txtItem.Text = "";
            alert.Show();
        }
    }
}

