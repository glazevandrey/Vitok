using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using web_server.Models.DBModels;

namespace web_app
{
    public class UserConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(User);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            User user;
            switch (item["Role"].ToString())
            {
                case "Tutor":
                    user = new Tutor();
                    break;
                case "Student":
                    user = new Student();
                    break;
                case "Manager":
                    user = new Manager();
                    break;
                default:
                    throw new Exception("Unknown role");
            }
            serializer.Populate(item.CreateReader(), user);
            return user;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
