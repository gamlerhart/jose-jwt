using System.Web.Script.Serialization;

namespace Jose
{
    public class JsSerializerMapper : IJsonMapper
    {
        private static JavaScriptSerializer js = new JavaScriptSerializer();


        public string Serialize(object obj)
        {
            return js.Serialize(obj);
        }

        public T Parse<T>(string json)
        {
            return js.Deserialize<T>(json);
        }
    }
}