using JsonApi.Exceptions;
using JsonApi.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace JsonApi.Core
{
    public class JsonApiReader
    {
        #region Property
        private readonly JObject _jsonObject;
        private readonly Type _typeModel;
        private readonly object _modelToBind;
        private int _constraintId;
        private bool _isPostRequest;
        private readonly JsonApiError _error;
        private ResolverData _resolverData;
        #endregion

        #region Construtor
        /// <summary>
        /// Initialize data
        /// </summary>
        /// <param name="data">object in format json</param>
        /// <param name="expectedObject">object expected in action controller</param>
        public JsonApiReader(string data, object expectedObject)
        {
            this._typeModel = expectedObject.GetType();
            this._modelToBind = expectedObject;
            this._error = new JsonApiError();

            if (string.IsNullOrEmpty(data))
            {
                throw new EmptyJsonObjectException();
            }

            try
            {
                this._jsonObject = JObject.Parse(data);
            }
            catch (JsonReaderException ex)
            {
                throw new ErrorParsingException(ex);
            }
        }
        #endregion

        #region Public Method
        /// <summary>
        /// Run binding
        /// </summary>
        /// <returns></returns>
        public object GetModel(bool force = false)
        {

            object model = this.Dispatch();

            if (!force && this._error.HasErrors())
            {
                throw new JsonApiException(this._error);
            }

            return model;
        }
      

        public void SetConstraintId(object id, string method)
        {
            if (id != null)
            {
                this._constraintId = (int)id;
            }

            if(method == "POST")
            {
                this._isPostRequest = true;
            }
        }

        public void SetResolver(ResolverData resolverData) => this._resolverData = resolverData;
        #endregion

        #region Start
        /// <summary>
        /// Check if type expexted is a list or a simple object
        /// </summary>
        /// <returns></returns>
        private object Dispatch()
        {
            if (Utils.IsEnum(this._modelToBind))
            {
                if (this.GetTypeJProperty(this._jsonObject) != "Array")
                {
                    throw new InvalidPropertyException("json", "data", "Array");
                }

                Type type = this._typeModel.GetGenericArguments()[0];
                this.BindList(type, this._modelToBind, this._jsonObject);
            }
            else
            {
                this.BindModel(this._typeModel, this._modelToBind, this._jsonObject);
            }

            return this._modelToBind;
        }
        #endregion

        #region Binding
        /// <summary>
        /// Make binding on an object type list with json array
        /// </summary>
        /// <param name="type">Type generic in the list</param>
        /// <param name="model">Instance of the list</param>
        /// <param name="jData">Array data json representing a list object</param>
        private void BindList(Type type, object model, JToken jData)
        {
            if (Utils.IsTypeSystem(type))
            {
                throw new NotImplementedJsonApiException();
            }

            foreach (JObject item in jData["data"])
            {
                object modelItem = Activator.CreateInstance(type);
                this.BindModel(type, modelItem, item);
                ((IList)model).Add(modelItem);
            }
        }

        /// <summary>
        /// Binding object with object json
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <param name="modelToBind">Instance of object</param>
        /// <param name="item">Object data json representing the object</param>
        private void BindModel(Type type, object modelToBind, JToken item)
        {
            // Check type
            if (this.ReadType(item) != AttributeHandling.GetLabelProperty(type)) 
                throw new InvalidPropertyException("type", this.ReadType(item), AttributeHandling.GetLabelProperty(type), (!string.IsNullOrEmpty(item.Path) ? item.Path : item.First.Path));

            // Bind Id
            this.BindId(type, modelToBind, item);

            // Bind Attributes
            IEnumerable<JToken> attributes = this.ReadAttributes(item);
            if (attributes != null)
                this.BindAttributes(type, modelToBind, attributes);

            // Bind Relationships
            IEnumerable<JToken> relationships = this.ReadRelationships(item);
            if (relationships != null)
                this.BindRelationships(type, modelToBind, relationships);
        }

        /// <summary>
        /// Bind object property Id
        /// <param name="type">Type object</param>
        /// <param name="modelToBind">Instance object</param> 
        /// </summary>
        private void BindId(Type type, object modelToBind, JToken item)
        {
            string id = this.ReadId(item);

            if (id == null)
                return;

            List<PropertyInfo> listeIdProperty = AttributeHandling.GetIdsProperties(type).Where(prop => prop.CanWrite).ToList();

            if (!listeIdProperty.Any())
                throw new NotImplementedJsonApiException($"{type.Name} has not attribute id");

            if (listeIdProperty.Count == 1) {
                // Check constraint Id on main Model
                if (modelToBind == this._modelToBind) {
                    if (this._constraintId > 0 && id != this._constraintId.ToString()) 
                        throw new InvalidPropertyException("id", id, this._constraintId.ToString());

                    if (this._isPostRequest && id != "0") 
                        throw new InvalidPropertyException("id", id, "empty", "POST Method");
                }

                this.BindIdValue(listeIdProperty.First(), modelToBind, item, id);
            }
            else {
                string[] ids = id.Split(Constants.SEPERATOR_IDS);

                if (listeIdProperty.Count != ids.Length) {
                    this._error.Create(Constants.ERROR_STATUT_JSONAPI, $"Error Format on attribute Id", "Id value is incorrect", item.ToString());
                    return;
                }

                foreach (PropertyInfo idProperty in listeIdProperty) 
                    this.BindIdValue(idProperty, modelToBind, item, ids[listeIdProperty.IndexOf(idProperty)]);
            }
        }

        private void BindIdValue(PropertyInfo idProperty, object modelToBind, JToken item, string id)
        {
            object idValue = Utils.FormatValueByType(idProperty.PropertyType, id);

            try {
                idProperty.SetValue(modelToBind, idValue);
            } catch (FormatException ex) {
                this._error.Create(Constants.ERROR_STATUT_JSONAPI, $"Error Format on attribute Id", ex.Message, item.ToString());
            }
        }

        /// <summary>
        /// Bind object property with simple type (string, bool, date, int)
        /// </summary>
        /// <param name="type">Type object</param>
        /// <param name="modelToBind">Instance object</param>
        /// <param name="attributes">List properties values</param>
        private void BindAttributes(Type type, object modelToBind, IEnumerable<JToken> attributes)
        {
            // Set known values
            foreach (JProperty attr in attributes)
            {
                PropertyInfo property = AttributeHandling.GetProperty(type, attr.Name);
                
                if (property == null)
                {
                    this._error.Create(Constants.ERROR_STATUT_JSONAPI, $"Invalid Property {attr.Name}", $"{ AttributeHandling.GetLabelProperty(type)} has not property {attr.Name}", "");
                }
                else if(property.CanWrite)
                {
                    try
                    {
                        Type typeAttribute = AttributeHandling.GetTypeProperty(property);
                        object valueFormat = this.ReadJsonValue(typeAttribute, attr);

                        if (this.ValidateData(property, attr.Name, valueFormat))
                        {
                            valueFormat = this.ResolveData(property, valueFormat);
                            property.SetValue(modelToBind, valueFormat);
                        }
                    }
                    catch (FormatException ex)
                    {
                        this._error.Create(Constants.ERROR_STATUT_JSONAPI, $"Error Format on attribute {attr.Name}", ex.Message, "");
                    }
                }
            }

            // Set empty properties with resolvers
            foreach(PropertyInfo property in type.GetProperties().Where(p => p.CanWrite && p.GetValue(modelToBind) == null)) {
                object valueResolved = this.ResolveData(property, null);
                if (valueResolved != null) property.SetValue(modelToBind, valueResolved);
            }
        }

        /// <summary>
        /// Bind object property type of object
        /// </summary>
        /// <param name="type">Type objec</param>
        /// <param name="modelToBind">Instance object</param>
        /// <param name="relationships">List object</param>
        private void BindRelationships(Type type, object modelToBind, IEnumerable<JToken> relationships)
        {
            foreach (JProperty attr in relationships)
            {
                PropertyInfo relationship = AttributeHandling.GetProperty(type, attr.Name);

                if (relationship == null)
                {
                    throw new InvalidPropertyException(type, attr.Name, true);
                }

                Type typeRelationship = AttributeHandling.GetTypeProperty(relationship);
                object tempObj = Activator.CreateInstance(typeRelationship);

                if (Utils.IsEnum(tempObj))
                {
                    if (this.GetTypeJProperty(attr.First()) != "Array")
                    {
                        throw new InvalidPropertyException("json", attr.Name, "Array");
                    }

                    //--------Cas particulier------//
                    //Type spécifié
                    Type sendedType = typeRelationship.GetGenericArguments()[0];
                    //Type attendu 
                    Type waitingType = relationship.PropertyType.GetGenericArguments()[0];
                    //Instanciation particulière lorsque le type attendu dans le BO est une List<Interface>
                    if (waitingType.GetTypeInfo().IsInterface) { tempObj = Activator.CreateInstance(typeof(List<>).MakeGenericType(waitingType)); }
                    //---------------------------//

                    this.BindList(sendedType, tempObj, attr.First());
                }
                else
                {
                    this.BindModel(typeRelationship, tempObj, attr.First());
                }

                relationship.SetValue(modelToBind, tempObj);
            }
        }
        #endregion

        #region Access Method Json
        private string ReadType(JToken j)
        {
            try
            {
                if (j["data"] != null) return j["data"]["type"].ToString();
                else return j["type"].ToString();
            }
            catch (NullReferenceException)
            {
                throw new MissingPropertyException("type", j.ToString());
            }
            catch (ArgumentException)
            {
                throw new MissingPropertyException("type", j.ToString());
            }
        }

        private string ReadId(JToken j)
        {
            if (j["data"] != null) return j["data"]["id"]?.ToString();
            else return j["id"]?.ToString();
        }

        private string GetTypeJProperty(JToken j)
        {
            try
            {
                return j["data"].Type.ToString();
            }
            catch (NullReferenceException)
            {
                throw new MissingPropertyException("data", j.ToString());
            }
        }

        private IEnumerable<JToken> ReadAttributes(JToken j)
        {
            if (j["data"] != null) return j["data"]["attributes"];
            else return j["attributes"];
        }

        private IEnumerable<JToken> ReadRelationships(JToken j)
        {
            if (j["data"] != null) return j["data"]["relationships"];
            else return j["relationships"];
        }

        private object ReadJsonValue(Type typeAttribute, JProperty attr)
        {
            //Check if the value is enum or a simple string
            List<string> list = attr.First().Select(m => m.ToString()).ToList();
            if (list.Count > 0)
            {
                return Utils.FormatListValueByType(typeAttribute, list);
            }
            else
            {
               if(attr.Value.Type == JTokenType.Null)
                    return Utils.FormatNullValue(typeAttribute);
                else
                    return Utils.FormatValueByType(typeAttribute, attr.First().ToString());
            }
        }
        #endregion

        #region Utils
        private bool ValidateData(PropertyInfo property, object name, object value)
        {
            bool isValidate = true;
            foreach (ValidationAttribute attribute in property.GetCustomAttributes<ValidationAttribute>()) {
                if (!attribute.IsValid(value)) {
                    this._error.Create(Constants.ERROR_STATUT_JSONAPI, $"Error Format on attribute {name}", string.Format(attribute.ErrorMessage, name), "");
                    isValidate = false;
                }
            }

            return isValidate;
        }

        private object ResolveData(PropertyInfo prop, object data)
        {
            if (this._resolverData == null)
                return data;

            string resolverName = AttributeHandling.GetResolverName(prop);

            if (resolverName == null)
                return data;

            return this._resolverData.ResolveReader(resolverName, data);
        }
        #endregion
    }
}
