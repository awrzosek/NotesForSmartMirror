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

namespace SimpleNotes
{
    class NotesListViewAdapter : BaseAdapter<Notes>
    {
        public List<Notes> mNotes;
        private Context mContext;

        public NotesListViewAdapter(Context context, List<Notes> notes)
        {
            mContext = context;
            mNotes = notes;
        }

        public override int Count
        {
            get { return mNotes.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Notes this[int position]
        {
            get { return mNotes[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if(row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.listview_row, null, false);
            }

            TextView title = row.FindViewById<TextView>(Resource.Id.title);
            title.Text = mNotes[position].Title;

            TextView note = row.FindViewById<TextView>(Resource.Id.note);
            note.Text = mNotes[position].Note;

            return row;
        }
    }
}