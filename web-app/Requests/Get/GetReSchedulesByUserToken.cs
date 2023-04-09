namespace web_app.Requests.Get
{
    public class GetReSchedulesByUserToken : CustomRequestGet
    {
        public GetReSchedulesByUserToken(string token) : base("api/account/getreschedule", token) { }
    }
}
