namespace JsonApiArchitecture.Attributes
{
    /// <summary>
    /// A Utiliser lorsque la table de relation NE POSSEDE PAS une clé primaire
    /// </summary>
    public class HasManyToManyJsonApiAttribute : HasManyJsonApiAttribute
    {
        public string Key { get; set; }
    }
}
