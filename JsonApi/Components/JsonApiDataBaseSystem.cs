using JsonApi.Interface;

namespace JsonApi.Components
{
    public class JsonApiDataBaseSystem : IJsonApiObject
    {
        public string id { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public object toJsonFormat()
        {
            return this.value;
        }
    }
}
