namespace web_app.Requests.Get
{
    public class GetGoalsRequest : CustomRequestGet
    {
        public GetGoalsRequest() : base("api/home/getGoals", null) { }

    }
}
