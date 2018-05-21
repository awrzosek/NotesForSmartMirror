using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace SimpleNotes.SQLite
{
    public class LocalDB
    {
        readonly SQLiteAsyncConnection database;
        public LocalDB(string path)
        {
            database = new SQLiteAsyncConnection(path);
            database.CreateTableAsync<Notes>().Wait();
        }

        public Task<List<Notes>> GetItemsAsync()
        {
            return database.Table<Notes>().ToListAsync();
        }

        public Task<int> AddItemAsync(Notes note)
        {
            return database.InsertAsync(note);
        }

        public Task<int> UpdateItemAsync(Notes note)
        {
            return database.UpdateAsync(note);
        }

        public Task<int> DeteleItemAsync(Notes note)
        {
            return database.DeleteAsync(note);
        }
    }
}