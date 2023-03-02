namespace web_app.Models.Requests.Get
{
    public class GetCourses: CustomRequestGet
    {
        public GetCourses(string token) : base("api/servercourses/getcourses", token) { }
    }
}
