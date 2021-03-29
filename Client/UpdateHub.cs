using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

using Orleans;

namespace TodoListApi.Client.Hubs
{
    public class UpdateHub : Hub
    {

        IClusterClient _orleansClient;
        public UpdateHub(IClusterClient clusterClient) {
            this._orleansClient = clusterClient;
        }
        public async Task SendUpdateCall()
        {
            await Clients.All.SendAsync("updatePlease");
        }
    }
}