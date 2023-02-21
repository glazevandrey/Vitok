namespace web_app.Models.Requests.Get
{
    public class GetSchedulesByUserToken : CustomRequestGet
    {
        public GetSchedulesByUserToken(string token) : base("api/account/getschedule", token) { }
    }
}
