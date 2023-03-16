namespace web_server.Services.Interfaces
{
    public interface ISenderService
    {
        public void SendMessage(string address, string message);
    }
}
