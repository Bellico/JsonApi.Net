using JsonApi.Core;
using JsonApi.Interface;
using JsonApiArchitecture.Attributes;
using JsonApiArchitecture.Extensions;
using System;
using System.Reflection;

namespace JsonApiArchitecture.Core
{
    /// <summary>
    /// Moteur de récupération des relations
    /// </summary>
    public class RelationsEngine
    {
        private readonly InformationRessource _informationRessource;
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueryService _queryService;

        public RelationsEngine(IServiceProvider serviceProvider, IQueryService queryService, object ressource)
        {
            this._informationRessource = new InformationRessource(ressource);
            this._serviceProvider = serviceProvider;
            this._queryService = queryService;
        }

        /// <summary>
        /// Parcours les relations et effectue pour chacune d'elle une requete de récupération. Les résultats trouvés correspondant sont affectés au relations.
        /// </summary>
        /// <param name="depth">Niveau de profondeur de relation transmise récursivement</param>
        public void BuildRelation(int depth = 0, string baseRelationPath = null)
        {
            if (depth >= 5)
                return;

            if (!this._informationRessource.HaveType)
                return;

            if (baseRelationPath != null) baseRelationPath += ".";

            // Parcours des relations
            foreach (Tuple<PropertyInfo, RelationshipJsonApiAttribute> relation in AttributeRelationsHandling.GetRelationshipProperties(this._informationRessource.TypeRessource)) {

                string relationPath = baseRelationPath + AttributeHandling.GetLabelProperty(relation.Item1);

                if (!this._queryService.IsInclude(relationPath))
                    continue;

                //Instantiation de la classe RelationsPropertyEngine avec son type de ressource
                Type typeEngine = typeof(RelationsPropertyEngine<>).MakeGenericType(relation.Item1.GetPropertyTypeSample());
                Activator.CreateInstance(typeEngine, new object[] {
                    this._serviceProvider,
                    this._queryService,
                    this._informationRessource,
                    relation.Item1,
                    relation.Item2,
                    relationPath,
                    depth});
            }
        }

        /// <summary>
        /// Parcours les relations et appelle les repository correspondant
        /// </summary>
        public void SaveRelation()
        {
            if (!this._informationRessource.HaveType)
                return;

            if (this._informationRessource.IsEnum)
                return;

            object idRessource = AttributeHandling.GetIdProperty(this._informationRessource.TypeRessource).GetValue(this._informationRessource.Ressource);

            // Parcours des relations
            foreach (Tuple<PropertyInfo, RelationshipJsonApiAttribute> relation in AttributeRelationsHandling.GetRelationshipProperties(this._informationRessource.TypeRessource)) {
                //Instantiation de la classe RelationSavingEngine avec son type de ressource
                Type typeRessource = relation.Item1.PropertyType.IsGenericType ? relation.Item1.PropertyType.GetGenericArguments()[0] : relation.Item1.PropertyType;
                Type typeEngine = typeof(RelationSavingEngine<>).MakeGenericType(typeRessource);
                Activator.CreateInstance(typeEngine, new object[] {
                    this._serviceProvider,
                    this._informationRessource,
                    relation.Item1,
                    relation.Item2,
                    idRessource });
            }
        }
    }
}