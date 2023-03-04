namespace web_app.Models.Requests.Get
{
    public class GetAllSchedules : CustomRequestGet
    {
        public GetAllSchedules() : base("api/home/getAllSchedules", null) { }
    }
}
