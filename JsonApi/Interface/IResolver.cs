namespace JsonApi.Interface
{
    public interface IResolver<T>
    {
        T ResolveParser(T value);
        T ResolveReader(T value);
    }
}
