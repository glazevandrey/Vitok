namespace web_app.Models.Requests.Get
{
    public class GetTutorByIdRequest : CustomRequestGet
    {
        public GetTutorByIdRequest(string id) : base("api/tutor/gettutor", id) { }
    }
}
