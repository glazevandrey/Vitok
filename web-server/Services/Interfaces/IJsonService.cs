namespace web_server.Services.Interfaces
{
    public interface IJsonService
    {
        public string PrepareSuccessJson(string message);
        public string PrepareErrorJson(string message);
    }
}
