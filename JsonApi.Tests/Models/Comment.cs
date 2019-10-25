using JsonApi.Attributes;
using JsonApi.Interface;
using System.Collections.Generic;

namespace JsonApi.Tests.Models
{
    class Comment : IQueryRead
    {
        [IdJsonApi]
        public int id { get; set; }
        public string body { get; set; }
        public string key { get; set; }
        public List<string> values { get; set; }


        public void ReadQuery(IQueryService queryService)
        {
            throw new System.NotImplementedException();
        }
    }
}
