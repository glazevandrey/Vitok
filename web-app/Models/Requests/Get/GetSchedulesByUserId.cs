namespace web_app.Models.Requests.Get
{
    public class GetSchedulesByUserId : CustomRequestGet
    {
        public GetSchedulesByUserId(string id) : base("api/home/getschedulebyid", id) { }
    }
}
