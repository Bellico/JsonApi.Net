using System;

namespace JsonApiArchitecture.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ControllerJsonApiAttribute : Attribute
    {
        public string Route { get; }
        public bool ReadOnly { get; }

        public ControllerJsonApiAttribute(string route, bool readOnly = false)
        {
            this.Route = route;
            this.ReadOnly = readOnly;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ControllerRelationJsonApiAttribute : Attribute
    {
        public bool ReadOnly { get; set; }
        public bool None { get; set; }
    }
}
