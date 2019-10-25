using JsonApi.Core;
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
    /// POC
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RelationSavingEngine<T>
    {
        #region Propriétés
        /// <summary>
        /// Information sur la ressource parente (celle qui contient les relations)
        /// </summary>
        private readonly InformationRessource _informationRessource;
        private readonly IServiceProvider _serviceProvider;

        // Information sur la proprieté relation T
        private readonly PropertyInfo _relationProperty;

        // Information sur la proprieté clé de la relation
        private readonly PropertyInfo _foreignKeyProperty;

        /// <summary>
        /// Many | One
        /// </summary>
        private readonly TypeRelation _typeRelation;
        private readonly object _idRessourceParent;
        #endregion

        #region Constructeur
        public RelationSavingEngine(
            IServiceProvider serviceProvider,
            InformationRessource informationRessource,
            PropertyInfo propertyRelation,
            RelationshipJsonApiAttribute relationshipJApiAttribute,
            object idRessourceParent)
        {
            this._serviceProvider = serviceProvider;
            this._informationRessource = informationRessource;
            this._relationProperty = propertyRelation;
            this._idRessourceParent = idRessourceParent;

            (PropertyInfo foreignKeyProperty, ForeignKeyJsonApiAttribute foreignKeyAttribute) =
                AttributeRelationsHandling.FindForeignKeyPropertyAndAttribute(this._informationRessource.TypeRessource, this._relationProperty.Name);

            if (foreignKeyProperty == null && foreignKeyAttribute == null)
                throw new JsonApiArchitectureException($"ForeignKeyJsonApi manquant pour {this._relationProperty.Name} dans {this._informationRessource.TypeRessource.Name}, ajouter[ForeignKeyJsonApi (RelationName = nameof({this._relationProperty.Name}))]");

            this._foreignKeyProperty = foreignKeyProperty;
            this._typeRelation = relationshipJApiAttribute is HasManyJsonApiAttribute ? TypeRelation.Many : TypeRelation.One;

            this.SaveRelations();
        }
        #endregion

        public void SaveRelations()
        {
            object relationValue = this._relationProperty.GetValue(this._informationRessource.Ressource);

            if (relationValue == null)
                return;

            object repoInstance = this.GetRepositoryInstance();
            MethodInfo updateMethod = this.GetMethodeUpdateInfo(repoInstance.GetType());

            if (this._typeRelation == TypeRelation.One)
                this.UpdateWithRepository(updateMethod, repoInstance, relationValue);

            else if (this._typeRelation == TypeRelation.Many)
                this.UpdateListeWithRepository(updateMethod, repoInstance, relationValue);
        }

        private void UpdateWithRepository(MethodInfo updateMethod, object repoInstance, object relationValue)
        {
            // int idRelation = (updateMethod.Invoke(repoInstance, new object[] { relationValue }) as Task<int>).GetAwaiter().GetResult();

            this._foreignKeyProperty.SetValue(this._informationRessource.Ressource, AttributeHandling.GetIdProperty(typeof(T)).GetValue(relationValue));
        }

        private void UpdateListeWithRepository(MethodInfo updateMethod, object repoInstance, object relationValue)
        {
            PropertyInfo foreignKeyProperty = AttributeRelationsHandling.FindForeignKeyPropertyFromType(typeof(T), this._informationRessource.TypeRessource);

            if (foreignKeyProperty == null)
                throw new JsonApiArchitectureException($"Une relation est manquante sur {typeof(T).Name} de type {this._informationRessource.TypeRessource.Name}");

            foreach (object value in (IEnumerable<object>)relationValue) {
                foreignKeyProperty.SetValue(value, this._idRessourceParent);
                //new RelationsEngine(this._serviceProvider, this._queryService, value).SaveRelation();
            }

            relationValue = this.CastListToParameterMethod(updateMethod, relationValue);
            (updateMethod.Invoke(repoInstance, new object[] { relationValue }) as Task).GetAwaiter().GetResult();
        }

        private object GetRepositoryInstance()
        {
            Type repositoryType = AttributeRelationsHandling.GetRepositoryType(typeof(T));

            if (repositoryType == null)
                throw new JsonApiArchitectureException($"Préciser le repository à utiliser sur {typeof(T).Name} grâce à l'attribute [RessourceJsonApiAttribute]");

            object repoInstance = this._serviceProvider.GetService(repositoryType);

            if (repoInstance == null)
                throw new JsonApiArchitectureException($"Ajouter {repositoryType.Name} dans les services (services.AddScoped<>)");

            return repoInstance;
        }

        private MethodInfo GetMethodeUpdateInfo(Type repositoryType)
        {
            MethodInfo updateMethod = this._typeRelation == TypeRelation.Many ?
                repositoryType.GetMethod(nameof(JsonApiArchitectureBase.IRepository<T>.UpdateListeAsync))
                : repositoryType.GetMethod(nameof(JsonApiArchitectureBase.IRepository<T>.UpdateAsync));

            if (updateMethod == null)
                throw new JsonApiArchitectureException($"Hériter {AttributeRelationsHandling.GetRepositoryType(typeof(T)).Name} de IRepository");

            return updateMethod;
        }

        private object CastListToParameterMethod(MethodInfo methodinfo, object valueObj)
        {
            Type typeListeInterface = methodinfo.GetParameters()[0].ParameterType;
            Type typeInterface = typeListeInterface.GetGenericArguments()[0];

            MethodInfo castMethod = typeof(Enumerable).GetMethod("Cast")
                .MakeGenericMethod(new Type[] { typeInterface });

            MethodInfo toArrayMethod = typeof(Enumerable).GetMethod("ToList")
                .MakeGenericMethod(new Type[] { typeInterface });

            object castedObjectEnum = castMethod.Invoke(null, new object[] { valueObj });
            object liste = toArrayMethod.Invoke(null, new object[] { castedObjectEnum });

            return liste;
        }
    }
}
