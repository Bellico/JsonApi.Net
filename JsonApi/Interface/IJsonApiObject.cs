namespace JsonApi.Interface
{
    interface IJsonApiObject
    {
        string id { get; set; }
        string type { get; set; }
        object toJsonFormat();
    }
}
