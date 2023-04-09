namespace web_app.Requests.Get
{
    public class GetSchedulesByUserToken : CustomRequestGet
    {
        public GetSchedulesByUserToken(string token) : base("api/account/getschedule", token) { }
    }
}
