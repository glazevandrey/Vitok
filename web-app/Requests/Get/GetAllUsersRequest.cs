namespace web_app.Requests.Get
{
    public class GetAllUsersRequest : CustomRequestGet
    {
        public GetAllUsersRequest() : base("api/home/getAllStudentsAndTutors", null) { }
    }
}
