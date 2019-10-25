using System;

namespace JsonApi.Attributes
{
    public class IgnoreJsonApiAttribute : Attribute
    {
        public bool IgnoreReader { get; set; }
        public bool IgnoreParser { get; set; }
    }
}
