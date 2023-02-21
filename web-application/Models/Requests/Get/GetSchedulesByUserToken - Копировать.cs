namespace web_application.Models.Requests.Get
{
    public class GetRegistrationByUserId : CustomRequestGet
    {
        public GetRegistrationByUserId(string id) : base("api/home/getregistration", id) { }
    }
}
