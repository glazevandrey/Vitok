using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_server
{
    [web_server.Models.Authorize]
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        UserRepository _userRepository;
        ChatRepository _chatRepository;
        ScheduleRepository _scheduleRepository;
        public ChatHub(UserRepository userRepository, ChatRepository chatRepository, ScheduleRepository scheduleRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
        }
        public async Task SetChat(int userId, int with)
        {
            var user = await _chatRepository.GetChatUserByUserId(userId);
            if (user != null)
            {
                user.InChat.WithUserId = with;
                //TestData.LiveChats.FirstOrDefault(m => m.UserId == userId).WithUserId = with.ToString();
            }
            else
            {
                user.InChat = new InChat()
                {
                    WithUserId = with
                };
            }
            await _chatRepository.Update(user);
        }

        public async Task GetMessages(string userId)
        {
             var currChatUser =await  _chatRepository.GetChatUserByFunc(u => u.ConnectionTokens.Any(t => t.Token == Context.ConnectionId));
            //var currChatUser = TestData.ChatUsers.FirstOrDefault(u => u.ConnectionTokens.Any(t => t.Token == Context.ConnectionId));
            var currUser =await _userRepository.GetUserById(currChatUser.UserId);
            //var currUser = TestData.UserList.FirstOrDefault(m => m.UserId == currChatUser.UserId);
            await SetChat(currChatUser.UserId, Convert.ToInt32(userId));

            var user = await _chatRepository.GetChatUserByUserId(Convert.ToInt32(userId));
            //var user = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(userId));

            if (user == null) { await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow"); return; }
             
            var messages = currChatUser.Messages.ToList();
            var result = new List<Messages>();

            await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow");

            foreach (var message in messages)
            {
                if (user.ConnectionTokens.Any(t => t.Token == message.SenderId) || user.ConnectionTokens.Any(t => t.Token == message.ReceiverId))
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
                var user2 = await _userRepository.GetUserById(user.UserId);
                if (user.ConnectionTokens.Any(t => t.Token == item.SenderId) || user.UserId == Convert.ToInt32(item.SenderId))
                {
                    if (item.FilePath == null)
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
                    if (item.FilePath == null)
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
            var user = await _chatRepository.GetChatUserByUserId(Convert.ToInt32(token));
            //var user = TestData.ChatUsers.FirstOrDefault(m => m.UserId == Convert.ToInt32(token));
            var own =await  _chatRepository.GetChatUserByFunc(m => m.ConnectionTokens.Any(m => m.Token == Context.ConnectionId));
            //var own = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.Any(m=>m.Token == Context.ConnectionId));
            var ownPhoto = await _userRepository.GetUserById(own.UserId);
            //var ownPhoto = TestData.UserList.FirstOrDefault(m => m.UserId == own.UserId).PhotoUrl;
            if (user == null) { return; }

            var active = user.ConnectionTokens.Where(m => m.Status == "Connected").ToList();
            foreach (var item in active)
            {
                var user2 = await _userRepository.GetUserById(user.UserId);
                //var user2 = TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId);
                if (user.InChat.WithUserId == own.UserId)
                {
                    if (filePath == null)
                    {
                        await Clients.Clients(item.Token).SendAsync("ReceiveMessage", message, own.UserId, user2.FirstName + " " + user2.LastName, null, ownPhoto);
                    }
                    else
                    {
                        await Clients.Clients(item.Token).SendAsync("ReceiveMessage", message, own.UserId, user2.FirstName + " " + user2.LastName, filePath, ownPhoto);
                    }
                }
            }

            if (filePath == null)
            {
                user.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
                own.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
            }
            else
            {
                user.Messages.Add(new Messages() { Message = message, FilePath = filePath, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
                own.Messages.Add(new Messages() { Message = message, FilePath = filePath, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });

            }
            await _chatRepository.Update(user);
            await _chatRepository.Update(own);
            foreach (var item in own.ConnectionTokens.Where(m => m.Status == "Connected"))
            {
                if (filePath == null)
                {
                    await Clients.Clients(item.Token).SendAsync("OwnMessage", message, null, ownPhoto);
                }
                else
                {
                    await Clients.Clients(item.Token).SendAsync("OwnMessage", message, filePath, ownPhoto);
                }
            }
        }
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var ctx = Context.GetHttpContext();
            if (!ctx.Request.Query.ContainsKey("token"))
            {
                await Task.CompletedTask;
            }
            var userId = Convert.ToInt32(ctx.Request.Query["token"]);
            var user = await _userRepository.GetUserById(userId);
            var photo = user.PhotoUrl;

            var chatUser = await _chatRepository.GetChatUserByUserId(userId); 
            if (chatUser == null)
            {
                var model = new ChatUser() { UserId = userId, ConnectionTokens = new List<ConnectionToken>() { new ConnectionToken() { Token = connectionId, Status = "Connected" } } };
               
                model.Messages.Add(new Messages() { Id = 0, MessageTime = DateTime.Now, ReceiverId = userId.ToString(), SenderId = (await _userRepository.GetManagerId()).ToString(), Message = "Здравствуйте! Добро пожаловать! Если у Вас есть какие-то вопросы, смело обращайтесь!" });
                
                await _chatRepository.AddChatUser(model);
                await SetNewContact(userId);

            }
            else
            {
                chatUser.ConnectionTokens.Add( new ConnectionToken() { Token = connectionId, Status = "Connected" });
            }

            await GetContacts(userId);

            foreach (var item in chatUser.Messages)
            {
                var who =await  _chatRepository.GetChatUserByFunc(m => m.ConnectionTokens.Any(t => t.Token == item.SenderId));
                //var who = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.Any(t => t.Token == item.SenderId));
                if (who == null)
                {
                    who = await _chatRepository.GetChatUserByUserId(Convert.ToInt32(item.SenderId));
                }
            }

            await SetChat(userId, 0);

            await base.OnConnectedAsync();
        }

        public async Task SendMessageWithFile(FileAttachment attachment, string to, string message)
        {
            var fileBytes = Convert.FromBase64String(attachment.content);
            File.WriteAllBytes($"wwwroot/files/{attachment.fileName}", fileBytes);
            await SendMessage(message, to, $"{attachment.fileName}");
        }
        public async Task SetNewContact(int userId)
        {
            var user = await _userRepository.GetUserById(userId);
            //var user = TestData.UserList.FirstOrDefault(m => m.UserId == userId);

            List<Schedule> userSchediles = new List<Schedule>();

            if (user.Role == "Student")
            {
                userSchediles = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == userId); //TestData.Schedules.Where(m => m.UserId == userId).ToList();
            }
            else if (user.Role == "Tutor")
            {
                userSchediles = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == userId);
            }
            else
            {
                userSchediles = await _scheduleRepository.GetSchedulesByFunc(null);
            }

            var managerId  =await _userRepository.GetManagerId();
           // var manager = TestData.UserList.FirstOrDefault(m => m.Role == "Manager");
            if (managerId != userId)
            {
                var managerChat = await _chatRepository.GetChatUserByUserId(managerId);
                //var managerChat = TestData.ChatUsers.FirstOrDefault(m => m.UserId == manager.UserId);
                if (managerChat != null)
                {
                    if (managerChat.Contacts.FirstOrDefault(m => m.UserId == userId) == null)
                    {
                        managerChat?.Contacts?.Add(new Contact() { UserId = userId });
                        var active = managerChat?.ConnectionTokens?.Where(m => m.Status == "Connected");

                        if (active != null)
                        {
                            foreach (var item in active)
                            {
                                await Clients.Client(item.Token).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                            }
                        }
                        await _chatRepository.Update(managerChat);

                    }
                }


            }
            var chatUser = await _chatRepository.GetChatUserByUserId(user.UserId);
            //var chatUser = TestData.ChatUsers.FirstOrDefault(m => m.UserId == user.UserId);

            if (user.Role == "Manager")
            {
                foreach (var item in userSchediles)
                {
                    if (item.TutorId == -1 || item.UserId == -1)
                    {
                        continue;
                    }

                   // var student = await _userRepository.GetUserById(item.UserId);
                    //var student = TestData.UserList.FirstOrDefault(m => m.UserId == item.UserId);
                    //var tutor = await _userRepository.GetUserById(item.TutorId);

                    //var tutor = TestData.UserList.FirstOrDefault(m => m.UserId == item.TutorId);



                    var userContct = new Contact()
                    {
                        UserId = item.UserId
                    };

                    var tutorContact = new Contact()
                    {
                        UserId = item.TutorId
                    };


                    if (chatUser.Contacts.FirstOrDefault(m => m.UserId == userContct.UserId) == null)
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

                if (item.UserId == -1)
                {
                    continue;
                }

                var contact = new Contact()
                {
                    UserId = item.TutorId
                };

                var chat = await _chatRepository.GetChatUserByUserId(item.UserId); //TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.UserId);


                if (user.UserId != item.UserId)
                {
                    if (chat == null)
                    {
                        if (chatUser.Contacts.FirstOrDefault(m => m.UserId == item.UserId) == null)
                        {
                            chatUser.Contacts.Add(new Contact() { UserId = item.UserId });
                        }
                    }
                    else
                    {

                        if (chat.Contacts.FirstOrDefault(m => m.UserId == contact.UserId) == null)
                        {
                            chatUser = chat;
                            chatUser.Contacts.Add(contact);

                            foreach (var token in chatUser.ConnectionTokens.Where(m => m.Status == "Connected"))
                            {
                                await Clients.Client(token.Token).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                            }

                            if (chatUser.Contacts.FirstOrDefault(m => m.UserId == item.UserId) == null)
                            {
                                chatUser.Contacts.Add(new Contact() { UserId = item.UserId });
                            }
                        }
                        else
                        {
                            if (chatUser.Contacts.FirstOrDefault(m => m.UserId == item.UserId) == null)
                            {
                                chatUser.Contacts.Add(new Contact() { UserId = item.UserId });
                            }
                        }
                    }

                }

                contact = new Contact()
                {
                    UserId = item.UserId
                };

                if (user.UserId != item.TutorId)
                {
                    var tutor = await _chatRepository.GetChatUserByUserId(item.TutorId); //TestData.ChatUsers.FirstOrDefault(m => m.UserId == item.TutorId);
                    if (tutor == null)
                    {
                        if (chatUser.Contacts.FirstOrDefault(m => m.UserId == item.TutorId) == null)
                        {
                            chatUser.Contacts.Add(new Contact() { UserId = item.TutorId });
                        }
                    }
                    else
                    {

                        if (tutor.Contacts.FirstOrDefault(m => m.UserId == contact.UserId) == null)
                        {
                            chatUser = await _chatRepository.GetChatUserByUserId(item.TutorId);

                            chatUser.Contacts.Add(contact);

                            foreach (var token in chatUser.ConnectionTokens.Where(m => m.Status == "Connected"))
                            {
                                await Clients.Client(token.Token).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                            }

                            tutor.Contacts.Add(contact);
                            await _chatRepository.Update(tutor);
                            chatUser.Contacts.Add(new Contact() { UserId = item.TutorId });

                        }
                        else
                        {
                            if (chatUser.Contacts.FirstOrDefault(m => m.UserId == item.TutorId) == null)
                            {
                                chatUser.Contacts.Add(new Contact() { UserId = item.TutorId });
                            }
                        }
                    }

                }

                await _chatRepository.Update(chat);

            }

            var contact2 = new Contact()
            {
                UserId = managerId
            };

            if (!chatUser.Contacts.Contains(contact2) && managerId!= user.UserId)
            {
                chatUser.Contacts.Add(contact2);
            }

            await _chatRepository.Update(chatUser);
        }

        //public static async Task RemoveContact(int userId)
        //{

        //    //var users = TestData.ChatUsers.ToList();
        //    foreach (var item in users)
        //    {
        //        var cont = item.Contacts.FirstOrDefault(m => m.UserId == userId);
        //        if (cont != null)
        //        {
        //            item.Contacts.Remove(cont);
        //        }
        //    }
        //}

        public async Task GetContacts(int userId)
        {
            var chatUser = await _chatRepository.GetChatUserByUserId(userId);
            //var chatUser = TestData.ChatUsers.FirstOrDefault(m => m.UserId == userId);
            foreach (var item in chatUser.Contacts)
            {
                if (item.UserId == -1)
                {
                    continue;
                }
                var user = await _userRepository.GetUserById(item.UserId);
                //var user = TestData.UserList.FirstOrDefault(m => m.UserId == item.UserId);

                await Clients.Clients(Context.ConnectionId).SendAsync("UserList", item.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
            }

        }
        public async override Task OnDisconnectedAsync(Exception ex)
        {
            var connectionId = Context.ConnectionId;
            var userId = Convert.ToInt32(Context.GetHttpContext().Request.Query["token"]);

            var user = await _chatRepository.GetChatUserByUserId(userId);
            user.ConnectionTokens.FirstOrDefault(m=>m.Token == connectionId).Status = "Disconnected";
            await _chatRepository.Update(user);
            await Clients.All.SendAsync("DisconnectUser", connectionId);

            await Task.CompletedTask;
        }
        public async Task OnlineUsers()
        {
            var connectionId = Context.ConnectionId;
            var who =await  _chatRepository.GetChatUserByFunc(m => m.ConnectionTokens.Any(m => m.Token == connectionId));
            //var who = TestData.ChatUsers.FirstOrDefault(m => m.ConnectionTokens.Any( m=>m.Token == connectionId));
            var user = await _userRepository.GetUserById(who.UserId);
            //var user = TestData.UserList.FirstOrDefault(m => m.UserId == who.UserId);
            await Clients.All.SendAsync("UserList", connectionId, user.FirstName + " " + user.LastName, user.PhotoUrl);
        }
    }
}
