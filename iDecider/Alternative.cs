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

namespace iDecider
{
    [Serializable()]
    public class Alternative
    {
        public string Text { get; set; }
        public bool Active { get; set; } = true;
        public bool Selected { get; set; } = false;
    }
    class AlternativeListAdapter : BaseAdapter<Alternative>
    {
        List<Alternative> alternatives { get; set; }
        Activity context;
        public AlternativeListAdapter(Activity context, List<Alternative> alternatives) : base()
        {
            this.context = context;
            this.alternatives = alternatives;
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
            get { return alternatives?.Count ?? 0; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            var txt = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            txt.TextSize = 18;
            var alt = alternatives[position];
            txt.Text = alt.Text;
            var color = alt.Active ? (alt.Selected ? Color.ForestGreen : Color.Black) : (alt.Selected ? Color.PaleGreen : Color.LightGray);
            txt.SetTextColor(color);

            return view;
        }
    }

}