namespace web_app.Requests.Get
{
    public class GetLiteUserById : CustomRequestGet
    {
        public GetLiteUserById(string id) : base("api/home/getliteuserbyid", id) { }
    }
}
