namespace web_app.Requests.Get
{
    public class GetTutorByIdRequest : CustomRequestGet
    {
        public GetTutorByIdRequest(string id) : base("api/tutor/gettutor", id) { }
    }
}
