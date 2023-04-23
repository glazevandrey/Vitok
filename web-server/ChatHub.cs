using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server
{
    [web_server.Models.Authorize]
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        UserRepository _userRepository;
        //  ChatRepository _chatRepository;
        ScheduleRepository _scheduleRepository;
        public ChatHub(UserRepository userRepository, ScheduleRepository scheduleRepository)
        {
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
        }


        public async Task GetMessages(string userId)
        {
            // var currChat 
            // var currChat =await  _chatRepository.GetChatByFunc(u => u.ConnectionTokens.Any(t => t.Token == Context.ConnectionId));
            // var currChat = TestData.Chats.FirstOrDefault(u => u.ConnectionTokens.Any(t => t.Token == Context.ConnectionId));
            // var currUser =await _userRepository.GetUserById(currChat.UserId);
            // var currUser = TestData.UserList.FirstOrDefault(m => m.UserId == currChat.UserId);

            var currUser = await _userRepository.GetUserByChatToken(Context.ConnectionId);



            currUser.Chat.InChat = Convert.ToInt32(userId);


            try
            {
                await _userRepository.SaveChanges(currUser);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            var user = await _userRepository.GetUserById(Convert.ToInt32(userId));
            //var user = await _chatRepository.GetChatByUserId(Convert.ToInt32(userId));
            //var user = TestData.Chats.FirstOrDefault(m => m.UserId == Convert.ToInt32(userId));

            if (user.Chat == null) { await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow"); return; }

            var messages = currUser.Chat.Messages.ToList();
            var result = new List<Messages>();

            await Clients.Clients(Context.ConnectionId).SendAsync("ClearNow");
            //messages.Reverse();
            foreach (var message in messages)
            {
                if (user.Chat.ConnectionTokens.Any(t => t.Token == message.SenderId) || user.Chat.ConnectionTokens.Any(t => t.Token == message.ReceiverId))
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
                //var user2 = await _userRepository.GetUserById(user.UserId);
                if (user.Chat.ConnectionTokens.Any(t => t.Token == item.SenderId) || user.UserId == Convert.ToInt32(item.SenderId))
                {
                    if (item.FilePath == null)
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", item.Message, userId, user.FirstName + " " + user.LastName, null, user.PhotoUrl);
                    }
                    else
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", item.Message, userId, user.FirstName + " " + user.LastName, item.FilePath, user.PhotoUrl);
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
            var user = await _userRepository.GetUser(Convert.ToInt32(token));
            //  var user = await _chatRepository.GetChatByUserId(Convert.ToInt32(token));
            //var user = TestData.Chats.FirstOrDefault(m => m.UserId == Convert.ToInt32(token));

            var own = await _userRepository.GetUserByChatToken(Context.ConnectionId);
            //var own =await  _chatRepository.GetChatByFunc(m => m.ConnectionTokens.Any(m => m.Token == Context.ConnectionId));
            //var own = TestData.Chats.FirstOrDefault(m => m.ConnectionTokens.Any(m=>m.Token == Context.ConnectionId));
            var ownPhoto = own.PhotoUrl;
            //var ownPhoto = TestData.UserList.FirstOrDefault(m => m.UserId == own.UserId).PhotoUrl;
            if (user.Chat == null) { return; }

            var active = user.Chat.ConnectionTokens.Where(m => m.Status == "Connected").ToList();
            foreach (var item in active)
            {
                if (user.Chat.InChat == own.UserId)
                {
                    if (filePath == null)
                    {
                        await Clients.Clients(item.Token).SendAsync("ReceiveMessage", message, own.UserId, user.FirstName + " " + user.LastName, null, ownPhoto);
                    }
                    else
                    {
                        await Clients.Clients(item.Token).SendAsync("ReceiveMessage", message, own.UserId, user.FirstName + " " + user.LastName, filePath, ownPhoto);
                    }
                }
            }

            if (filePath == null)
            {
                user.Chat.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
                own.Chat.Messages.Add(new Messages() { Message = message, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
            }
            else
            {
                user.Chat.Messages.Add(new Messages() { Message = message, FilePath = filePath, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });
                own.Chat.Messages.Add(new Messages() { Message = message, FilePath = filePath, SenderId = own.UserId.ToString(), ReceiverId = user.UserId.ToString(), MessageTime = DateTime.Now });

            }

            await _userRepository.SaveChanges(user);

            try
            {
                await _userRepository.SaveChanges(own);


            }
            catch (Exception ex)
            {

                throw ex;
            }

            foreach (var item in own.Chat.ConnectionTokens.Where(m => m.Status == "Connected"))
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

            var user = await _userRepository.GetUser(userId);

            if (user.Chat == null)
            {
                var model = new ChatDTO() { InChat = 0, UserId = userId, ConnectionTokens = new List<ConnectionToken>() { new ConnectionToken() { Token = connectionId, Status = "Connected" } } };

                if (user.Role != "Manager")
                {
                    model.Messages.Add(new Messages() { Id = 0, MessageTime = DateTime.Now, ReceiverId = userId.ToString(), SenderId = (await _userRepository.GetManagerId()).ToString(), Message = "Здравствуйте! Добро пожаловать! Если у Вас есть какие-то вопросы, смело обращайтесь!" });

                }
                user.Chat = model;
                user.Chat.InChat = 0;

                await SetNewContact(user);

                try
                {
                    await _userRepository.SaveChanges(user);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (user.Chat?.Contacts.Count == 0)
            {

                await SetNewContact(user);
                user.Chat.ConnectionTokens.Add(new ConnectionToken() { Token = connectionId, Status = "Connected" });
                user.Chat.Messages.Add(new Messages() { Id = 0, MessageTime = DateTime.Now, ReceiverId = userId.ToString(), SenderId = (await _userRepository.GetManagerId()).ToString(), Message = "Здравствуйте! Добро пожаловать! Если у Вас есть какие-то вопросы, смело обращайтесь!" });

                try
                {
                    await _userRepository.SaveChanges(user);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                user.Chat.InChat = 0;

                user.Chat.ConnectionTokens.Add(new ConnectionToken() { Token = connectionId, Status = "Connected" });
                try
                {
                    await _userRepository.SaveChanges(user);

                }
                catch (Exception ex)
                {

                    throw ex;
                }


            }


            await GetContacts(user);



            await base.OnConnectedAsync();
        }

        public async Task SendMessageWithFile(FileAttachment attachment, string to, string message)
        {
            var fileBytes = Convert.FromBase64String(attachment.content);
            File.WriteAllBytes($"wwwroot/files/{attachment.fileName}", fileBytes);
            await SendMessage(message, to, $"{attachment.fileName}");
        }
        public async Task SetNewContact(UserDTO user)
        {
            List<ScheduleDTO> userSchediles;

            if (user.Role == "Manager")
            {
                var schedules = await _scheduleRepository.GetSchedulesByFunc(null);

                foreach (var item in schedules)
                {
                    if (item.TutorId == 1 || item.UserId == 1)
                    {
                        continue;
                    }

                    var userContct = new Contact()
                    {
                        UserId = item.UserId
                    };

                    var tutorContact = new Contact()
                    {
                        UserId = item.TutorId
                    };


                    if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == userContct.UserId) == null)
                    {
                        user.Chat.Contacts.Add(userContct);
                    }


                    if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == tutorContact.UserId) == null)
                    {
                        user.Chat.Contacts.Add(tutorContact);
                    }
                }
                //await _userRepository.Update(user);

                await _scheduleRepository.UpdateRange(schedules);
                return;
            }
            else
            {
                if (user.Role == "Tutor")
                {
                    userSchediles = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == user.UserId);
                }
                else
                {
                    userSchediles = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user.UserId);

                }

                await _scheduleRepository.UpdateRange(userSchediles);
            }

            var managerId = await _userRepository.GetManagerId();
            var manager = (Manager)await _userRepository.GetUserById(managerId);



            if (user.Role != "Manager")
            {
                //var managerChat = TestData.Chats.FirstOrDefault(m => m.UserId == manager.UserId);
                if (manager?.Chat != null)
                {
                    manager.Chat?.Messages.Add(new Messages() { Message = "Здравствуйте! Добро пожаловать! Если у Вас есть какие-то вопросы, смело обращайтесь!", MessageTime = DateTime.Now, ReceiverId = user.UserId.ToString(), SenderId = manager.UserId.ToString() });

                    if (manager.Chat.Contacts.FirstOrDefault(m => m.UserId == user.UserId) == null)
                    {
                        manager.Chat?.Contacts?.Add(new Contact() { UserId = user.UserId });
                       
                        var active = manager.Chat?.ConnectionTokens?.Where(m => m.Status == "Connected");

                        if (active != null)
                        {
                            foreach (var item in active)
                            {
                                await Clients.Client(item.Token).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                            }
                        }
                        try
                        {

                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }

                    }

                    await _userRepository.Update(manager);

                }


            }


            foreach (var item in userSchediles)
            {

                if (item.UserId == 1)
                {
                    continue;
                }

                var contact = new Contact()
                {
                    UserId = item.TutorId
                };


               if (user.UserId != item.UserId)
                {
                    var itemUser = await _userRepository.GetStudent(item.UserId);

                    if (itemUser.Chat == null)
                    {
                        if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == item.UserId) == null)
                        {
                            user.Chat.Contacts.Add(new Contact() { UserId = item.UserId });
                        }
                    }
                    else
                    {

                        if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == contact.UserId) == null)
                        {
                            //user.Chat = itemUser.Chat;
                            itemUser.Chat.Contacts.Add(contact);

                            foreach (var token in user.Chat.ConnectionTokens.Where(m => m.Status == "Connected"))
                            {
                                await Clients.Client(token.Token).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                            }

                            if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == item.UserId) == null)
                            {
                                user.Chat.Contacts.Add(new Contact() { UserId = item.UserId });
                            }

                        }
                        else
                        {
                            if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == item.UserId) == null)
                            {
                                user.Chat.Contacts.Add(new Contact() { UserId = item.UserId });
                            }
                        }
                    }

                    await _userRepository.SaveChanges(itemUser);

                }

                contact = new Contact()
                {
                    UserId = item.UserId
                };

                if (user.UserId != item.TutorId)
                {
                    var tutor = await _userRepository.GetTutor(item.TutorId);
                    //var tutor = await _chatRepository.GetChatByUserId(item.TutorId); //TestData.Chats.FirstOrDefault(m => m.UserId == item.TutorId);
                    if (tutor.Chat == null)
                    {
                        if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == item.TutorId) == null)
                        {
                            user.Chat.Contacts.Add(new Contact() { UserId = item.TutorId });
                        }
                    }
                    else
                    {

                        if (tutor.Chat.Contacts.FirstOrDefault(m => m.UserId == contact.UserId) == null)
                        {
                            //Chat = await _chatRepository.GetChatByUserId(item.TutorId);

                            tutor.Chat.Contacts.Add(contact);

                            foreach (var token in tutor.Chat.ConnectionTokens.Where(m => m.Status == "Connected"))
                            {
                                await Clients.Client(token.Token).SendAsync("UserList", user.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
                            }
                            
                            if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == item.TutorId) == null)
                            {
                                user.Chat.Contacts.Add(new Contact() { UserId = item.TutorId });
                            }

                        }
                        else
                        {
                            if (user.Chat.Contacts.FirstOrDefault(m => m.UserId == item.TutorId) == null)
                            {
                                user.Chat.Contacts.Add(new Contact() { UserId = item.TutorId });
                            }
                        }
                    }
                    await _userRepository.SaveChanges(tutor);

                }

            }

            var contact2 = new Contact()
            {
                UserId = managerId
            };

            if (!user.Chat.Contacts.Contains(contact2) && managerId != user.UserId)
            {
                user.Chat.Contacts.Add(contact2);
            }

        }

        public async Task GetContacts(UserDTO curUser)
        {
            
            foreach (var item in curUser.Chat.Contacts)
            {
                if (item.UserId == 1)
                {
                    continue;
                }
                if(item.UserId == curUser.UserId)
                {
                    continue;
                }
                var user = await _userRepository.GetUserById(item.UserId);

                await Clients.Clients(Context.ConnectionId).SendAsync("UserList", item.UserId, user.FirstName + " " + user.LastName, user.PhotoUrl);
            }

        }
        public async override Task OnDisconnectedAsync(Exception ex)
        {
            //var connectionId = Context.ConnectionId;
            //  var userId = Convert.ToInt32(Context.GetHttpContext().Request.Query["token"]);

            var user = await _userRepository.GetUserByChatToken(Context.ConnectionId);
            // var user = await _chatRepository.GetChatByUserId(userId);
            if (user.Chat == null)
            {
                return;
            }
            user.Chat.ConnectionTokens.FirstOrDefault(m => m.Token == Context.ConnectionId).Status = "Disconnected";
            await _userRepository.SaveChanges(user);
            await Clients.All.SendAsync("DisconnectUser", Context.ConnectionId);

            await Task.CompletedTask;
        }
        public async Task OnlineUsers()
        {

            //var who =await  _chatRepository.GetChatByFunc(m => m.ConnectionTokens.Any(m => m.Token == connectionId));
            //var who = TestData.Chats.FirstOrDefault(m => m.ConnectionTokens.Any( m=>m.Token == connectionId));
            var user = await _userRepository.GetUserByChatToken(Context.ConnectionId);
            //var user = TestData.UserList.FirstOrDefault(m => m.UserId == who.UserId);
            await Clients.All.SendAsync("UserList", Context.ConnectionId, user.FirstName + " " + user.LastName, user.PhotoUrl);
            await _userRepository.SaveChanges(user);

        }
    }
}
