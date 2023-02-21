using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.DbContext;
using web_server.Models;

namespace web_server
{
    [web_server.Models.Authorize]
    public class Class : Microsoft.AspNetCore.SignalR.Hub
    {

        public async Task SetChat(string userId, int with)
        {
            if(TestData.LiveChats.FirstOrDefault(m=>m.UserId == userId) != null)
            {
                TestData.LiveChats.FirstOrDefault(m => m.UserId == userId).WithUserId = with.ToString();
            }
            else
            {
                TestData.LiveChats.Add(new InChat()
                {

                    UserId = userId,
                    WithUserId = with.ToString()
                });
            }
      
        }

        public async Task GetMessages(string userId)
        {
            await SetChat(TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(Context.ConnectionId)).UserId.ToString(), Convert.ToInt32(userId));

            var user = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(userId));

            if (user == null) { await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow"); return; }

            var messages = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(Context.ConnectionId)).Messages.ToList();
            var result = new List<Messages>();

            await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow");

            foreach (var message in messages)
            {
                if(user.ConnectionTokens.ContainsKey(message.SenderId)|| user.ConnectionTokens.ContainsKey(message.ReceiverId))
                {
                    result.Add(message);
                }
                if (user.UserId == Convert.ToInt32(message.SenderId) || user.UserId == Convert.ToInt32(message.ReceiverId))
                {
                    result.Add(message);
                }
            }

            foreach (var item in result)
            {
                if(user.ConnectionTokens.ContainsKey(item.SenderId) || user.UserId == Convert.ToInt32(item.SenderId))
                {
                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", item.Message, userId);
                }
                else
                {
                    await Clients.Clients(Context.ConnectionId).SendAsync("OwnMessage", item.Message);
                }
            }
        }

        public async Task SendMessage(string message, string token)
        {
            var user = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(token));
            var own = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(Context.ConnectionId));

            if (user == null) { return; }

            var active = user.ConnectionTokens.Where(m => m.Value == "Connected").ToList();
            foreach (var item in active) 
            {
                if(TestData.LiveChats.FirstOrDefault(m=>m.UserId == token).WithUserId == own.UserId.ToString())
                {
                    await Clients.Clients(item.Key).SendAsync("ReceiveMessage", message, own.UserId);
                }
            }

            user.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId= user.UserId.ToString(), MessageTime = DateTime.Now});
            

            own.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });

            foreach (var item in own.ConnectionTokens.Where(m => m.Value == "Connected"))
            {
                await Clients.Clients(item.Key).SendAsync("OwnMessage", message, own.UserId);
            }
        }
        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var ctx = Context.GetHttpContext();
            if (!ctx.Request.Query.ContainsKey("token"))
            {
                return Task.CompletedTask; 
            }
            var userId = Convert.ToInt32(ctx.Request.Query["token"]);
            
            if (TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId) == null)
            {
                TestData.ChatUsers.Add(new ChatUser() { UserId = userId, ConnectionTokens = new System.Collections.Generic.Dictionary<string, string>() { {  connectionId , "Connected"} }  });
                SetNewContact(userId);

            }
            else
            {
                TestData.ChatUsers.FirstOrDefault(m=>m.UserId == userId).ConnectionTokens.Add(connectionId, "Connected");
            }
            GetContacts(userId);

            foreach (var item in TestData.ChatUsers.FirstOrDefault(m=>m.UserId == userId).Messages)
            {
                var who = TestData.ChatUsers.FirstOrDefault(m=>m.ConnectionTokens.ContainsKey(item.SenderId));
                if(who == null)
                {
                    who = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(item.SenderId));
                }
            }

            SetChat(userId.ToString(), 0);

            var ff = base.OnConnectedAsync();
            return ff;
        }

        public async Task SetNewContact(int userId)
        {           
            var user = TestData.UserList.FirstOrDefault(m=>m.UserId == userId);

            List<Schedule> userSchediles = new List<Schedule>();
            
            if(user.Role == "Student")
            {
                userSchediles = TestData.Schedules.Where(m => m.UserId == userId).ToList();
            }
            else
            {
                userSchediles =  TestData.Schedules.Where(m => m.TutorId == userId).ToList();
            }

            foreach (var item in userSchediles)
            {
                var contact = new Contact()
                {
                    DisplayName = item.TutorFullName,
                    UserId = item.TutorId
                };
                
                if(TestData.ChatUsers.FirstOrDefault(m=>m.UserId == item.UserId ) == null)
                {
                    continue;
                }

                if ( TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.UserId).Contacts.FirstOrDefault(m=>m.UserId == contact.UserId) == null)
                {
                    TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.UserId).Contacts.Add(contact);
                }

                contact = new Contact()
                {
                    DisplayName = item.UserName,
                    UserId = item.UserId
                };
                var tutor = TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.TutorId);
                if(tutor == null)
                {
                    continue;
                }

                if (tutor.Contacts.FirstOrDefault(m => m.UserId == contact.UserId) == null)
                {
                    TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.TutorId).Contacts.Add(contact);
                }   
            }
        }
        public async Task GetContacts(int userId)
        {
            var chatUser = TestData.ChatUsers.FirstOrDefault(m=>m.UserId == userId);
            foreach (var item in chatUser.Contacts)
            {
                await Clients.Clients(Context.ConnectionId).SendAsync("UserList", item.UserId, item.DisplayName);
            }

        }
        public override Task OnDisconnectedAsync(Exception ex)
        {
            var connectionId = Context.ConnectionId;
            var userId = Convert.ToInt32(Context.GetHttpContext().Request.Query["token"]);

            TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId).ConnectionTokens[connectionId] =  "Disconnected";
            Clients.All.SendAsync("DisconnectUser", connectionId);

            return Task.CompletedTask;
        }

        public async Task OnlineUsers()
        {
            var connectionId = Context.ConnectionId;
            var who = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(connectionId));

            await Clients.All.SendAsync("UserList", connectionId, who.UserId);
        }
    }
}
