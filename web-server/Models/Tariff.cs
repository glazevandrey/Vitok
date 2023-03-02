namespace web_server.Models
{
    public class Tariff
    {
        public int Id { get; set; }
        public string Title{ get; set; }
        public int LessonsCount { get; set; }
        public double Amount { get; set; }
    }
}
