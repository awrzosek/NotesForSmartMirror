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

namespace SimpleNotes
{
    public class Notes
    {
        [PrimaryKey, AutoIncrement]
        public int Note_ID { get; set; }
        public int tempNote_ID { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }

    }
}