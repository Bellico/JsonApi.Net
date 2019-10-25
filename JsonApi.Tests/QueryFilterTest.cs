using JsonApi.Helpers;
using JsonApi.Services;
using JsonApi.Tests.Models;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace JsonApi.Tests
{
    public class QueryFilterTest
    {
        [Fact]
        public void Case1()
        {
            var filterObject = new
            {
                id = 4,
            };

            JObject json = JObject.FromObject(filterObject);
            var query = new QueryService(json);

            Assert.Equal("4", query.Filter("id"));
            Assert.Equal(4, query.FilterInt("id"));
            Assert.Equal(new System.Tuple<int,int>(1,15), query.Page(15));
            Assert.Throws<ArgumentException>(() => query.FilterInt("require", true));
        }

        [Fact]
        public void Case2()
        {
            var filterObject = new
            {
                comment = new { id = 4}
            };

            JObject json = JObject.FromObject(filterObject);
            var query = new QueryFilterService(json);

            Assert.Equal(4, query.Filter<Comment>("comment").id);
        }

        [Fact]
        public void Case3()
        {
            var filterObject = new
            {
                comment = new { key = "value" },
            };

            JObject json = JObject.FromObject(filterObject);
            var query = new QueryFilterService(json);
            Comment comment = query.Filter<Comment>("comment");

            Assert.Equal("value", comment.key);
        }

        [Fact]
        public void Case4()
        {
            var filterObject = new
            {
                comment = new object[]
                {
                    new { key = "value"},
                    new { key = "value2"},
                }
            };

            JObject json = JObject.FromObject(filterObject);
            var query = new QueryFilterService(json);
            System.Collections.Generic.List<Comment> comment = query.FilterList<Comment>("comment");

            Assert.Equal(2, comment.Count);
            Assert.Equal("value", comment[0].key);
            Assert.Equal("value2", comment[1].key);
        }

        [Fact]
        public void Case5()
        {
            var filterObject = new
            {
                list = new object[] { "item1", "item2", "item3" }
            };

            JObject json = JObject.FromObject(filterObject);
            var query = new QueryFilterService(json);
            System.Collections.Generic.List<string> list = query.FilterList("list");

            Assert.Equal(3, list.Count);
            Assert.Equal("item1", list[0]);
            Assert.Equal("item2", list[1]);
        }


        [Fact]
        public void Case6()
        {
            var filterObject = new
            {
                id = 1,
                key = "key",
                body = "body"
            };

            JObject json = JObject.FromObject(filterObject);
            var query = new QueryFilterService(json);
            Comment comment = query.FilterModel<Comment>();

            Assert.Equal(1, comment.id);
            Assert.Equal("key", comment.key);
            Assert.Equal("body", comment.body);
        }
    }
}
