namespace web_app.Models.Requests.Get
{
    public class GetAllUsersRequest : CustomRequestGet
    {
        public GetAllUsersRequest() : base("api/home/getAllStudentsAndTutors", null) { }
    }
}
