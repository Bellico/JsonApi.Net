using System;
using System.ComponentModel;

namespace JsonApi.Attributes
{
    public class PropertyJsonApiAttribute : DisplayNameAttribute
    {
        private string _label;

        public string Label
        {
            get => this._label;
            set 
            {
                this._label = value;
                this.DisplayNameValue = value;
            }
        }
    
        public Type Type { get; set; }
        public Type Converter { get; set; }
    }
}
