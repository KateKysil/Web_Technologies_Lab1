using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace LibraryInfrastructure.Hubs
{
    public class ReadingListHub : Hub
    {
        public async Task SyncList(int listId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"list-{listId}");
        }

        public async Task UpdateList(int listId)
        {
            await Clients.OthersInGroup($"list-{listId}").SendAsync("ListUpdated", listId);
        }
    }
}
