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


namespace SimpleNotes.Services
{
    public class SimpleNotesService
    {
        public async Task<List<Notes>> GetNotesAsync(string Uri)
        {
            RESTClientService<Notes> restClient = new RESTClientService<Notes>();

            var notes = await restClient.GetListAsync(Uri);

            return notes;
        }

        public async Task PostNotesAsync(string Uri, Notes note)
        {
            RESTClientService<Notes> restClient = new RESTClientService<Notes>();

            var result = await restClient.PostAsync(Uri, note);
            
        }

        public async Task DeleteNotesAsync(string Uri, int id)
        {
            RESTClientService<PUTNote> restClient = new RESTClientService<PUTNote>();

            var result = await restClient.DeleteAsync(Uri, id);
        }

        public async Task DeleteAllNotesAsync(string Uri, string action)
        {
            RESTClientService<PUTNote> restClient = new RESTClientService<PUTNote>();

            var result = await restClient.DeleteAllAsync(Uri, action);
        }

        public async Task PutNotesAsync(string Uri, int id, PUTNote note)
        {
            RESTClientService<PUTNote> restClient = new RESTClientService<PUTNote>();

            var result = await restClient.PutAsync(Uri, id, note);
        }
    }
}