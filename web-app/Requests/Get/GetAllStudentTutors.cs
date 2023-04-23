namespace web_app.Requests.Get
{
    public class GetAllStudentTutorsRequest : CustomRequestGet
    {
        public GetAllStudentTutorsRequest(string args) : base("api/account/getallstudenttutors", args) { }
    }
}
