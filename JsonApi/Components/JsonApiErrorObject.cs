namespace JsonApi.Components
{
    public class JsonApiErrorObject
    {
        public int status { get; set; }
        public string source { get; set; }
        public string title { get; set; }
        public string detail { get; set; }
    }
}
