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
    public class NotesHelper
    {
        public int Local_Note_ID { get; set; }
        public int Note_ID { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
    }
}