namespace web_app.Requests.Get
{
    public class GetAllUsersRequest : CustomRequestGet
    {
        public GetAllUsersRequest(string args) : base("api/home/getAllStudentsAndTutors", args) { }
    }
}
