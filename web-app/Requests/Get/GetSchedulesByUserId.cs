namespace web_app.Requests.Get
{
    public class GetSchedulesByUserId : CustomRequestGet
    {
        public GetSchedulesByUserId(string id) : base("api/home/getschedulebyid", id) { }
    }
}
