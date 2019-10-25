using JsonApi.Core;
using JsonApi.Helpers;
using JsonApi.Services;
using JsonApiArchitecture.Core;
using JsonApiArchitecture.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiArchitecture.Services
{
    public class QueryServiceArchitecture : QueryService
    {
        public QueryServiceArchitecture(IHttpContextAccessor httpContextAccessor, JsonApiService jsonapi) : base(httpContextAccessor, jsonapi)
        {
        }

        #region Fields With Include notion
        /// <summary>
        ///  Récupere la liste des champs donnés dans la Query String pour un type de modele
        ///  Si des "include" sont demandés au premier niveau, les champs clés des relations sont ajoutés également dans la liste des champs
        /// </summary>
        public override List<string> GetListeFields(Type type)
        {
            IEnumerable<PropertyInfo> relationshipProperties =
               AttributeRelationsHandling.GetRelationshipProperties(type)
               .Where(p => Utils.IsRelationInInclude(AttributeHandling.GetLabelProperty(p.Item1), this._includes))
               .Select(p => p.Item1);

            return this.GetListeFieldsFromRelations(type, relationshipProperties);
        }

        /// <summary>
        ///  Récupere la liste des champs donnés dans la Query String pour un type de modele
        ///  Si des "include" sont demandés, les champs clés des relations sont ajoutés également dans la liste des champs
        /// </summary>
        public override List<string> GetListeFields(Type type, string relationPath)
        {
            IEnumerable<PropertyInfo> relationshipProperties =
                AttributeRelationsHandling.GetRelationshipProperties(type)
                .Where(p => Utils.IsRelationInInclude(relationPath + "." + AttributeHandling.GetLabelProperty(p.Item1), this._includes))
                .Select(p => p.Item1);

            return this.GetListeFieldsFromRelations(type, relationshipProperties);
        }

        /// <summary>
        ///  Concatene la liste des champs donnés dans la Query String pour un nom de modele json api
        ///  Et la liste les champs clés des propriétés de relations
        /// </summary>
        private List<string> GetListeFieldsFromRelations(Type type, IEnumerable<PropertyInfo> relationshipProperties)
        {
            // Récuperation des champs simple pour le type de modele
            List<string> fields = base.GetListeFields(type);

            List<string> fieldsKey = new List<string>();

            foreach (PropertyInfo relation in relationshipProperties) {
                PropertyInfo foreignKeyProperty = AttributeRelationsHandling.FindForeignKeyPropertyAndAttribute(type, relation.Name).Item1;

                if (foreignKeyProperty == null)
                    throw new JsonApiArchitectureException($"ForeignKeyJsonApi manquant pour {relation.Name} dans {type.Name}, ajouter[ForeignKeyJsonApi (RelationName = nameof({relation.Name}))]");

                fieldsKey.Add(foreignKeyProperty.Name);
            }

            if (fields != null && fieldsKey.Any()) {
                fields.AddRange(fieldsKey);
                return fields;
            }

            if (fields == null && fieldsKey.Any()) {
                return fieldsKey;
            }

            return fields;
        }
        #endregion
    }
}
