using JsonApi.Attributes;
using System;

namespace JsonApiArchitecture.Attributes
{
    public class RessourceJsonApiAttribute : PropertyJsonApiAttribute
    {
        public Type DataSource { get; set; }
        public Type Repository { get; set; }
        public Type Criteres { get; set; }

        public RessourceJsonApiAttribute(string label, Type dataSource, Type repository, Type criteres) : this(dataSource, repository, criteres)
        {
            this.Label = label;
        }

        public RessourceJsonApiAttribute(Type dataSource, Type repository, Type criteres)
        {
            this.DataSource = dataSource;
            this.Repository = repository;
            this.Criteres = criteres;
        }

        public RessourceJsonApiAttribute(string label, Type dataSource, Type criteres)
        {
            this.Label = label;
            this.DataSource = dataSource;
            this.Criteres = criteres;
        }
    }
}
