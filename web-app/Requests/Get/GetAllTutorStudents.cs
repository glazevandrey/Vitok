namespace web_app.Requests.Get
{
    public class GetAllTutorStudentsRequest : CustomRequestGet
    {
        public GetAllTutorStudentsRequest(string args) : base("api/account/getalltutorstudents", args) { }
    }
}
