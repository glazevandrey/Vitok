namespace web_app.Requests.Get
{
    public class GetLiteUserByToken : CustomRequestGet
    {
        public GetLiteUserByToken(string token) : base("api/home/getliteuser", token) { }
    }
}
