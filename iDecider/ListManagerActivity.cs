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
using Android.Content.PM;

namespace iDecider
{
    [Activity(Label = "iDecider - List Manager", LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait)]
    public class ListManagerActivity : Activity
    {
        List<ItemList> Lists { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.list_manager);
            Title = "Lists";
            TitleColor = Android.Graphics.Color.White;

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar_list_manager);
            SetActionBar(toolbar);

            CreateListView();
        }
        protected override void OnResume()
        {
            base.OnResume();
            LoadList();
        }
        public void CreateListView()
        {
            ListView view = LoadList();

            view.ItemClick += (sender, e) =>
            {
                var list = Lists[e.Position];
                Intent mainActivity = new Intent(this, typeof(MainActivity));
                mainActivity.PutExtra("itemListId", list.id);
                StartActivity(mainActivity);
            };
            view.ItemLongClick += (sender, e) =>
            {
                var list = Lists[e.Position];

                var alert = new AlertDialog.Builder(this);
                var text = new EditText(this) { Text = list.Name };

                alert.SetView(text);

                alert.SetTitle("Rename list");

                alert.SetPositiveButton("Done", (x, y) =>
                {
                    if (!string.IsNullOrWhiteSpace(text.Text))
                        list.Name = text.Text.Trim();
                    else
                        Toast.MakeText(this, "Name cannot be empty", ToastLength.Long).Show();

                    list.SaveChanges();
                    RedrawList();
                });
                alert.SetNeutralButton("Cancel", (x, y) => { });
                alert.SetNegativeButton("Delete", (x, y) =>
                {
                    using (var db = new SQLiteConnection(Database.Path))
                    {
                        db.Delete(list);
                        Lists.Remove(list);
                    }
                    RedrawList();
                });

                alert.Show();
            };
        }
        public ListView LoadList()
        {
            using (var db = new SQLiteConnection(Database.Path))
            {
                Lists = db.Table<ItemList>().ToList();
                ListView view = FindViewById<ListView>(Resource.Id.list_list);
                view.Adapter = new ListListAdapter(this, Lists);
                return view;
            }
        }
        public void RedrawList()
        {
            var list = FindViewById<ListView>(Resource.Id.list_list);
            (list.Adapter as ListListAdapter).NotifyDataSetChanged();
        }

        public void Add_Click()
        {
            var alert = new AlertDialog.Builder(this);
            var text = new EditText(this);

            alert.SetTitle("Add list");
            alert.SetPositiveButton("Done", (senderAlert, args) =>
            {
                text.Text = text.Text.Trim();
                if (!string.IsNullOrWhiteSpace(text.Text))
                {
                    using (var db = new SQLiteConnection(Database.Path))
                    {
                        db.Insert(new ItemList { Name = text.Text.Trim() });
                        Lists.Clear();
                        Lists.AddRange(db.Table<ItemList>().ToList());

                    }
                    RedrawList();
                }
                else
                {
                    Toast.MakeText(this, "Name cannot be empty", ToastLength.Long).Show();
                }

            });
            alert.SetNeutralButton("Cancel", (senderAlert, args) => { });
            alert.SetView(text);

            alert.Show();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus_list_manager, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_add_list:
                    Add_Click();
                    break;
                default:
                    Toast.MakeText(this, "Error", ToastLength.Long).Show();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}