using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WebDownload
{
    public class ChatHub:Hub
    {
        //发送消息--发送给所有连接的客户端
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        //发送消息--发送给指定用户
        public Task SendPrivateMessage(string userId, string message)
        {
            return Clients.User(userId).SendAsync("ReceiveMessage", message);
        }
    }
}
