namespace web_app.Models.Requests.Get
{
    public class GetCourses : CustomRequestGet
    {
        public GetCourses() : base("api/ServerCourses/getcourses", null) { }
    }
}
