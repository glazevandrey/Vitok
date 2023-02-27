namespace web_app.Models.Requests.Get
{
    public class GetUserById : CustomRequestGet
    {
        public GetUserById(string id) : base("api/home/getuserbyid", id) { }
    }
}
