using JsonApi.Attributes;
using System.Collections.Generic;

namespace JsonApi.Tests.Models
{
    class Article
    {
        [IdJsonApi]
        public int IdJson { get; set; }
        public int Id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public bool online { get; set; }
        public PersonModel author { get; set; }
        public List<Comment> comments { get; set; }
        public List<string> codes { get; set; }
        [IgnoreJsonApi]
        public int propIgnore { get; set; }
    }
}
