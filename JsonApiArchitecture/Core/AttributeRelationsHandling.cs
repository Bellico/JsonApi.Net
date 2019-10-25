using JsonApiArchitecture.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JsonApiArchitecture.Core
{
    public static class AttributeRelationsHandling
    {
        /// <summary>
        /// Retourne le type de datasource d'un type de ressource
        /// </summary>
        public static Type GetDataSourceType(Type type) => type.GetCustomAttribute<RessourceJsonApiAttribute>()?.DataSource;

        /// <summary>
        /// Retourne le type de repository d'un type de ressource
        /// </summary>
        public static Type GetRepositoryType(Type type) => type.GetCustomAttribute<RessourceJsonApiAttribute>()?.Repository;

        /// <summary>
        /// Retourne le type de critere d'un type de ressource
        /// </summary>
        public static Type GetCriteresType(Type type) => type.GetCustomAttribute<RessourceJsonApiAttribute>()?.Criteres;

        /// <summary>
        /// Récupere la liste des couples propriétés / attributes (HasOneJsonApiAttribute et/ou HasManyJsonApiAttribute) pour un type
        /// </summary>
        /// <param name="type">Type sur lequel chercher</param>
        /// <param name="typeRelation">Type de relation recherché, par défaut TypeRelation.None = Tous les relations</param>
        public static IEnumerable<Tuple<PropertyInfo, RelationshipJsonApiAttribute>> GetRelationshipProperties(Type type, TypeRelation typeRelation = TypeRelation.None)
        {
            Type typeAttribute;

            if (typeRelation == TypeRelation.One)
                typeAttribute = typeof(HasOneJsonApiAttribute);
            else if (typeRelation == TypeRelation.Many)
                typeAttribute = typeof(HasManyJsonApiAttribute);
            else if (typeRelation == TypeRelation.ManyToMany)
                typeAttribute = typeof(HasManyToManyJsonApiAttribute);
            else
                typeAttribute = typeof(RelationshipJsonApiAttribute);

            foreach (PropertyInfo prop in type.GetProperties()) {
                var attr = (RelationshipJsonApiAttribute)prop.GetCustomAttribute(typeAttribute);

                if (attr != null)
                    yield return new Tuple<PropertyInfo, RelationshipJsonApiAttribute>(prop, attr);
            }
        }

        /// <summary>
        /// Recherche une propriété et son attribute ForeignKeyJsonApi à partir du nom de relation
        /// </summary>
        public static Tuple<PropertyInfo, ForeignKeyJsonApiAttribute> FindForeignKeyPropertyAndAttribute(Type type, string nomRelation)
        {
            foreach (PropertyInfo prop in type.GetProperties()) {
                ForeignKeyJsonApiAttribute attr = prop.GetCustomAttribute<ForeignKeyJsonApiAttribute>();

                if (attr != null && attr.RelationName == nomRelation)
                    return new Tuple<PropertyInfo, ForeignKeyJsonApiAttribute>(prop, attr);
            }

            return new Tuple<PropertyInfo, ForeignKeyJsonApiAttribute>(null, null);
        }

        /// <summary>
        /// Recherche la propriété ForeignKeyJsonApi d'une relation ToOne à partir d'un type dans un type donnée
        /// Exemple : trouver la propriété IdBenef dans le type BenefSiteRessource à partir du Type BenefRessource
        /// </summary>
        public static PropertyInfo FindForeignKeyPropertyFromType(Type type, Type searchtype)
        {
            foreach (Tuple<PropertyInfo, RelationshipJsonApiAttribute> relation in GetRelationshipProperties(type, TypeRelation.One)) {
                if (relation.Item1.PropertyType == searchtype) {
                    return FindForeignKeyPropertyAndAttribute(type, relation.Item1.Name).Item1;
                }
            }

            return null;
        }
    }
}
