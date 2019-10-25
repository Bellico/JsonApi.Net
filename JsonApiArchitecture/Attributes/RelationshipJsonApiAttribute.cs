using JsonApi.Attributes;

namespace JsonApiArchitecture.Attributes
{
    public class RelationshipJsonApiAttribute : PropertyJsonApiAttribute
    {

    }

    public enum TypeRelation
    {
        ManyToMany = 3,
        Many = 1,
        One = 2,
        None = 0
    };
}
