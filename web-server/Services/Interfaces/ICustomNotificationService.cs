namespace web_server.Services.Interfaces
{
    public interface ICustomNotificationService
    {
        public void SendMessage(string message, string to);
    }
}
