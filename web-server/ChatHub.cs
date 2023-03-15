using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using web_server.DbContext;
using web_server.Models;

namespace web_server
{
    [web_server.Models.Authorize]
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {

        public async Task SetChat(string userId, int with)
        {
            if (TestData.LiveChats.FirstOrDefault(m => m.UserId == userId) != null)
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
            var currChatUser = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(Context.ConnectionId));
            var currUser = TestData.UserList.FirstOrDefault(m => m.UserId == currChatUser.UserId);
            await SetChat(currChatUser.UserId.ToString(), Convert.ToInt32(userId));

            var user = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(userId));

            if (user == null) { await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow"); return; }
            var messages = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(Context.ConnectionId)).Messages.ToList();
            var result = new List<Messages>();

            await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow");

            foreach (var message in messages)
            {
                if (user.ConnectionTokens.ContainsKey(message.SenderId) || user.ConnectionTokens.ContainsKey(message.ReceiverId))
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
                var user2 = TestData.UserList.FirstOrDefault(m=>m.UserId== user.UserId);
                if (user.ConnectionTokens.ContainsKey(item.SenderId) || user.UserId == Convert.ToInt32(item.SenderId))
                {
                    if(item.FilePath == null)
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", item.Message, userId, user2.FirstName + " " + user2.LastName, null, user2.PhotoUrl);
                    }
                    else
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", item.Message, userId, user2.FirstName + " " + user2.LastName, item.FilePath, user2.PhotoUrl);
                    }
                }
                else
                {
                    if(item.FilePath == null)
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("OwnMessage", item.Message, null, currUser.PhotoUrl);
                    }
                    else
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("OwnMessage", item.Message, item.FilePath, currUser.PhotoUrl);
                    }
                }
            }
        }

        public async Task SendMessage(string message, string token, string filePath)
        {
            var user = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(token));
            var own = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(Context.ConnectionId));
            var ownPhoto = TestData.UserList.FirstOrDefault(m => m.UserId == own.UserId).PhotoUrl;
            if (user == null) { return; }

            var active = user.ConnectionTokens.Where(m => m.Value == "Connected").ToList();
            foreach (var item in active)
            {
                var user2 = TestData.UserList.FirstOrDefault(m=>m.UserId == user.UserId);
                if (TestData.LiveChats.FirstOrDefault(m => m.UserId == token).WithUserId == own.UserId.ToString())
                {
                    if(filePath == null)
                    {
                        await Clients.Clients(item.Key).SendAsync("ReceiveMessage", message, own.UserId, user2.FirstName + " " + user2.LastName, null, ownPhoto);
                    }
                    else
                    {
                        await Clients.Clients(item.Key).SendAsync("ReceiveMessage", message, own.UserId, user2.FirstName + " " + user2.LastName, filePath, ownPhoto);
                    }
                }
            }

            if(filePath == null)
            {
                user.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
                own.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
            }
            else
            {
                user.Messages.Add(new Messages() { Message = message, FilePath=filePath, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
                own.Messages.Add(new Messages() { Message = message, FilePath = filePath, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });

            }

            foreach (var item in own.ConnectionTokens.Where(m => m.Value == "Connected"))
            {
                if(filePath == null)
                {
                    await Clients.Clients(item.Key).SendAsync("OwnMessage", message, null, ownPhoto);
                }
                else
                {
                    await Clients.Clients(item.Key).SendAsync("OwnMessage", message, filePath, ownPhoto);
                }
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
            var photo = TestData.UserList.FirstOrDefault(m=>m.UserId == userId).PhotoUrl;
            if (TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId) == null)
            {
                var user = TestData.UserList.FirstOrDefault(m => m.UserId == userId);
                TestData.ChatUsers.Add(new ChatUser() { UserId = userId, ConnectionTokens = new System.Collections.Generic.Dictionary<string, string>() { { connectionId, "Connected" } } });
                
                SetNewContact(userId);

            }
            else
            {
                TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId).ConnectionTokens.Add(connectionId, "Connected");
            }
           
            GetContacts(userId);

            foreach (var item in TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId).Messages)
            {
                var who = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(item.SenderId));
                if (who == null)
                {
                    who = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(item.SenderId));
                }
            }

            SetChat(userId.ToString(), 0);

            return base.OnConnectedAsync();
        }
        public async Task SendMessageWithFile(FileAttachment attachment, string to, string message)
        {
            var fileBytes = Convert.FromBase64String(attachment.content);
            File.WriteAllBytes($"wwwroot/files/{attachment.fileName}", fileBytes);
            await SendMessage(message,to, $"{attachment.fileName}");
        }
        public async Task SetNewContact(int userId)
        {
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == userId);

            List<Schedule> userSchediles = new List<Schedule>();

            if (user.Role == "Student")
            {
                userSchediles = TestData.Schedules.Where(m => m.UserId == userId).ToList();
            }
            else if (user.Role == "Tutor")
            {
                userSchediles = TestData.Schedules.Where(m => m.TutorId == userId).ToList();
            }
            else
            {
                userSchediles = TestData.Schedules;
            }

            var manager = TestData.Managers.First();
            if(manager.UserId != userId)
            {
                TestData.ChatUsers.FirstOrDefault(m => m.UserId == manager.UserId)?.Contacts?.Add(new Contact() { UserId = userId });
                var active = TestData.ChatUsers.FirstOrDefault(m => m.UserId == manager.UserId)?.ConnectionTokens?.Where(m=>m.Value == "Connected");

                if(active != null)
                {
                    foreach (var item in active)
                    {
                        await Clients.Client(item.Key).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                    }
                }
            }

            var chatUser = TestData.ChatUsers.FirstOrDefault(m => m.UserId == user.UserId);

            if (user.Role == "Manager")
            {   
                foreach (var item in userSchediles)
                {
                    var student = TestData.UserList.FirstOrDefault(m=>m.UserId == item.UserId);
                    var tutor = TestData.UserList.FirstOrDefault(m => m.UserId == item.TutorId);

                    var userContct = new Contact()
                    {
                        UserId = item.UserId
                    };

                    var tutorContact = new Contact()
                    {
                       UserId = item.TutorId
                    };


                    if (chatUser.Contacts.FirstOrDefault(m=>m.UserId == userContct.UserId) == null)
                    {
                        chatUser.Contacts.Add(userContct);
                    }


                    if (chatUser.Contacts.FirstOrDefault(m => m.UserId == tutorContact.UserId) == null)
                    {
                        chatUser.Contacts.Add(tutorContact);
                    }
                }

                return;
            }

            foreach (var item in userSchediles)
            {
                var contact = new Contact()
                {
                    UserId = item.TutorId
                };

               
                if (user.UserId != item.UserId)
                {
                    if (TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.UserId) == null)
                    {
                        continue;
                    }

                    if (TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.UserId).Contacts.FirstOrDefault(m => m.UserId == contact.UserId) == null)
                    {
                        chatUser = TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.UserId);
                        chatUser.Contacts.Add(contact);

                        foreach (var token in chatUser.ConnectionTokens.Where(m=>m.Value == "Connected"))
                        {
                            await Clients.Client(token.Key).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                        }

                        TestData.ChatUsers.FirstOrDefault(m => m.UserId == user.UserId).Contacts.Add(new Contact() { UserId = item.UserId });

                    }
                }
                
                contact = new Contact()
                {
                    UserId = item.UserId
                };

                if (user.UserId != item.TutorId)
                {
                    var tutor = TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.TutorId);
                    if (tutor == null)
                    {
                        continue;
                    }

                    if (tutor.Contacts.FirstOrDefault(m => m.UserId == contact.UserId) == null)
                    {
                        chatUser = TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.TutorId);

                        chatUser.Contacts.Add(contact);

                        foreach (var token in chatUser.ConnectionTokens.Where(m => m.Value == "Connected"))
                        {
                            await Clients.Client(token.Key).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                        }

                        TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.TutorId).Contacts.Add(contact);
                        TestData.ChatUsers.FirstOrDefault(m => m.UserId == user.UserId).Contacts.Add(new Contact() { UserId = item.TutorId });

                    }
                }
               

            }

            var contact2 = new Contact()
            {
                UserId = manager.UserId
            };

            if(!TestData.ChatUsers.FirstOrDefault(m => m.UserId == user.UserId).Contacts.Contains(contact2) && manager.UserId != user.UserId)
            {
                TestData.ChatUsers.FirstOrDefault(m => m.UserId == user.UserId).Contacts.Add(contact2);
            }
        }
        public async Task GetContacts(int userId)
        {
            var chatUser = TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId);
            foreach (var item in chatUser.Contacts)
            {
                var user = TestData.UserList.FirstOrDefault(m => m.UserId == item.UserId);

                await Clients.Clients(Context.ConnectionId).SendAsync("UserList", item.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
            }

        }
        public override Task OnDisconnectedAsync(Exception ex)
        {
            var connectionId = Context.ConnectionId;
            var userId = Convert.ToInt32(Context.GetHttpContext().Request.Query["token"]);

            TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId).ConnectionTokens[connectionId] = "Disconnected";
            Clients.All.SendAsync("DisconnectUser", connectionId);

            return Task.CompletedTask;
        }

        public async Task OnlineUsers()
        {
            var connectionId = Context.ConnectionId;
            var who = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.ContainsKey(connectionId));
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == who.UserId);
            await Clients.All.SendAsync("UserList", connectionId, user.FirstName + " " + user.LastName, user.PhotoUrl);
        }
    }
}
