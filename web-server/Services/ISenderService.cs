namespace web_server.Services
{
    public interface ISenderService
    {
        public void SendMessage(string address, string message);
    }
}
