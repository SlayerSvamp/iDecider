using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using Android.Views;
using System.Linq;
using Android.Views.InputMethods;
using Android.Graphics;

namespace iDecider
{
    public static class Persistents
    {
        public static List<Alternative> alternatives = new List<Alternative>();
    }
    public class Alternative
    {
        public string Text { get; set; }
        public bool Active { get; set; } = true;
    }
    class AlternativeListAdapter : BaseAdapter<Alternative>
    {
        List<Alternative> alternatives { get { return Persistents.alternatives; } }
        Activity context;
        public AlternativeListAdapter(Activity context, List<Alternative> alternatives) : base()
        {
            this.context = context;
            Persistents.alternatives = alternatives;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override Alternative this[int position]
        {
            get { return alternatives[position]; }
        }
        public void Add(Alternative alternative)
        {
            alternatives.Add(alternative);
        }
        public override int Count
        {
            get { return alternatives.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            var txt = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            txt.Text = alternatives[position].Text;
            var color = alternatives[position].Active ? Color.Black : Color.Gray;
            txt.SetTextColor(color);

            return view;
        }
    }


    [Activity(Label = "iDecider", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        List<Alternative> alternatives { get { return Persistents.alternatives; } }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Title = "iDecider";
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetActionBar(toolbar);
            //ActionBar.Title = "iDecider";

            var btnGo = FindViewById<Button>(Resource.Id.btnGo);
            var btnAdd = FindViewById<Button>(Resource.Id.btnAddAlternative);
            ListView list = FindViewById<ListView>(Android.Resource.Id.List);

            list.Adapter = new AlternativeListAdapter(this, alternatives);

            list.ItemClick += (item, args) =>
            {
                LinearLayout layout = new LinearLayout(this);
                layout.Orientation = Orientation.Vertical;

                var alt = alternatives[args.Position];
                var alert = new AlertDialog.Builder(this);
                alert.SetTitle("Edit alternative");
                var text = new EditText(this);
                text.Text = alt.Text;
                var slider = new Switch(this);
                slider.Text = "Activated";
                slider.Checked = alt.Active;
                slider.SetPadding(30, 30, 30, 30);
                layout.AddView(text);
                layout.AddView(slider);
                alert.SetView(layout);
                alert.SetPositiveButton("Done", (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(text.Text.Trim()))
                        alt.Text = text.Text.Trim();
                    alt.Active = slider.Checked;
                    (list.Adapter as AlternativeListAdapter).NotifyDataSetChanged();
                });
                alert.SetNeutralButton("Cancel", (sender, e) => { });
                alert.SetNegativeButton("Delete", (sender, e) =>
                {
                    alternatives.Remove(alt);
                    (list.Adapter as AlternativeListAdapter).NotifyDataSetChanged();
                });
                alert.Show();
            };

            btnGo.Click += (obj, args) =>
            {
                var alert = new AlertDialog.Builder(this);
                string result;
                if (alternatives.Any(a => a.Active))
                    result = alternatives.Where(a => a.Active).OrderBy(x => Guid.NewGuid()).First().Text;
                else
                {
                    result = (Guid.NewGuid().ToByteArray().First()) % 2 == 0 ? "Yes" : "No";
                    alert.SetMessage("Default Yes/No when no items in the list are Activated");
                }

                alert.SetTitle(result);
                alert.SetPositiveButton("OK", (x, y) => { });
                alert.Show();
            };
            btnAdd.Click += (x, y) =>
            {
                PromptAddAlternative(list);
            };
        }
        public void PromptAddAlternative(ListView list)
        {
            var txtAlternative = new EditText(this);
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("Enter alternative:");
            alert.SetPositiveButton("Done", (senderAlert, args) =>
            {
                string text = txtAlternative.Text.Trim();
                string toastMessage;
                if (text.Length == 0)
                    toastMessage = "Input cannot be empty";
                //else if (alternatives.Any(a => a.Text == text))
                //    toastMessage = "Alternative already in list";
                else
                {

                    alternatives.Add(new Alternative { Text = text });
                    (list.Adapter as AlternativeListAdapter).NotifyDataSetChanged();
                    toastMessage = $"'{text}' added";
                }

                Toast.MakeText(this, toastMessage, ToastLength.Short).Show();
            });
            alert.SetNeutralButton("Cancel", (senderAlert, args) => Toast.MakeText(this, $"Cancelled", ToastLength.Short).Show());
            alert.SetView(txtAlternative);
            txtAlternative.Text = "";
            alert.Show();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Toast.MakeText(this, "Action selected: " + item.TitleFormatted,
                ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }
    }
}

