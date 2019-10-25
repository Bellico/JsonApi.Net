using JsonApiArchitectureBase.Interfaces;
using JsonApi.Core;
using JsonApi.Interface;
using JsonApiArchitecture.Attributes;
using JsonApiArchitecture.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JsonApiArchitecture.Core
{
    /// <summary>
    /// Moteur de récupération d'une relation pour une propriété (une ressource T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal partial class RelationsPropertyEngine<T>
    {
        #region Propriétés
        /// <summary>
        /// Information sur la ressource parente (celle qui contient les relations)
        /// </summary>
        private readonly InformationRessource _informationRessource;
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueryService _queryService;

        // Information sur la proprieté relation T
        private readonly PropertyInfo _relationProperty;
        private readonly RelationshipJsonApiAttribute _relationAttribute;
        private PropertyInfo _identifiantRelationProperty;

        // Information sur la proprieté clé de la relation
        private readonly PropertyInfo _foreignKeyProperty;
        private readonly ForeignKeyJsonApiAttribute _foreignKeyAttribute;

        /// <summary>
        /// Many | One
        /// </summary>
        private readonly TypeRelation _typeRelation;
        private readonly int _depth;
        private readonly string _relationPath;
        #endregion

        #region Constructeur
        public RelationsPropertyEngine(
            IServiceProvider serviceProvider,
            IQueryService queryService,
            InformationRessource informationRessource,
            PropertyInfo propertyRelation,
            RelationshipJsonApiAttribute relationshipJApiAttribute,
            string _relationPath,
            int depth)
        {
            this._serviceProvider = serviceProvider;
            this._queryService = queryService;
            this._informationRessource = informationRessource;
            this._relationProperty = propertyRelation;
            this._relationAttribute = relationshipJApiAttribute;
            this._depth = depth;
            this._relationPath = _relationPath;

            (PropertyInfo foreignKeyProperty, ForeignKeyJsonApiAttribute foreignKeyAttribute) =
                AttributeRelationsHandling.FindForeignKeyPropertyAndAttribute(this._informationRessource.TypeRessource, this._relationProperty.Name);

            if (foreignKeyProperty == null && foreignKeyAttribute == null)
                throw new JsonApiArchitectureException($"ForeignKeyJsonApi manquant pour {this._relationProperty.Name} dans {this._informationRessource.TypeRessource.Name}, ajouter[ForeignKeyJsonApi (RelationName = nameof({this._relationProperty.Name}))]");

            this._identifiantRelationProperty = AttributeHandling.GetIdProperty(typeof(T));

            if (this._identifiantRelationProperty == null)
                throw new JsonApiArchitectureException($"Ajouter [IdJsonApi] sur {typeof(T).Name}");

            this._foreignKeyProperty = foreignKeyProperty;
            this._foreignKeyAttribute = foreignKeyAttribute;
            this._typeRelation = this._relationAttribute is HasManyJsonApiAttribute ? TypeRelation.Many : TypeRelation.One;

            this.GetRelations();
        }
        #endregion

        /// <summary>
        /// 1 - Appelle le datasource pour récuperer toutes les relations T
        /// </summary>
        private void GetRelations()
        {
            // Récupération du datasource définit sur la ressource
            Type datasourcetype = AttributeRelationsHandling.GetDataSourceType(typeof(T));

            if (datasourcetype == null)
                throw new JsonApiArchitectureException($"Préciser le datasource à utiliser sur {typeof(T).Name} grâce à l'attribute [RessourceJsonApiAttribute]");

            // Récupération de l'instance du datasource dans les services
            object datasourceInstance = this._serviceProvider.GetService(datasourcetype);

            if (datasourceInstance == null)
                throw new JsonApiArchitectureException($"Ajouter {datasourcetype.Name} dans les services (services.AddScoped<>)");

            MethodInfo ressourceListeMethod =
                datasourceInstance.GetType().GetMethod(nameof(JsonApiArchitectureBase.IDataSource<T, ICriteresBase>.GetListeRessourceAsync))
                .MakeGenericMethod(new Type[] { typeof(T) });

            // Définition des criteres à utiliser pour le datasource
            object criteres = this.GetCriteres();

            // Définition des champs à récuperer
            List<string> fields = this.GetFields();

            // Appelle de la fonction GetListeRessourceAsync<T> et récupération des relations
            List<T> relations = ((Task<List<T>>)ressourceListeMethod.Invoke(datasourceInstance, new object[] { criteres, fields })).GetAwaiter().GetResult();

            // Recherche des relations récursivement
            new RelationsEngine(this._serviceProvider, this._queryService, relations).BuildRelation(this._depth + 1, this._relationPath);

            // Affectation des relations trouvées aux bonnes ressources
            this.BindRelations(relations);
        }

        /// <summary>
        /// 2 - Parcours la ressource parente et affecte dans la propriété de relation T, les relations trouvées correspondantes
        /// </summary>
        /// <param name="relations"></param>
        private void BindRelations(List<T> relations)
        {
            // Il a été noté que lorsque le nombre de ressource récupérée est important, l'utilisation de Parallel.ForEach améliore les performances du traitement sur boucle
            Parallel.ForEach(this._informationRessource.RessourceEnum, (ressource) => {
                try {
                    if (this._typeRelation == TypeRelation.Many)
                        this._relationProperty.SetValue(ressource, relations.Where(this.GetExpressionPredicate(ressource).Compile()).ToList());
                    else
                        this._relationProperty.SetValue(ressource, relations.FirstOrDefault(this.GetExpressionPredicate(ressource).Compile()));
                } catch (NullReferenceException ex) {
                    throw new JsonApiArchitectureException($"La valeur de la clé {this._foreignKeyProperty.Name} dans {this._informationRessource.TypeRessource.Name} n'est pas renseignée{Environment.NewLine}{ex.Message}");
                } catch (InvalidCastException ex) {
                    throw new JsonApiArchitectureException($"Utiliser l'attribut HasManyToManyJsonApi sur {this._relationProperty.Name} dans {this._informationRessource.TypeRessource.Name} et définir le parametre 'Key'{Environment.NewLine}{ex.Message}");
                }
            });
        }
    }
}
