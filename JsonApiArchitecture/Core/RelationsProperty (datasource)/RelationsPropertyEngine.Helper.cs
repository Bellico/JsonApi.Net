using JsonApiArchitectureBase.Interfaces;
using JsonApi.Core;
using JsonApi.Interface;
using JsonApiArchitecture.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiArchitecture.Extensions;

namespace JsonApiArchitecture.Core
{
    internal partial class RelationsPropertyEngine<T>
    {
        /// <summary>
        /// Récupere les champs que l'on souhaite récupérer dans la ressource, et notamment les champs clés pour les relations demandées
        /// </summary>
        /// <returns></returns>
        private List<string> GetFields()
        {
            // Définition des champs à récuperer
            List<string> fields = this._queryService.GetListeFields(typeof(T), this._relationPath);

            // Cas particulier de la relation HasManyToMany
            if (this._relationAttribute is HasManyToManyJsonApiAttribute) {
                if (fields == null)
                    fields = new List<string>();

                fields.Add((this._relationAttribute as HasManyToManyJsonApiAttribute).Key);
                this._identifiantRelationProperty = typeof(T).GetProperty((this._relationAttribute as HasManyToManyJsonApiAttribute).Key);
            }

            return fields;
        }

        /// <summary>
        /// Récuperer l'instance de la classe de critéres à utiliser pour le datasource
        /// </summary>
        /// <returns></returns>
        private object GetCriteres()
        {
            Type typeCriteres = this._foreignKeyAttribute.Criteres ?? AttributeRelationsHandling.GetCriteresType(typeof(T));
            MethodInfo filterMethod = this._queryService.GetType().GetMethod(nameof(IQueryService.FilterOnInclude)).MakeGenericMethod(typeCriteres);
            object criteres = filterMethod.Invoke(this._queryService, new object[] { AttributeHandling.GetLabelProperty(this._relationProperty) });

            if (this._foreignKeyAttribute.Property == null) {
                if (this._typeRelation == TypeRelation.Many)
                    (criteres as ICriteresBase).ListeId = this.GetListeIdentifiantMany().ConvertListObjectToListInt();
                else
                    (criteres as ICriteresBase).ListeId = this.GetListeIdentifiant().ConvertListObjectToListInt();

                (criteres as ICriteresBase).SkipTake = null;
            } else
                typeCriteres.GetProperty(this._foreignKeyAttribute.Property).SetValue(criteres, this.GetListeIdentifiant());

            return criteres;
        }

        /// <summary>
        /// Récupere une liste d'identifiant qui sera injectée à la classe de criteres
        /// Dans le cas d'une relation ToOne, la liste d'identifiant contiendra la liste des clés étrangeres
        /// Dans le cas d'une relation ToMany, la liste d'identifiant contiendra la liste des identifiants des ressources parentes
        /// </summary>
        private object GetListeIdentifiant()
        {
            PropertyInfo propertyInfo = this._typeRelation == TypeRelation.One ?
                this._foreignKeyProperty :
                AttributeHandling.GetIdProperty(this._informationRessource.TypeRessource);

            Type typeListe = typeof(List<>);
            Type typeListeGeneric = typeListe.MakeGenericType(propertyInfo.PropertyType);
            object listeIdentifiant = Activator.CreateInstance(typeListeGeneric);

            foreach (object ressource in this._informationRessource.RessourceEnum) {
                typeListeGeneric.GetMethod("Add").Invoke(listeIdentifiant, new object[] { propertyInfo.GetValue(ressource) });
            }

            return listeIdentifiant;
        }

        /// <summary>
        /// Récuperer une liste d'identifiant qui sera injectée à la classe de criteres 
        /// Uniquement pour un cas de relation ToMany (si aucune propriété dans l'attribut foreignKey n'est précisée)
        /// Dans ce cas, la liste d'identifiant contiendra un SelectMany.Distinct sur les liste des identifiants clé (ToMany)
        /// </summary>
        /// <returns></returns>
        private object GetListeIdentifiantMany()
        {
            if (this._foreignKeyProperty.PropertyType == typeof(List<decimal>))
                return this._informationRessource.RessourceEnum.SelectMany(r => this._foreignKeyProperty.GetValue(r) as List<decimal>).Distinct().ToList();

            else if (this._foreignKeyProperty.PropertyType == typeof(List<int>))
                return this._informationRessource.RessourceEnum.SelectMany(r => this._foreignKeyProperty.GetValue(r) as List<int>).Distinct().ToList();

            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Récupere l'expression boolean pour la clause Where dans "BindRelations"
        /// </summary>
        /// <returns></returns>
        private Expression<Func<T, bool>> GetExpressionPredicate(object element)
        {
            if (this._foreignKeyProperty.PropertyType == typeof(decimal?)) {
                return r => (decimal)this._identifiantRelationProperty.GetValue(r) == (this._foreignKeyProperty.GetValue(element) as decimal?).GetValueOrDefault();
            }

            if (this._foreignKeyProperty.PropertyType == typeof(List<decimal>)) {
                return r => (this._foreignKeyProperty.GetValue(element) as List<decimal>).Contains((decimal)this._identifiantRelationProperty.GetValue(r));
            }

            if (this._foreignKeyProperty.PropertyType == typeof(List<int>)) {
                return r => (this._foreignKeyProperty.GetValue(element) as List<int>).Contains((int)this._identifiantRelationProperty.GetValue(r));
            }

            if (this._foreignKeyProperty.PropertyType == typeof(int?)) {
                return r => (int)this._identifiantRelationProperty.GetValue(r) == (this._foreignKeyProperty.GetValue(element) as int?).GetValueOrDefault();
            }

            return r => (decimal)this._identifiantRelationProperty.GetValue(r) == (decimal)this._foreignKeyProperty.GetValue(element);
        }
    }
}
