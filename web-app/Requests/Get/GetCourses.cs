namespace web_app.Requests.Get
{
    public class GetCourses : CustomRequestGet
    {
        public GetCourses() : base("api/ServerCourses/getcourses", null) { }
    }
}
