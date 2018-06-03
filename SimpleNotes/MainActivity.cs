using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using SimpleNotes.SQLite;
using System.IO;
using static Android.Widget.AdapterView;
using SimpleNotes.Services;
using Android.Net;

namespace SimpleNotes
{
    [Activity(Label = "", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private static ListView notesListView;
        public static ListView NotesListView {
            get
            {
                return notesListView;
            }
            set
            {
                notesListView = value;
            }
        }
        private static List<Notes> _notes;
        public static List<Notes> notes
        {
            get
            {
                return _notes;
            }
            set
            {
                _notes = value;
            }
        }

        private static LocalDB _db;
        public static LocalDB db
        {
            get
            {
                if(_db == null)
                {
                    _db = new LocalDB(Path.Combine(System.Environment
                .GetFolderPath(System.Environment.SpecialFolder.Personal), "notesDB.db3"));
                }
                return _db;
            }
        }
        public enum NetworkState
        {
            Unknown,
            ConnectedWifi,
            ConnectedData,
            Disconnected
        }
        ////////////
        private NetworkState _state;

        public NetworkState State
        {
            get
            {
                UpdateNetworkStatus();

                return _state;
            }
        }
        public void UpdateNetworkStatus()
        {
            _state = NetworkState.Unknown;

            // Retrieve the connectivity manager service
            var connectivityManager = (ConnectivityManager)
                Application.Context.GetSystemService(
                    Context.ConnectivityService);

            // Check if the network is connected or connecting.
            // This means that it will be available, 
            // or become available in a few seconds.
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;

            if (activeNetworkInfo.IsConnectedOrConnecting)
            {
                // Now that we know it's connected, determine if we're on WiFi or something else.
                _state = activeNetworkInfo.Type == ConnectivityType.Wifi ?
                    NetworkState.ConnectedWifi : NetworkState.ConnectedData;
            }
            else
            {
                _state = NetworkState.Disconnected;
            }
        }
        /////////////

        private string Uri = "http://smnotes.azurewebsites.net/api/notes/";

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            // this method is currently not working
            //Start();

            notesListView = FindViewById<ListView>(Resource.Id.notesListView);
            
            notes = await db.GetItemsAsync();

            NotesListViewAdapter adapter = new NotesListViewAdapter(this, notes);

            notesListView.Adapter = adapter;

            // api operations
            // operacje związane z api
            var simpleNotesService = new SimpleNotesService();
            if (isOnline())
            {
                // removing all notes from sql server database
                // usunięcie wszystkich notatek z serwera
                List<Notes> no = await simpleNotesService.GetNotesAsync(Uri);
                if (no != null)
                {
                    await simpleNotesService.DeleteAllNotesAsync("http://smnotes.azurewebsites.net/api/",
                        "deleteallnotes");
                }

                // if there are notes saved offline in local database
                // upload them to the sql server
                // jeżeli jakieś notatki były zapisane lokalnie bez dostępu do internetu
                // to należy je wysłać na serwer
                if (notes != null)
                {
                    foreach (Notes n in notes)
                    {
                        Notes note = new Notes() { Note_ID = n.Note_ID, Title = n.Title, Note = n.Note };
                        await simpleNotesService.PostNotesAsync(Uri, note);
                    }
                    GetSqlID();
                }
            }
            


            ///////////////////


            notesListView.ItemClick += delegate (object sender, ItemClickEventArgs args)
            {
                Intent details_activity = new Intent(this, typeof(DetailsActivity));
                details_activity.PutExtra("ID", notes[(int)args.Id].Note_ID);
                details_activity.PutExtra("sqlID", notes[(int)args.Id].sqlNote_ID);
                details_activity.PutExtra("title", notes[(int)args.Id].Title);
                details_activity.PutExtra("note", notes[(int)args.Id].Note);
                StartActivity(details_activity);
            };

            notesListView.ItemLongClick += delegate (object sender, ItemLongClickEventArgs args)
            {
                AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                AlertDialog alert = dialog.Create();
                alert.SetMessage("Czy na pewno chcesz usunąć tę notatkę?");
                alert.SetButton("OK", async (c, ev) =>
                {
                    Notes delNote = new Notes()
                    {
                        Note_ID = notes[(int)args.Id].Note_ID,
                        sqlNote_ID = notes[(int)args.Id].sqlNote_ID,
                        Title = notes[(int)args.Id].Title,
                        Note = notes[(int)args.Id].Note
                    };
                    await db.DeteleItemAsync(delNote);
                    notes.RemoveAt((int)args.Id);
                    // if there is an Internet connection, we have to delete selected note
                    // also from sql server database
                    // jeżeli jest połączenie z Internetem trzeba usunąć notatkę także z serwera
                    if (isOnline())
                    {
                        await simpleNotesService.DeleteNotesAsync(Uri, delNote.sqlNote_ID);
                    }
                    adapter = new NotesListViewAdapter(this, notes);

                    notesListView.Adapter = adapter;
                });
                alert.SetButton2("Anuluj", (c, ev) => { });
                alert.Show();
            };
        }

        // adding Note_ID value from sql server to local variable
        // dodanie do zmiennej przechowującej listę notatek z SQLite 
        // wartości Note_ID z bazy MSSQL
        public async void GetSqlID()
        {
            var simpleNotesService = new SimpleNotesService();
            List<Notes> temp = await simpleNotesService.GetNotesAsync(Uri);
            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].sqlNote_ID = temp[i].Note_ID;
            }
        }

        // this method checks if there is any Internet connection available
        public bool isOnline()
        {
            ConnectivityManager cm =
                (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            return netInfo != null && netInfo.IsConnectedOrConnecting;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if(id == Resource.Id.addNote)
            {
                //Toast.MakeText(this, "add clicked", ToastLength.Short).Show();
                Intent addNote = new Intent(this, typeof(DetailsActivity));
                this.InvalidateOptionsMenu();
                StartActivity(addNote);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }



        ///////
        public event EventHandler NetworkStatusChanged;
        private NetStatBroadcastReceiver _broadcastReceiver;
        public void Start()
        {
            if (_broadcastReceiver != null)
            {
                throw new InvalidOperationException(
                    "Network status monitoring already active.");
            }

            // Create the broadcast receiver and bind the event handler
            // so that the app gets updates of the network connectivity status
            _broadcastReceiver = new NetStatBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;

            // Register the broadcast receiver
            Application.Context.RegisterReceiver(_broadcastReceiver,
              new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            var currentStatus = _state;

            UpdateNetworkStatus();

            if (currentStatus != _state && NetworkStatusChanged != null) {
                NetworkStatusChanged(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (_broadcastReceiver == null)
            {
                throw new InvalidOperationException(
                    "Network status monitoring not active.");
            }

            // Unregister the receiver so we no longer get updates.
            Application.Context.UnregisterReceiver(_broadcastReceiver);

            // Set the variable to nil, so that we know the receiver is no longer used.
            _broadcastReceiver.ConnectionStatusChanged -= OnNetworkStatusChanged;
            _broadcastReceiver = null;
        }
    }
}

