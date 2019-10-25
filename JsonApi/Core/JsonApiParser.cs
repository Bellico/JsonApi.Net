using JsonApi.Components;
using JsonApi.Helpers;
using JsonApi.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApi.Core
{
    public class JsonApiParser
    {
        #region Property
        private object _modelData;
        private Dictionary<string, object> _meta;
        private ResolverData _resolverData;

        #region Parameter Speciaux
        private bool _modeExtand = false;
        #endregion

        #region Parameter Pagination
        private int[] _pagination;
        private Dictionary<string, string> _links;
        private bool _isCustomPagination;
        #endregion

        #region Parameter Include
        private List<IJsonApiObject> _includedRelationship;
        private List<string> _includedQuery = new List<string>();
        private Dictionary<string, List<string>> _fields = new Dictionary<string, List<string>>();
        private readonly List<string> _relationship_id = new List<string>();
        #endregion
        #endregion

        #region Construtors
        public JsonApiParser()
        {

        }

        public JsonApiParser(object ModelData)
        {
            this._modelData = ModelData;
        }
        #endregion

        #region Public Main Method
        public void SetModel(object ModelData) 
            => this._modelData = ModelData;

        public void SetResolver(ResolverData resolverData) 
            => this._resolverData = resolverData;

        /// <summary>
        /// Permet d'avoir l'arborescence complete des relations et ne plus avoir les "includes" dissociés
        /// </summary>
        /// <returns></returns>
        public JsonApiParser ModeExtand()
        {
            this._modeExtand = true;
            this._includedRelationship?.Clear();
            this._relationship_id?.Clear();
            this._includedRelationship = null;

            return this;
        }

        /// <summary>
        /// Return object jsonapi
        /// </summary>
        /// <returns></returns>
        public object GetJson()
        {
            if (this._modelData == null) return null;

            return JObject.FromObject(new
            {
                data = this.Dispatch(this._modelData, true),
                included = this._includedRelationship,
                meta = this._meta,
                links = this._links
            }, Constants.GetJsonSerializer());
        }

        public void SetOptionsWithQuery(Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            this._includedQuery = Utils.FormatQueryToInclude(query[Constants.HTTP_QUERY_INCLUDE]);
            this._fields = Utils.FormatQueryToFields(query.Where(m => m.Key.Contains("fields")));
            this.MakePagination(string.Join("&", query.Where(q => q.Key != "page").Select(q => q.Key + "=" + q.Value).ToArray()));
        }
        #endregion

        #region Include
        public void AddIncludeQuery(string parameter)
        {
            foreach (string item in parameter.Split(','))
            {
                this._includedQuery.Add(item);
            }
        }

        private void AddInclude(IJsonApiObject value)
        {
            if (this._modeExtand) return;

            if (value.id == null)
            {
                throw new ArgumentException($"Id missing in {value.type}, use attribute [IdJsonApi]");
            }

            string id = value.type + value.id;

            if (this._includedRelationship == null)
            {
                this._includedRelationship = new List<IJsonApiObject>();
            }

            if (!this._relationship_id.Contains(id))
            {
                this._includedRelationship.Add(value);
                this._relationship_id.Add(id);
            }
        }

        private bool IsInclude(string name)
        {
            return Utils.IsRelationInInclude(name, this._includedQuery); 
        }
        #endregion

        #region Meta
        public void AddMeta(string name, object data)
        {
            if (this._meta == null)
            {
                this._meta = new Dictionary<string, object>();
            }
            this._meta.Add(name, this.Dispatch(data, true));
        }
        #endregion

        #region Paginatation
        /// <summary>
        /// Précise au Json Parser que l'on souhaite une pagination
        /// </summary>
        /// <param name="page_number">numero de la page</param>
        /// <param name="page_size">taille de la page</param>
        /// <param name="total">nombre total d'élement</param>
        /// <param name="custom">Si le nombre de résultat est geré ou non en amont, si false le parser limite automatiquement le resultat de sortie, si true le nombre de résultat doit etre calculé</param>
        public void CreatePagination(Tuple<int,int> page, int total, bool custom)
        {
            this.AddMeta("total", total);

            if (page == null) return;

            if (page.Item1 > 0 && page.Item2 > 0)
            {
                this._pagination = new int[] { page.Item1, page.Item2, total };
                this._isCustomPagination = custom;
            }
            else
            {
                throw new ArgumentException("Parametre pagination incorrect");
            }
        }

        private void MakePagination(string query)
        {
            if (this._pagination != null)
            {
                if (this._pagination[0] > 1)
                    this.AddLink("prev", Utils.FormatLinkPagination(this._pagination[0] - 1, this._pagination[1], query)); 

                if (this._pagination[2] > 0 && this._pagination[0] * this._pagination[1] < this._pagination[2])
                    this.AddLink("next", Utils.FormatLinkPagination(this._pagination[0] + 1, this._pagination[1], query));
            }
        }

        public void AddLink(string name, string value)
        {
            if (this._links == null)
            {
                this._links = new Dictionary<string, string>();
            }
            this._links.Add(name, value);
        }
        #endregion

        #region Set Data
        /// <summary>
        /// Create a object or an array object if model is a list of object or an object
        /// </summary>
        /// <param name="model">instance model convert to json</param>
        /// <param name="expandData">true if we want add to the object json : attributes, relationship and include</param>
        /// <returns></returns>
        private object Dispatch(object model, bool expandData)
        {
            if (Utils.IsEnum(model))
            {
                if (model == this._modelData && !this._isCustomPagination && this._pagination != null)
                    return this.SetListDataWithPagination(model, expandData).Select(m => m.toJsonFormat()).ToList();
                else
                    return this.SetListData(model, expandData).Select(m => m.toJsonFormat()).ToList();  
            }
            else
            {
                return this.SetData(model, expandData).toJsonFormat();
            }
        }

        /// <summary>
        /// Set a list of object
        /// </summary>
        /// <param name="model"></param>
        /// <param name="expandData"></param>
        /// <returns></returns>
        private List<IJsonApiObject> SetListData(object model, bool expandData, string pathRelation = null)
        {
            var data = new List<IJsonApiObject>();

            foreach (object item in (IEnumerable)model)
            {
                data.Add(this.SetData(item, expandData, pathRelation));
            }

            return data;
        }

        /// <summary>
        /// Set a list of object with Pagination
        /// </summary>
        /// <param name="model"></param>
        /// <param name="expandData"></param>
        /// <returns></returns>
        private List<IJsonApiObject> SetListDataWithPagination(object model, bool expandData)
        {
            var data = new List<IJsonApiObject>();
            int i = 0;
            int count = 0;

            foreach (object item in (IEnumerable)model)
            {
                if(i >= this._pagination[0] && count < this._pagination[1])
                {
                    data.Add(this.SetData(item, expandData));
                    count++;
                }
                i++;
            }

            return data;
        }

        /// <summary>
        /// Create base object (id, type) and add attributes
        /// </summary>
        /// <param name="model">instance</param>
        /// <param name="extandData">true return attributes in the object</param>
        /// <returns></returns>
        private IJsonApiObject SetData(object model, bool extandData, string pathRelation = null)
        {
            if (model == null) return null;

            if (pathRelation != null) pathRelation += ".";

            Type type = model.GetType();

            if (model is IDictionary<string, string>)
            {
                return this.CreateJsonApiListKeyValue(model);
            }

            if (model is List<KeyValuePair<string, string>>)
            {
                return this.CreateJsonApiListKeyValue(model);
            }

            if (Utils.IsTypeSystem(type))
            {
                return this.CreateJsonApiForSystemObject(model, type);
            }

            IJsonApiObject json = this.CreateJsonApiOject(model, type, extandData);
            if (extandData)
            {
                this.CreateAttribute(model, type, (JsonApiData)json, pathRelation);
            }

            return json;
        }
        #endregion

        #region Initialize Json Object
        /// <summary>
        /// Add property value for Systeme object
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IJsonApiObject CreateJsonApiForSystemObject(object model, Type type)
        {
            return new JsonApiDataBaseSystem
            {
                type = type.Name,
                value = model.ToString()
            };
        }

        private IJsonApiObject CreateJsonApiListKeyValue(object model)
        {
            var returnObject = new JsonApiData();
            foreach (KeyValuePair<string, string> entry in (IEnumerable) model)
            {
                returnObject.AddAttribute(entry.Key, entry.Value);
            }
            return returnObject;
        }

        /// <summary>
        /// Create base object (id, type)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <param name="extandData"></param>
        /// <returns></returns>
        private IJsonApiObject CreateJsonApiOject(object model, Type type, bool extandData)
        {
            IJsonApiObject returnObject = extandData ? new JsonApiData() : new JsonApiDataBase();

            IEnumerable<PropertyInfo> idsProperties = AttributeHandling.GetIdsProperties(type);
            if (idsProperties.Any())
                returnObject.id = string.Join(Constants.SEPERATOR_IDS, idsProperties.Select(p => p.GetValue(model).ToString()));

            string labelType = AttributeHandling.GetLabelProperty(type);
            returnObject.type = (!type.Name.Contains("AnonymousType")) ? labelType : null;

            return returnObject;
        }
        #endregion

        #region Utils
        private IEnumerable<PropertyInfo> GetListProperties(Type type, string typeJson)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties()
                .Where(p => !AttributeHandling.GetIdsProperties(type).Contains(p))
                .Where(p => !AttributeHandling.IsIgnoreJsonApi(p))
                .Select(p => p);

            // Si des champs sont selectionnés, on récupere seulement les champs selectionés et les champs relations
            List<string> listSelected = this._fields.Where(m => m.Key == typeJson).Select(m => m.Value).FirstOrDefault();

            if (listSelected != null) {
                properties = properties.Where(p => 
                listSelected.Contains(AttributeHandling.GetLabelProperty(p)) 
                || !Utils.IsTypeSystem(p.PropertyType)
                || !Utils.HasGenericTypeSystem(p.PropertyType));
            }

            return properties;
        }

        private object ResolveData(PropertyInfo prop, object data)
        {
            if (this._resolverData == null)
                return data;

            string resolverName = AttributeHandling.GetResolverName(prop);

            if (resolverName == null)
                return data;

            return this._resolverData.ResolveParser(resolverName, data);
        }
        #endregion

        #region Create Attribute & Relationship
        /// <summary>
        /// Add attributes, relationship and included relationship
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <param name="json"></param>
        private void CreateAttribute(object model, Type type, JsonApiData json, string pathRelation)
        {
            foreach (PropertyInfo prop in this.GetListProperties(type, json.type))
            {
                object value = this.ResolveData(prop, prop.GetValue(model));

                string propName = AttributeHandling.GetLabelProperty(prop);
               
                if (value != null)
                {
                    Type typeValue = value.GetType();
                    if (Utils.IsEnum(value))
                    {
                        if (Utils.HasGenericTypeSystem(typeValue))
                        {
                            json.AddAttribute(propName, this.SetListData(value, this._modeExtand).Select(m => m.toJsonFormat()).ToList());
                        }
                        else
                        {
                            this.CreateRelationship(json, propName, true, this.IsInclude(pathRelation + propName), (extand) => this.SetListData(value, extand, pathRelation + propName));
                        }
                    }
                    else if (Utils.IsTypeSystem(typeValue))
                    {
                        json.AddAttribute(propName, value);
                    }
                    else
                    {
                        this.CreateRelationship(json, propName, false, this.IsInclude(pathRelation + propName), (extand) => this.SetData(value, extand, pathRelation + propName));
                    }
                }
            }
        }

        private void CreateRelationship(JsonApiData json, string name, bool isEnum, bool isInclude, Func<bool,object> resultData)
        {
            if (isInclude)
            {
                json.AddRelationship(name, resultData(this._modeExtand));
                if (isEnum)
                {
                    foreach (IJsonApiObject item in (List<IJsonApiObject>) resultData(true))
                    {
                        this.AddInclude(item);
                    }
                }
                else
                {
                    this.AddInclude((IJsonApiObject) resultData(true));
                }
            }
        }
        #endregion
    }
}