using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LibraryInfrastructure.Hubs
{
    public class BoardHub : Hub
    {
        public Task Draw(object stroke) => Clients.Others.SendAsync("ReceiveStroke", stroke);

        public Task ClearBoard() => Clients.All.SendAsync("BoardCleared");

        public Task AddNote(object note) => Clients.Others.SendAsync("NoteAdded", note);
        public Task UpdateNote(object note) => Clients.Others.SendAsync("NoteUpdated", note);
        public Task MoveNote(object note) => Clients.Others.SendAsync("NoteMoved", note);
        public Task DeleteNote(int id) => Clients.Others.SendAsync("NoteDeleted", id);
    }
}
