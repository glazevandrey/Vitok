namespace web_app.Requests.Get
{
    public class GetAllTutorsRequest : CustomRequestGet
    {
        public GetAllTutorsRequest() : base("api/tutor/getall", null) { }
    }
}
