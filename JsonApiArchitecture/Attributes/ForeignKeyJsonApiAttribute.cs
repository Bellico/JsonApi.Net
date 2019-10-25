using JsonApi.Attributes;
using System;

namespace JsonApiArchitecture.Attributes
{
    public class ForeignKeyJsonApiAttribute : IgnoreJsonApiAttribute
    {
        /// <summary>
        /// Nom de la propriété de relation
        /// </summary>
        public string RelationName { get; set; }

        /// <summary>
        /// Type de la classe de critère à utiliser pour le datasource
        /// Ecrase le critere par défaut définit dans la ressource
        /// </summary>
        public Type Criteres { get; set; }

        /// <summary>
        /// Propriété de type Liste<> qui sera afféctée par une liste d'identifiant, si non présicée, la propriété de base "ListeId" sera affectée
        /// Cas d'une relation ToOne et ToMany, ne rien mettre, "ListeId" convient
        /// Cas d'une relation ManyToMany, creer un criteres pour recevoir une liste d'identifiant de la ressource courante
        /// </summary>
        public string Property { get; set; }
    }
}
