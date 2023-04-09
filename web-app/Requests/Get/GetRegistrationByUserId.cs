namespace web_app.Requests.Get
{
    public class GetRegistrationByUserId : CustomRequestGet
    {
        public GetRegistrationByUserId(string id) : base("api/home/getregistration", id) { }
    }
}
