namespace web_app.Models.Requests.Get
{
    public class GetStatisticsData : CustomRequestGet
    {
        public GetStatisticsData(string args) : base("api/home/getstatistics",  args) { }
    }
}
