namespace web_server.Services.Interfaces
{
    public interface ICourseService
    {
        public string SetNewCourse(string[] args);
        public string RemoveCourse(string args);
        public string EditCourse(string[] args);

    }
}
