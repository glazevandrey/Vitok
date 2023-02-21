namespace web_application.Models.Requests.Get
{
    public class GetUserByToken : CustomRequestGet
    {
        public GetUserByToken(string token) : base("api/home/getuser", token) { }
    }
}
