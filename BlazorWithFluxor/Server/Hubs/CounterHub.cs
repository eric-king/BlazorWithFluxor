using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlazorWithFluxor.Server.Hubs
{
    public class CounterHub : Hub
    {
        public async Task SendCount(int count)
        {
            await Clients.Others.SendAsync("ReceiveCount", count);
        }
    }
}
