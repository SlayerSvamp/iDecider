using Android.App;
using Android.Widget;
using System.Collections.Generic;
using System;
using Android.Views;
using System.Linq;
using Newtonsoft.Json;
using Android.Content;
using SQLite;
using System.IO;

namespace iDecider
{

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    public class MainActivity : Activity
    {
        ItemList ItemList { get; set; }

        protected override void OnSaveInstanceState(Android.OS.Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutString("ItemList", ItemList.AsJson());

        }
        protected override void OnCreate(Android.OS.Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            Action endOfCreate = () => { };
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            if (bundle == null)
            {
                if (!PreferenceTools.Load<bool>("iExist"))
                {
                    RequestedOrientation = Android.Content.PM.ScreenOrientation.Locked;
                    endOfCreate += () => {
                        var alert = new AlertDialog.Builder(this);
                        var text = new TextView(this);
                        alert.SetView(text);
                        alert.SetTitle("Welcome!");
                        text.Text = "To use this app, just list the options for your decision making and let me be the decider.";
                        text.SetPadding(48,48,48,48);
                        alert.SetCancelable(false);
                        alert.SetPositiveButton("Ok", (sender, e) => { RequestedOrientation = Android.Content.PM.ScreenOrientation.Unspecified; });
                        
                        alert.Show();
                    };
                    //first time starting app / if data is cleared
                    PreferenceTools.Save("iExist", true);
                    FirstTime();
                }
                var id = Intent.GetIntExtra("itemListId", -1);
                if (id < 0)
                {
                    id = PreferenceTools.Load("startupListId", -1);
                    using (var db = new SQLiteConnection(Database.Path))
                    {
                        if (!db.Table<ItemList>().Any(x => x.id == id))
                        {
                            id = db.Table<ItemList>().FirstOrDefault()?.id ?? -1;
                        }
                    }
                }
                PreferenceTools.Save("startupListId", id);
                if (id < 0)
                {
                    Finish();
                    StartActivity(typeof(ListManagerActivity));
                    return;
                }

                using (var db = new SQLiteConnection(Database.Path))
                {
                    ItemList = db.Get<ItemList>(id);
                }
            }
            else
            {
                ItemList = bundle.GetString("ItemList").FromJson<ItemList>();
            }

            ActionBar.Title = ItemList.Name;
            //has to be after loading ItemList
            CreateItemListView();
            RedrawList();
            endOfCreate();
        }
        public void FirstTime()
        {

            var items = new List<Item>();
            items.Add(new Item { Text = "Yes" });
            items.Add(new Item { Text = "No" });
            items.Add(new Item { Text = "Maybe", Active = false });

            var list = new ItemList()
            {
                Name = "Example list",
                JsonItems = items.AsJson()
            };
            using (var db = new SQLiteConnection(Database.Path))
            {
                db.CreateTable<ItemList>();
                db.Insert(list);
                PreferenceTools.Save("startupListId", list.id);
            }
        }
        public void CreateItemListView()
        {
            ListView list = FindViewById<ListView>(Android.Resource.Id.List);

            list.Adapter = new ItemListAdapter(this, ItemList);

            list.ItemClick += (sender, e) =>
            {
                var item = ItemList.Items[e.Position];
                item.Active = !item.Active;
                ItemList.SaveChanges();
                RedrawList();
            };
            list.ItemLongClick += (sender, e) =>
            {
                var item = ItemList.Items[e.Position];

                var alert = new AlertDialog.Builder(this);
                var text = new EditText(this) { Text = item.Text };

                alert.SetView(text);
                alert.SetTitle("Edit item");

                alert.SetPositiveButton("Done", (x, y) =>
                {
                    if (!string.IsNullOrWhiteSpace(text.Text))
                    {
                        item.Text = text.Text.Trim();
                        ItemList.SaveChanges();
                        RedrawList();
                    }
                    else
                        Toast.MakeText(this, "Name cannot be empty", ToastLength.Long).Show();

                });
                alert.SetNeutralButton("Cancel", (x, y) => { });
                alert.SetNegativeButton("Delete", (x, y) =>
                {
                    ItemList.Items.Remove(item);
                    ItemList.SaveChanges();
                    RedrawList();
                });

                alert.Show();
            };
        }
        public void RedrawList()
        {
            ActionBar.Title = ItemList.Name;
            var list = FindViewById<ListView>(Android.Resource.Id.List);
            (list.Adapter as ItemListAdapter).NotifyDataSetChanged();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_goto_list_manager:
                    Finish();
                    StartActivity(typeof(ListManagerActivity));
                    break;
                case Resource.Id.menu_go:
                    Go_Click();
                    break;
                case Resource.Id.menu_add_item:
                    Add_Click();
                    break;
                case Resource.Id.menu_rename:
                    Rename_Click();
                    break;
                default:
                    Toast.MakeText(this, "Error", ToastLength.Long).Show();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void Go_Click()
        {
            if (ItemList.HasActiveItems)
            {
                ItemList.Select();
                var alert = new AlertDialog.Builder(this);
                RedrawList();

                alert.SetTitle(ItemList.Selected.Text);
                alert.SetPositiveButton("Ok", (x, y) => { });
                alert.Show();
            }
            else
            {
                Toast.MakeText(this, "No available items", ToastLength.Long).Show();
            }

        }
        public void Add_Click()
        {
            var alert = new AlertDialog.Builder(this);
            var text = new EditText(this);

            alert.SetTitle("Add item");
            alert.SetPositiveButton("Done", (senderAlert, args) =>
            {
                text.Text = text.Text.Trim();
                if (!string.IsNullOrWhiteSpace(text.Text))
                {
                    ItemList.Items.Add(new Item { Text = text.Text.Trim() });
                    ItemList.SaveChanges();
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
        private void Rename_Click()
        {
            var alert = new AlertDialog.Builder(this);
            var text = new EditText(this) { Text = ItemList.Name };
            alert.SetTitle("Rename list");
            alert.SetView(text);
            alert.SetNeutralButton("Cancel", (sender, e) => { });
            alert.SetPositiveButton("Done", (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(text.Text))
                {
                    ItemList.Name = text.Text.Trim();
                    ItemList.SaveChanges();
                    RedrawList();
                }
                else
                {
                    Toast.MakeText(this, "Name cannot be empty", ToastLength.Long).Show();
                }
            });
            alert.Show();
        }
    }
}

