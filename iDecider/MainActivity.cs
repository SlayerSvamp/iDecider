using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using Android.Views;
using System.Linq;
using Newtonsoft.Json;
using Android.Content;

namespace iDecider
{

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        string ListNamesKey { get { return Resources.GetString(Resource.String.list_names); } }
        List<Alternative> alternatives { get; set; }

        protected override void OnSaveInstanceState(Bundle outState)
        {

            base.OnSaveInstanceState(outState);
            var alts = JsonConvert.SerializeObject(alternatives);

            outState.PutString("alts", alts);

        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (bundle == null)
            {
                var firstTime = !LoadPreference<bool>("iExist");
                if (firstTime)
                {
                    //first time starting app
                    SaveList(ListNamesKey, "");
                    SavePreference("iExist", true);
                }

                var listNames = LoadPreference<string>(ListNamesKey).FromJson<List<string>>();
                var loaded = LoadList(listNames.First());
            }
            else
            {
                alternatives = bundle.GetString("alts").FromJson<List<Alternative>>();
            }
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = Resources.GetString(Resource.String.app_name);
            CreateAlternativesListView();
        }
        public void CreateAlternativesListView()
        {
            ListView list = FindViewById<ListView>(Android.Resource.Id.List);

            list.Adapter = new AlternativeListAdapter(this, alternatives);

            list.ItemClick += (item, args) =>
            {
                LinearLayout layout = new LinearLayout(this);
                layout.Orientation = Orientation.Vertical;

                var alt = alternatives[args.Position];
                var alert = new AlertDialog.Builder(this);
                var text = new EditText(this);
                var slider = new Switch(this);

                slider.SetPadding(30, 30, 30, 30);

                alert.SetTitle("Edit");
                text.Text = alt.Text;
                slider.Text = "Active";
                slider.Checked = alt.Active;

                layout.AddView(text);
                layout.AddView(slider);
                alert.SetView(layout);

                alert.SetPositiveButton("Done", (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(text.Text.Trim()))
                        alt.Text = text.Text.Trim();
                    alt.Active = slider.Checked;
                    RedrawAlternatives();
                });
                alert.SetNeutralButton("Cancel", (sender, e) => { });
                alert.SetNegativeButton("Delete", (sender, e) =>
                {
                    alternatives.Remove(alt);
                    RedrawAlternatives();
                });

                alert.Show();
            };
        }
        public void Go()
        {
            var alert = new AlertDialog.Builder(this);
            if (alternatives.Any(a => a.Active))
            {
                alternatives.ForEach(a => a.Selected = false);
                var alt = alternatives.Where(a => a.Active).OrderBy(x => Guid.NewGuid()).First();

                alt.Selected = true;
                RedrawAlternatives();

                alert.SetTitle(alt.Text);
                alert.SetPositiveButton("Ok", (x, y) => { });
                alert.Show();

            }
            else
            {
                Toast.MakeText(this, "No alternatives available!", ToastLength.Long).Show();
            }

        }
        public void PromptAddAlternative()
        {
            var text = new EditText(this);
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("New");
            alert.SetPositiveButton("Done", (senderAlert, args) =>
            {
                text.Text = text.Text.Trim();
                if (string.IsNullOrWhiteSpace(text.Text))
                {
                    Toast.MakeText(this, "Input cannot be empty", ToastLength.Long).Show();
                }
                else
                {
                    alternatives.Add(new Alternative { Text = text.Text.Trim() });
                    RedrawAlternatives();
                }

            });
            alert.SetNeutralButton("Cancel", (senderAlert, args) => Toast.MakeText(this, $"Cancelled", ToastLength.Short).Show());
            alert.SetView(text);
            alert.Show();
        }

        public void RedrawAlternatives()
        {
            var list = FindViewById<ListView>(Android.Resource.Id.List);
            (list.Adapter as AlternativeListAdapter).NotifyDataSetChanged();
        }
        public void RemoveAlternatives()
        {
            alternatives.Clear();
            RedrawAlternatives();
        }
        public void SaveList(string name, string listAsJson)
        {
            var listkey = Resources.GetString(Resource.String.list_names);
            var listnames = LoadPreference<List<string>>(ListNamesKey);
            listnames.Add(name);
            SavePreference(ListNamesKey, listnames.AsJson());

            var key = $"list:{name}";
            if (alternatives.Count > 0)
                SavePreference(key, listAsJson);
            else
                RemovePreference(key);
        }
        public List<Alternative> LoadList(string name)
        {
            var key = $"list:{name}";
            return LoadPreference<List<Alternative>>(key) ?? new List<Alternative>();
        }
        public void SavePreference<T>(string key, T value)
        {
            var prefs = GetSharedPreferences(PackageName, FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            if (typeof(T) == typeof(string))
                prefEditor.PutString(key, (dynamic)value);
            else if (typeof(T) == typeof(bool))
                prefEditor.PutBoolean(key, (dynamic)value);
            else if (typeof(T) == typeof(int))
                prefEditor.PutInt(key, (dynamic)value);
            else if (typeof(T) == typeof(float))
                prefEditor.PutFloat(key, (dynamic)value);
            else
                prefEditor.PutString(key, JsonConvert.SerializeObject(value));
            prefEditor.Commit();

        }
        public void RemovePreference(string key)
        {
            var prefs = GetSharedPreferences(PackageName, FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.Remove(key);
            prefEditor.Commit();

        }
        public T LoadPreference<T>(string key)
        {
            var prefs = GetSharedPreferences(PackageName, FileCreationMode.Private);
            if (typeof(T) == typeof(string))
                return (dynamic)prefs.GetString(key, string.Empty);
            else if (typeof(T) == typeof(bool))
                return (dynamic)prefs.GetBoolean(key, false);
            else if (typeof(T) == typeof(int))
                return (dynamic)prefs.GetInt(key, 0);
            else if (typeof(T) == typeof(float))
                return (dynamic)prefs.GetFloat(key, 0.0f);
            else
                return prefs.GetString(key, string.Empty).FromJson<T>();
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
                case Resource.Id.menu_addAlt:
                    PromptAddAlternative();
                    break;
                case Resource.Id.menu_go:
                    Go();
                    break;
                case Resource.Id.menu_removeAlts:
                    RemoveAlternatives();
                    break;
                case Resource.Id.menu_save:
                    var alert = new AlertDialog.Builder(this);
                    alert.SetTitle("List name");
                    var text = FindViewById<TextView>(Android.Resource.Id.Text1);
                    text.Text = "";
                    alert.SetView(text);
                    alert.SetNeutralButton("Cancel", (sender, e) => { });
                    alert.SetPositiveButton("Save", (sender, e) =>
                    {
                        string toastMessage;
                        if (!string.IsNullOrWhiteSpace(text.Text))
                        {
                            SaveList(text.Text.Trim(), alternatives.AsJson());
                            toastMessage = $"'{text.Text.Trim()}' saved";
                        }
                        else
                        {
                            toastMessage = "List name cannot be empty";
                        }
                        Toast.MakeText(this, toastMessage, ToastLength.Long).Show();
                    });
                    alert.Show();
                    break;
                default:
                    Toast.MakeText(this, "Error", ToastLength.Short).Show();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}

