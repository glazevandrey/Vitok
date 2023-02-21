namespace web_server.Services
{
    public interface IJsonService
    {
        public string PrepareSuccessJson(string message);
        public string PrepareErrorJson(string message);
    }
}
