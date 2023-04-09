namespace web_app.Requests.Get
{
    public class GetUserByToken : CustomRequestGet
    {
        public GetUserByToken(string token) : base("api/home/getuser", token) { }
    }
}
