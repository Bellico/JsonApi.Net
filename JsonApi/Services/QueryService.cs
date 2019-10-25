using JsonApi.Core;
using JsonApi.Exceptions;
using JsonApi.Helpers;
using JsonApi.Interface;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApi.Services
{
    /// <summary>
    /// Class utilitaires pour parser et recuperer les parametres en Query String
    /// </summary>
    public class QueryService : IQueryService
    {
        #region Constantes
        const string FILTER = "filters";
        const string SIZE = "size";
        const string PAGE = "page";
        const string SORT = "sort";
        const string SELECTION = "selection";
        const string PARAMETER = "params";
        const string INCLUDE = "include";
        const string FIELDS = "fields";
        #endregion

        #region Property
        readonly private JsonApiService _jsonapi;
        readonly private IQueryCollection _query;
        readonly private JObject _jFilter;
        readonly private JObject _jPage;
        readonly private JObject _jSort;
        readonly private JObject _jSelection;
        readonly private JObject _jParameter;
        readonly private Dictionary<string, List<string>> _fields;
        protected readonly List<string> _includes;
        #endregion

        #region Constructor
        /// <summary>
        /// Lis la query string, décode les parametres json
        /// </summary>
        /// <param name="httpContextAccessor">Context http</param>
        public QueryService(IHttpContextAccessor httpContextAccessor, JsonApiService jsonapi)
        {
            this._jsonapi = jsonapi;
            this._query = httpContextAccessor.HttpContext.Request.Query;

            this._jFilter = this.ParseQuery(FILTER);
            this._jPage = this.ParseQuery(PAGE);
            this._jSort = this.ParseQuery(SORT);
            this._jSelection = this.ParseQuery(SELECTION);
            this._jParameter = this.ParseQuery(PARAMETER);
            this._includes = Utils.FormatQueryToInclude(this._query[INCLUDE]);
            this._fields = Utils.FormatQueryToFields(this._query.Where(m => m.Key.Contains(FIELDS)));
        }

        public QueryService(params string[] element)
        {

        }
        #endregion

        #region Param Simple
        /// <summary>
        /// Recupere sous forme de string un parametre
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Param(string key, bool isRequired = false)
        {
            return this.Get(this._jParameter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme d'entier nullable un parametre
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int? ParamInt(string key, bool isRequired = false)
        {
            return this.GetInt(this._jParameter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme de boolean un parametre
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool? ParamBoolean(string key, bool isRequired = false)
        {
            return this.GetBoolean(this._jParameter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme de liste un parametre
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        public List<string> ParamList(string key, bool isRequired = false)
        {
            return this.GetList(this._jParameter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme de liste de decimal un parametre
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        public List<decimal> ParamListDecimal(string key, bool isRequired = false)
        {
            return this.GetListDecimal(this._jParameter, key, isRequired);
        }
        #endregion

        #region Param with Model
        public T Param<T>() where T : new()
        {
            return this.GetJsonApi<T>(this._jParameter);
        }

        public T Param<T>(string key) where T : new()
        {
            if (this._jParameter == null || this._jParameter[key] == null)
                return new T();

            return this.GetJsonApi<T>(this._jParameter[key]);
        }

        #endregion

        #region Filter Simple

        public bool HasFilter(string key)
        {
            string value = this.Get(this._jFilter, key, false);

            return !string.IsNullOrWhiteSpace(value) && value.ToLower() != "null";
        }

        /// <summary>
        /// Recupere sous forme de string un filtre
        /// </summary>
        /// <returns></returns>
        public string Filter(string key, bool isRequired = false)
        {
            return this.Get(this._jFilter, key, isRequired);
        }

        /// <summary>
        /// Recupere sous forme de liste un filtre
        /// </summary>
        /// <returns></returns>
        public List<string> FilterList(string key, bool isRequired = false)
        {
            return this.GetList(this._jFilter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme d'entier un filtre
        /// </summary>
        /// <returns></returns>
        public int? FilterInt(string key, bool isRequired = false)
        {
            return this.GetInt(this._jFilter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme de boolean un filtre
        /// </summary>
        /// <returns></returns>
        public bool? FilterBoolean(string key, bool isRequired = false)
        {
            return this.GetBoolean(this._jFilter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme de date un filtre
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DateTime? FilterDate(string key, bool isRequired = false)
        {
            return this.GetDate(this._jFilter, key, isRequired);
        }

        /// <summary>
        ///  Recupere sous forme de date un filtre
        /// </summary>
        /// <returns></returns>
        public DateTime? FilterDate(string key, string moment)
        {
            string value = this._jFilter?.GetValue(key)?[moment]?.ToString();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            bool isDate = DateTime.TryParse(value, out DateTime valueDate);

            if (!isDate) {
                throw new ArgumentException($"Query invalid|{key} must be a date");
            }

            return valueDate;
        }

        #endregion

        #region Filter with Model
        /// <summary>
        /// Recupere une instance de T et les valeur correspondantes dans la Query String
        /// </summary>
        /// <returns></returns>
        public T Filter<T>() where T : new()
        {
            return this.Filter<T>(this._jFilter);
        }

        public T FilterOnInclude<T>(string key) where T : new()
        {
            JObject filters = this.ParseQuery($"{FILTER}[{key}]");

            return this.Filter<T>(filters);
        }

        private T Filter<T>(JToken source) where T : new()
        {
            T model = this.GetJsonApi<T>(source);

            if (model is IQueryRead) {
                ((IQueryRead)model).ReadQuery(this);
            }

            return model;
        }

        public List<T> FilterToList<T>() where T : IQueryParseList, new()
        {
            if (this._jFilter == null)
                return new List<T>();

            var liste = new List<T>();

            foreach (KeyValuePair<string, JToken> filter in this._jFilter) {
                var element = new T();

                switch (filter.Value.Type) {
                    case JTokenType.String: {
                        element.Parse(filter.Key, filter.Value.ToString());
                        break;
                    }
                    case JTokenType.Array: {
                        List<string> data = JsonConvert.DeserializeObject<List<string>>(filter.Value.ToString());
                        element.Parse(filter.Key, data);
                        break;
                    }
                    case JTokenType.Object: {
                        element.Parse(filter.Key, filter.Value.ToDictionary(x => (x as JProperty).Name, x => x.First.ToString()));
                        break;
                    }
                    default: {
                        throw new NotImplementedException("Cas de figure non géré.");
                    }
                }

                liste.Add(element);
            }

            return liste;
        }
        #endregion

        #region Size Query
        /// <summary>
        ///  Recupere le parametre "size" formaté
        /// </summary>
        /// <returns></returns>
        public int[] Size()
        {
            string[] s = this._query[SIZE].ToString().Split('x');
            int w = 0;
            int h = 0;
            if (s?.Length > 0)
                int.TryParse(s[0], out w);
            if (s?.Length > 1)
                int.TryParse(s[1], out h);
            if (w > 0 && h > 0)
                return new int[] { w, h };
            else
                return null;
        }
        #endregion

        #region Sort Query
        /// <summary>
        ///  Recupere le parametre de tri
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, string>> Sorting()
        {
            if (this._jSort == null)
                return null;

            var item = (JProperty)this._jSort.First;

            if (item == null)
                return null;

            return new List<Tuple<string, string>>() { new Tuple<string, string>(item.Name, item.Value.ToString()) };
        }

        public string GetOrder(string key)
        {
            if (this._jSort == null)
                return null;

            var item = (JProperty)this._jSort["key"];
            string order = item.Value.ToString();

            if (!string.IsNullOrWhiteSpace(order) && new List<string> { "ASC", "DESC" }.Contains(order.ToUpper()))
                return order;
            else
                return null;
        }
        #endregion

        #region Option Selection
        /// <summary>
        ///  Recupere le parametre de selection "tous"
        /// </summary>
        /// <returns></returns>
        public bool IsAllSelected()
        {
            if (this._jSelection == null || this._jSelection["all"] == null)
                return false;

            return (this._jSelection["all"]?.ToString().ToLower() == "true");
        }

        /// <summary>
        ///  Recupere le parametre de selection des id selectionées
        /// </summary>
        /// <returns></returns>
        public List<decimal> GetSelectedId()
        {
            if (this._jSelection == null || this._jSelection["selected"] == null || !this._jSelection["selected"].Any())
                return null;

            try {
                return (this._jSelection["selected"].Select(v => decimal.Parse(v.ToString())).ToList());
            } catch {
                throw new ArgumentException($"Selection invalid|Error on selected parameter");
            }
        }

        /// <summary>
        ///  Recupere le parametre de selection des id exclues
        /// </summary>
        /// <returns></returns>
        public List<decimal> GetExcludedId()
        {
            if (this._jSelection == null || this._jSelection["excluded"] == null || !this._jSelection["excluded"].Any())
                return null;

            try {
                return (this._jSelection["excluded"].Select(v => decimal.Parse(v.ToString())).ToList());
            } catch {
                throw new ArgumentException($"Selection invalid|Error on excluded parameter");
            }
        }
        #endregion

        #region Pagination Query
        /// <summary>
        /// Recupere le parametre de pagination formaté présent dans la query
        /// </summary>
        /// <returns></returns>
        public Tuple<int, int> Page(int? default_size)
        {
            if (this._jPage != null) {
                int.TryParse(this._jPage["number"]?.ToString(), out int page_number);
                int.TryParse(this._jPage["size"]?.ToString(), out int page_size);

                if (page_number >= 0 && page_size > 0)
                    return new Tuple<int, int>(page_number, page_size);
            }

            return (default_size != null) ? new Tuple<int, int>(1, default_size.Value) : null;
        }

        /// <summary>
        /// Recupere le parametre de pagination formaté
        /// </summary>
        /// <returns></returns>
        public Tuple<int, int> PageOffset(int? default_size)
        {
            Tuple<int, int> page = this.Page(default_size);

            if (page != null && page.Item1 > 0 && page.Item2 > 0) {
                int page_number = (page.Item1 - 1) * page.Item2;

                return new Tuple<int, int>(page_number, page.Item2);
            }

            return page;
        }
        #endregion

        #region Include
        /// <summary>
        /// Définit si un type de modele est présent dans le parametre "include" de la Query String
        /// </summary>
        public bool IsInclude<T>() => this.IsInclude(AttributeHandling.GetLabelProperty(typeof(T)));

        /// <summary>
        /// Définit si un nom de modele JsonApi est présent dans le parametre "include" de la Query String
        /// </summary>
        public bool IsInclude(string relationPath) => Utils.IsRelationInInclude(relationPath, this._includes);
        #endregion

        #region Fields
        /// <summary>
        /// Récupere la liste des champs donnés dans la Query String pour un type de modele
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<string> GetListeFields<T>() => this.GetListeFields(typeof(T));

        /// <summary>
        /// Récupere la liste des champs donnés dans la Query String pour un type de modele
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetListeFields(Type type)
        {
            List<string> fields = this.GetListeFields(AttributeHandling.GetLabelProperty(type));

            if (fields != null) {
                //Récupere les vrais noms des propriétés
                List<PropertyInfo> fieldsModel = AttributeHandling.GetListeProperties(type, fields).ToList();
                fields = fieldsModel.Select(f => f.Name).ToList();
            }

            return fields;
        }

        /// <summary>
        /// Implémenté dans QueryServiceArchitecture
        /// </summary>
        public virtual List<string> GetListeFields(Type type, string relationPath) => throw new NotImplementedException();

        /// <summary>
        ///  Récupere la liste des champs json api dans la Query String pour un nom de modele json api
        /// </summary>
        /// <returns></returns>
        public List<string> GetListeFields(string nomType) => this._fields.Where(f => f.Key == nomType).Select(f => f.Value).FirstOrDefault();
        #endregion

        #region Utils
        private string Get(JToken source, string key, bool isRequired)
        {
            string value;

            if (source == null) {
                value = null;
            } else if (source[key]?.Type == JTokenType.Array) {
                value = source[key].First.ToString();
            } else {
                value = source[key]?.ToString();
            }

            if (isRequired && value == null) {
                throw new ArgumentException($"Query invalid|Filter {key} in query string is required");
            }

            return value;
        }

        private int? GetInt(JToken source, string key, bool isRequired = false)
        {
            string value = this.Get(source, key, isRequired);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            bool isInt = int.TryParse(value, out int valueInt);

            if (!isInt) {
                throw new ArgumentException("Query invalid", $"{key} must be a int");
            }

            return valueInt;
        }

        private bool? GetBoolean(JToken source, string key, bool isRequired = false)
        {
            string value = this.Get(source, key, isRequired);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            bool isBool = bool.TryParse(value, out bool valueBool);

            if (!isBool && value != null) {
                return value == "1";
            }

            return valueBool;
        }

        private DateTime? GetDate(JToken source, string key, bool isRequired)
        {
            string value = this.Get(source, key, isRequired);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            bool isDate = DateTime.TryParse(value, out DateTime valueDate);

            if (!isDate) {
                throw new ArgumentException($"Query invalid|{key} must be a date");
            }

            return valueDate;
        }

        private List<string> GetList(JToken source, string key, bool isRequired)
        {
            List<string> value = null;

            if (source != null) {
                if (source[key]?.Type == JTokenType.Array) {
                    value = source[key].Select(v => v.ToString())
                        .Where(v => !string.IsNullOrEmpty(v))
                        .Select(v => v.TrimStart(new char[] { ' ' }).TrimEnd(new char[] { ' ' })).ToList();
                } else if (source[key] != null) {
                    this._jsonapi.Error.Create("Error Syntax Json Query String", $"Element {key} must be an array", "Query String");
                }
            }

            if (isRequired && value == null) {
                throw new ArgumentException($"Query invalid|Element {key} in query string is required");
            }

            return value;
        }

        private List<decimal> GetListDecimal(JToken source, string key, bool isRequired)
        {
            List<string> value = this.GetList(source, key, isRequired);

            if (value != null) {
                try {
                    return value.Select(d => decimal.Parse(d)).ToList();
                } catch (FormatException) {
                    throw new ArgumentException($"Query invalid|Element {key} in query string is not decimal list");
                }
            } else {
                return new List<decimal>();
            }
        }

        private T GetJsonApi<T>(JToken source) where T : new()
        {
            var model = new T();

            if (source == null)
                return model;

            try {
                var type = new JProperty("type", AttributeHandling.GetLabelProperty(new T().GetType()));
                var attr = new JProperty("attributes", source);
                var jobject = new JObject {
                    type,
                    attr
                };

                return (T)new JsonApiReader(jobject.ToString(), model).GetModel(true);

            } catch (JsonApiException ex) {
                ex.Error.GetErrors().ForEach(e => this._jsonapi.Error.Create(e));

                throw new ArgumentException($"Query invalid|Element in query string is not in expected format");
            }
        }

        private JObject ParseQuery(string key)
        {
            if (this._query[key].Count > 0 && !string.IsNullOrWhiteSpace(this._query[key]) && this._query[key] != "null") {
                try {
                    var jo = JObject.Parse(this._query[key].FirstOrDefault());

                    return (jo.HasValues) ? jo : null;

                } catch (Newtonsoft.Json.JsonReaderException ex) {
                    this._jsonapi.Error.Create("Error Syntax Json Query String", ex.Message, ex.Path);

                    throw new ArgumentException($"Query invalid|Element in query string is not in expected format");
                }
            } else
                return null;
        }
        #endregion
    }
}