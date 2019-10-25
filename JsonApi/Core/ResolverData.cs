using JsonApi.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApi.Core
{
    public class ResolverData
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _listeResolvers;
        private readonly Dictionary<string, object> _listeResolversInstance = new Dictionary<string, object>();

        public ResolverData(IServiceProvider serviceProvider, Dictionary<string, Type> listeResolvers)
        {
            this._serviceProvider = serviceProvider;
            this._listeResolvers = listeResolvers;
        }

        public object ResolveParser(string nameResolver, object data)
        {
            return this.Resolve(nameResolver, data, nameof(IResolver<object>.ResolveParser));
        }

        public object ResolveReader(string nameResolver, object data)
        {
            return this.Resolve(nameResolver, data, nameof(IResolver<object>.ResolveReader));
        }

        private object Resolve(string nameResolver, object data, string methodeName)
        {
            Type typeResolver = this._listeResolvers[nameResolver];

            object resolver = this.GetInstanceResolver(nameResolver, typeResolver);

            return typeResolver.GetMethod(methodeName).Invoke(resolver, new object[] { data });
        }

        private object GetInstanceResolver(string nameResolver, Type typeResolver)
        {
            if (!this._listeResolversInstance.Any(r => r.Key == nameResolver)) {
                object[] parameters = typeResolver.GetConstructors()[0].GetParameters().Select(p => this._serviceProvider.GetService(p.ParameterType)).ToArray();
                this._listeResolversInstance.Add(nameResolver, Activator.CreateInstance(typeResolver, parameters));
            }

            return this._listeResolversInstance[nameResolver];
        }
    }
}
