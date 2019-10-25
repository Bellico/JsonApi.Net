using JsonApi.Core;
using JsonApi.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApi.Services
{
    public class JsonApiService
    {
        private JsonApiParser _parser;
        private JsonApiError _error;

        public Dictionary<string, Type> Resolvers { get; }

        public JsonApiService()
        {
            this.Resolvers = new Dictionary<string, Type>();
        }

        public JsonApiParser Parser
        {
            get {
                if (this._parser == null)
                    this._parser = new JsonApiParser();

                return this._parser;
            }
        }

        public JsonApiError Error
        {
            get {
                if (this._error == null)
                    this._error = new JsonApiError();

                return this._error;
            }
        }

        public JsonApiService AddResolver(string name, Type typeResolver)
        {
            if (!typeResolver.GetInterfaces().Any(x => x.GetGenericTypeDefinition() == typeof(IResolver<>)))
                  throw new ArgumentException("Le type de resolver n'implémente pas IResolver<T>");

            this.Resolvers.Add(name, typeResolver);

            return this;
        }
    }
}
