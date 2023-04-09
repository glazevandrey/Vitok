namespace web_app.Requests.Get
{
    public class GetAllSchedules : CustomRequestGet
    {
        public GetAllSchedules() : base("api/home/getAllSchedules", null) { }
    }
}
