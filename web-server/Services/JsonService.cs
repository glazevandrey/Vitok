using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class JsonService : IJsonService
    {
        public string PrepareSuccessJson(string message)
        {
            string result = @"{""success"":true,""result"":" + message + "}";
            return result;
        }

        public string PrepareErrorJson(string message)
        {
            string result = @"{""success"":false,""result"":""" + message + @"""}";
            return result;
        }
    }
}
