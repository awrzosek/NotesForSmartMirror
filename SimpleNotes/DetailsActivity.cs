using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SimpleNotes.Services;

namespace SimpleNotes
{
    [Activity(Label = "")]
    class DetailsActivity : Activity
    {
        private string Uri = "http://smnotes.azurewebsites.net/api/notes/";

        private static Notes _note;
        public static Notes note
        {
            get
            {
                return _note;
            }
            set
            {
                _note = value;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Details);
            var simpleNotesService = new SimpleNotesService();

            note = new Notes()
            {
                Note_ID = Intent.GetIntExtra("ID", 0),
                sqlNote_ID = Intent.GetIntExtra("sqlID",0),
                Title = Intent.GetStringExtra("title"),
                Note = Intent.GetStringExtra("note")
            };
            EditText titleTV = FindViewById<EditText>(Resource.Id.title);
            titleTV.Text = note.Title;
            EditText noteTV = FindViewById<EditText>(Resource.Id.note);
            noteTV.Text = note.Note;
        }
        ///////////////////////////
        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_details, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        public async void Save(EditText titleTV, EditText noteTV)
        {
            if (note.Title == null && note.Note == null)
            {
                Notes newNote = new Notes()
                {
                    Title = titleTV.Text,
                    Note = noteTV.Text
                };
                await MainActivity.db.AddItemAsync(newNote);
                if (isOnline())
                {
                    Toast.MakeText(this, "Zapisywanie notatki na serwerze", ToastLength.Short).Show();
                    var simpleNotesService = new SimpleNotesService();
                    await simpleNotesService.PostNotesAsync(Uri, newNote);
                }
                    
            }
            else
            {
                note.Title = titleTV.Text;
                note.Note = noteTV.Text;
                await MainActivity.db.UpdateItemAsync(note);
                PUTNote putNote = new PUTNote()
                {
                    Note_ID = note.sqlNote_ID,
                    Title = note.Title,
                    Note = note.Note
                };
                if (isOnline())
                {
                    Toast.MakeText(this, "Zapisywanie notatki na serwerze", ToastLength.Short).Show();
                    var simpleNotesService = new SimpleNotesService();
                    await simpleNotesService.PutNotesAsync(Uri, note.sqlNote_ID, putNote);
                }
                
            }

            NotesListViewAdapter adapter = new NotesListViewAdapter(this, await MainActivity.db.GetItemsAsync());
            MainActivity.NotesListView.Adapter = adapter;
            MainActivity.notes = await MainActivity.db.GetItemsAsync();
            if (isOnline())
            {
                var simpleNotesService = new SimpleNotesService();
                List<Notes> temp = await simpleNotesService.GetNotesAsync(Uri);
                for (int i = 0; i < MainActivity.notes.Count; i++)
                {
                    MainActivity.notes[i].sqlNote_ID = temp[i].Note_ID;
                }
            }
            Finish();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.saveNote)
            {
                EditText titleTV = FindViewById<EditText>(Resource.Id.title);
                EditText noteTV = FindViewById<EditText>(Resource.Id.note);
                Save(titleTV, noteTV);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool isOnline()
        {
            ConnectivityManager cm =
                (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            return netInfo != null && netInfo.IsConnectedOrConnecting;
        }
    }
}