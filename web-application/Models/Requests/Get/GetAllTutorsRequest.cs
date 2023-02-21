namespace web_application.Models.Requests.Get
{
    public class GetAllTutorsRequest : CustomRequestGet
    {
        public GetAllTutorsRequest() : base("api/tutor/getall", null) { }
    }
}
