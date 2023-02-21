using Microsoft.VisualBasic;
using server.DbContext;
using System.Collections.Generic;

namespace server.Models
{
    public class ChatContext
    {
        public static List<User> Users { get; set; } = TestData.UserList;
        public static List<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
