using web_server.Models;
using web_server.Models.DBModels;

namespace web_app.Requests
{
    public class CustomRequestPost
    {
        public User User { get; set; }
        public string Token { get; set; }
        public string Address { get; set; }
        public string Args { get; set; }
        public TransferModel TransferData { get; set; }

        public CustomRequestPost(string address, User user)
        {
            Address = address;
            User = user;
        }
        //public CustomRequestPost(string address, Student user)
        //{
        //    Address = address;
        //    User = user;
        //}
        //public CustomRequestPost(string address, Tutor user)
        //{
        //    Address = address;
        //    User = user;
        //}
        public CustomRequestPost(string address, TransferModel data)
        {
            Address = address;
            TransferData = data;
        }
        public CustomRequestPost(string address, string args)
        {
            Address = address;
            Args = args;
        }
    }
}
