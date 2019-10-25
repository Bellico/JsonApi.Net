namespace JsonApi.Interface
{
    public interface IQueryRead
    {
        void ReadQuery(IQueryService queryService);
    }

    public interface IQueryParseList
    {
        void Parse(string code, object values);
    }
}
