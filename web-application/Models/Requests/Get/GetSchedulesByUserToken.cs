namespace web_application.Models.Requests.Get
{
    public class GetSchedulesByUserToken : CustomRequestGet
    {
        public GetSchedulesByUserToken(string token) : base("api/account/getschedule", token) { }
    }
}
