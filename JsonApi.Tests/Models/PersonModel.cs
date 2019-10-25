using JsonApi.Attributes;
using System;
using System.Collections.Generic;

namespace JsonApi.Tests.Models
{
    class PersonModel
    {
        [IdJsonApi]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool online { get; set; }
        public DateTime date { get; set; }
        public PersonModel Parent { get; set; }
        public List<Article> Articles { get; set; }
    }
}
