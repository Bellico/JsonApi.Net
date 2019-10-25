using JsonApi.Core;
using JsonApi.Exceptions;
using JsonApi.Tests.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace JsonApi.Tests
{
    public class JsonApiTests
    {
        public JsonApiParser GetParser(object model) => new JsonApiParser(model);

        [Fact]
        public void Case0()
        {
            object a = this.GetParser("string").GetJson();
            Assert.Equal(((JObject)a)["data"].ToString(), "string");
        }

        [Fact]
        public void Case1()
        {
            var person = new PersonModel() { Id = 4, Name = "Nom", Password = "Password", online = true, date = DateTime.Today };
            var json = (JObject)this.GetParser(person).GetJson();

            Assert.Equal(json["data"]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"]["id"].ToString(), "4");

            Assert.Equal(json["data"]["attributes"]["Name"].ToString(), "Nom");
            Assert.Equal(json["data"]["attributes"]["Password"].ToString(), "Password");
            Assert.Equal(json["data"]["attributes"]["online"].ToString(), "True");
            Assert.Equal(json["data"]["attributes"]["date"].ToString(), DateTime.Today.ToString("s"));

            var personModel = new PersonModel();

            var personResult = (PersonModel)new JsonApiReader(json.ToString(), personModel).GetModel();

            Assert.Equal(personResult.Id, 4);
            Assert.Equal(personResult.Name, "Nom");
            Assert.Equal(personResult.Password, "Password");
            Assert.Equal(personResult.online, true);
            Assert.Equal(personResult.date, DateTime.Today);
        }

        [Fact]
        public void Case2()
        {
            var person = new PersonModel() { Id = 4, Name = "Nom", Password = "Password", online = true, date = DateTime.Today };
            person.Parent = new PersonModel() { Id = 5, Name = "Parent Name", Password = "pass parent", online = true, date = DateTime.Today.AddDays(2) };
            JsonApiParser parser = this.GetParser(person);
            parser.AddIncludeQuery("Parent");
            var json = (JObject)parser.GetJson();


            Assert.Equal(json["data"]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"]["id"].ToString(), "4");

            Assert.Equal(json["data"]["attributes"]["Name"].ToString(), "Nom");
            Assert.Equal(json["data"]["attributes"]["Password"].ToString(), "Password");
            Assert.Equal(json["data"]["attributes"]["online"].ToString(), "True");
            Assert.Equal(json["data"]["attributes"]["date"].ToString(), DateTime.Today.ToString("s"));

            Assert.Equal(json["data"]["relationships"]["Parent"]["data"]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"]["relationships"]["Parent"]["data"]["id"].ToString(), "5");

            Assert.Equal(json["included"].Type.ToString(), "Array");
            Assert.Equal(json["included"][0]["type"].ToString(), "personmodel");
            Assert.Equal(json["included"][0]["id"].ToString(), "5");
            Assert.Equal(json["included"][0]["attributes"]["Password"].ToString(), "pass parent");
            Assert.Equal(json["included"][0]["attributes"]["Name"].ToString(), "Parent Name");
            Assert.Equal(json["included"][0]["attributes"]["online"].ToString(), "True");
            Assert.Equal(json["included"][0]["attributes"]["date"].ToString(), DateTime.Today.AddDays(2).ToString("s"));

            var personModel = new PersonModel();
            var personResult = (PersonModel)new JsonApiReader(json.ToString(), personModel).GetModel();

            Assert.Equal(personResult.Id, 4);
            Assert.Equal(personResult.Name, "Nom");
            Assert.Equal(personResult.Password, "Password");
            Assert.Equal(personResult.online, true);
            Assert.Equal(personResult.date, DateTime.Today);
            Assert.Equal(personResult.Parent.Id, 5);
        }

        [Fact]
        public void Case3()
        {

            var comments = new List<Comment>()
            {
                new Comment() {id = 1, body = "comment 1" },
                new Comment() {id = 2, body = "comment 2" }
            };

            JsonApiParser parser = this.GetParser(comments);
            var json = (JObject)parser.GetJson();


            Assert.Equal(json["data"][0]["type"].ToString(), "comment");
            Assert.Equal(json["data"][0]["id"].ToString(), "1");
            Assert.Equal(json["data"][0]["attributes"]["body"].ToString(), "comment 1");
            Assert.Equal(json["data"][1]["id"].ToString(), "2");
            Assert.Equal(json["data"][1]["attributes"]["body"].ToString(), "comment 2");

            var list = new List<Comment>();
            var model = (List<Comment>)new JsonApiReader(json.ToString(), list).GetModel();

            Assert.Equal(model[0].id, 1);
            Assert.Equal(model[0].body, "comment 1");
            Assert.Equal(model[1].id, 2);
            Assert.Equal(model[1].body, "comment 2");
        }

        [Fact]
        public void Case4()
        {
            var auteur = new PersonModel() { Id = 4, Name = "Auteur" };
            var comments = new List<Comment>()
            {
                new Comment() {id = 1, body = "comment 1" },
                new Comment() {id = 2, body = "comment 2" }
            };
            var article = new Article() { title = "titre article", body = "body artcicle", author = auteur, comments = comments };

            JsonApiParser parser = this.GetParser(article);
            parser.AddIncludeQuery("*");
            var json = (JObject)parser.GetJson();

            Assert.Equal(json["data"]["type"].ToString(), "article");
            Assert.Equal(json["data"]["id"].ToString(), "0_0");
            Assert.Equal(json["data"]["attributes"]["title"].ToString(), "titre article");
            Assert.Equal(json["data"]["attributes"]["body"].ToString(), "body artcicle");
            Assert.Equal(json["data"]["attributes"]["online"].ToString(), "False");

            Assert.Equal(json["data"]["relationships"]["author"]["data"]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"]["relationships"]["author"]["data"]["id"].ToString(), "4");

            Assert.Equal(json["data"]["relationships"]["comments"]["data"].Type.ToString(), "Array");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][0]["type"].ToString(), "comment");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][0]["id"].ToString(), "1");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][1]["type"].ToString(), "comment");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][1]["id"].ToString(), "2");

            var articlemodel = new Article();
            var model = (Article)new JsonApiReader(json.ToString(), articlemodel).GetModel();

            Assert.Equal(model.Id, 0);
            Assert.Equal(model.title, "titre article");
            Assert.Equal(model.body, "body artcicle");
            Assert.Equal(model.author.Id, 4);
            Assert.Equal(model.comments.Count, 2);
            Assert.Equal(model.comments[0].id, 1);
            Assert.Equal(model.comments[1].id, 2);

        }

        [Fact]
        public void Case5()
        {
            var auteur = new PersonModel() { Id = 4, Name = "Auteur" };
            var comments = new List<Comment>()
            {
                new Comment() {id = 1, body = "comment 1" },
                new Comment() {id = 2, body = "comment 2" }
            };

            var articles = new List<Article>()
            {
                new Article() { title = "titre article 1", body = "body artcicle 1", author = auteur, comments = comments },
                new Article() { title = "titre article 2", body = "body artcicle 2", author = auteur, comments = comments }
            };

            JsonApiParser parser = this.GetParser(articles);
            parser.AddIncludeQuery("*");
            var json = (JObject)parser.GetJson();


            Assert.Equal(json["data"].Type.ToString(), "Array");

            //article 1
            Assert.Equal(json["data"][0]["type"].ToString(), "article");
            Assert.Equal(json["data"][0]["id"].ToString(), "0_0");
            Assert.Equal(json["data"][0]["attributes"]["title"].ToString(), "titre article 1");
            Assert.Equal(json["data"][0]["attributes"]["body"].ToString(), "body artcicle 1");

            Assert.Equal(json["data"][0]["relationships"]["author"]["data"]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"][0]["relationships"]["author"]["data"]["id"].ToString(), "4");

            Assert.Equal(json["data"][0]["relationships"]["comments"]["data"].Type.ToString(), "Array");
            Assert.Throws<ArgumentOutOfRangeException>(() => json["data"][0]["relationships"]["comments"]["data"][3]);

            Assert.Equal(json["data"][0]["relationships"]["comments"]["data"][0]["type"].ToString(), "comment");
            Assert.Equal(json["data"][0]["relationships"]["comments"]["data"][0]["id"].ToString(), "1");
            Assert.Equal(json["data"][0]["relationships"]["comments"]["data"][1]["type"].ToString(), "comment");
            Assert.Equal(json["data"][0]["relationships"]["comments"]["data"][1]["id"].ToString(), "2");

            //article 2
            Assert.Equal(json["data"][1]["type"].ToString(), "article");
            Assert.Equal(json["data"][1]["id"].ToString(), "0_0");
            Assert.Equal(json["data"][1]["attributes"]["title"].ToString(), "titre article 2");
            Assert.Equal(json["data"][1]["attributes"]["body"].ToString(), "body artcicle 2");

            Assert.Equal(json["data"][1]["relationships"]["author"]["data"]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"][1]["relationships"]["author"]["data"]["id"].ToString(), "4");

            Assert.Equal(json["data"][1]["relationships"]["comments"]["data"].Type.ToString(), "Array");
            Assert.Equal(json["data"][1]["relationships"]["comments"]["data"][0]["type"].ToString(), "comment");
            Assert.Equal(json["data"][1]["relationships"]["comments"]["data"][0]["id"].ToString(), "1");
            Assert.Equal(json["data"][1]["relationships"]["comments"]["data"][1]["type"].ToString(), "comment");
            Assert.Equal(json["data"][1]["relationships"]["comments"]["data"][1]["id"].ToString(), "2");


            var listmodel = new List<Article>();
            var model = (List<Article>)new JsonApiReader(json.ToString(), listmodel).GetModel();

            //article 1
            Assert.Equal(model[0].Id, 0);
            Assert.Equal(model[0].title, "titre article 1");
            Assert.Equal(model[0].body, "body artcicle 1");
            Assert.Equal(model[0].online, false);
            Assert.Equal(model[0].author.Id, 4);
            Assert.Equal(model[0].comments.Count, 2);
            Assert.Equal(model[0].comments[0].id, 1);
            Assert.Equal(model[0].comments[1].id, 2);

            //article 2
            Assert.Equal(model[1].Id, 0);
            Assert.Equal(model[1].title, "titre article 2");
            Assert.Equal(model[1].body, "body artcicle 2");
            Assert.Equal(model[1].online, false);
            Assert.Equal(model[1].author.Id, 4);
            Assert.Equal(model[1].comments.Count, 2);
            Assert.Equal(model[1].comments[0].id, 1);
            Assert.Equal(model[1].comments[1].id, 2);

        }

        [Fact]
        public void Case6()
        {
            var u = new PersonModel() { Id = 3, Name = "Test 1", UserName = "NameTest 1" };
            u.Articles = new List<Article>()
            {
                new Article() {IdJson = 1, body = "body article", title = "title" },
                new Article() {IdJson = 2, body = "body article", title = "title" }
            };

            var listperson = new List<PersonModel>()
            {
                u,
                new PersonModel() {Id = 4, Name = "Test 2", UserName = "NameTest 2", Parent = u }
            };

            JsonApiParser parser = this.GetParser(listperson);
            parser.AddIncludeQuery("*");
            var json = (JObject)parser.GetJson();

            Assert.Equal(json["data"].Type.ToString(), "Array");
            Assert.Equal(json["data"][0]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"][0]["id"].ToString(), "3");
            Assert.Equal(json["data"][0]["attributes"]["UserName"].ToString(), "NameTest 1");

            Assert.Equal(json["data"][0]["relationships"]["Articles"]["data"].Type.ToString(), "Array");
            Assert.Equal(json["data"][0]["relationships"]["Articles"]["data"][0]["type"], "article");
            Assert.Equal(json["data"][0]["relationships"]["Articles"]["data"][0]["id"], "1_0");
            Assert.Equal(json["data"][0]["relationships"]["Articles"]["data"][1]["type"], "article");
            Assert.Equal(json["data"][0]["relationships"]["Articles"]["data"][1]["id"], "2_0");

            Assert.Equal(json["data"][1]["type"].ToString(), "personmodel");
            Assert.Equal(json["data"][1]["id"].ToString(), "4");
            Assert.Equal(json["data"][1]["relationships"]["Parent"]["data"].Type.ToString(), "Object");
            Assert.Equal(json["data"][1]["relationships"]["Parent"]["data"]["type"], "personmodel");
            Assert.Equal(json["data"][1]["relationships"]["Parent"]["data"]["id"], "3");

            var listmodel = new List<PersonModel>();
            var model = (List<PersonModel>)new JsonApiReader(json.ToString(), listmodel).GetModel();

            Assert.Equal(model[0].Id, 3);
            Assert.Equal(model[0].UserName, "NameTest 1");
            Assert.Equal(model[0].Articles[0].IdJson, 1);
            Assert.Equal(model[0].Articles[1].IdJson, 2);

            Assert.Equal(model[1].Id, 4);
            Assert.Equal(model[1].UserName, "NameTest 2");
            Assert.Equal(model[1].Parent.Id, 3);
        }

        [Fact]
        public void Case7()
        {
            var a = new Article() { IdJson = 1, title = "titre", body = "body" };
            a.author = new PersonModel() { Id = 1, Name = "Franck", Parent = new PersonModel() { Id = 55, Name = "Parent Included" } };
            a.comments = new List<Comment>()
            {
                new Comment() { id = 1, body = "i like it" },
                new Comment() { id = 2, body = "yes i do" }
            };

            JsonApiParser parser = this.GetParser(a);
            parser.AddIncludeQuery("author");
            parser.AddIncludeQuery("comments");
            var json = (JObject)parser.GetJson();

            Assert.Equal(json["data"]["id"].ToString(), "1_0");
            Assert.Equal(json["data"]["type"].ToString(), "article");
            Assert.Equal(json["data"]["attributes"]["title"].ToString(), "titre");
            Assert.Equal(json["data"]["attributes"]["body"].ToString(), "body");

            Assert.Equal(json["data"]["relationships"]["author"].Type.ToString(), "Object");
            Assert.Equal(json["data"]["relationships"]["author"]["data"]["id"], "1");
            Assert.Equal(json["data"]["relationships"]["author"]["data"]["type"], "personmodel");

            Assert.Equal(json["data"]["relationships"]["comments"]["data"].Type.ToString(), "Array");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][0]["id"], "1");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][0]["type"], "comment");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][1]["id"], "2");
            Assert.Equal(json["data"]["relationships"]["comments"]["data"][1]["type"], "comment");

            Assert.Equal(json["included"].Type.ToString(), "Array");
            Assert.Equal(json["included"][0]["id"], "1");
            Assert.Equal(json["included"][0]["type"], "personmodel");
            Assert.Equal(json["included"][0]["attributes"]["Name"], "Franck");

            Assert.Equal(json["included"][1]["type"], "comment");
            Assert.Equal(json["included"][1]["id"], "1");
            Assert.Equal(json["included"][1]["attributes"]["body"], "i like it");

            Assert.Equal(json["included"][2]["type"], "comment");
            Assert.Equal(json["included"][2]["id"], "2");
            Assert.Equal(json["included"][2]["attributes"]["body"], "yes i do");

            Assert.Throws<ArgumentOutOfRangeException>(() => json["included"][3]);

            var articlemodel = new Article();
            var model = (Article)new JsonApiReader(json.ToString(), articlemodel).GetModel();

            Assert.Equal(model.IdJson, 1);
            Assert.Equal(model.title, "titre");
            Assert.Equal(model.body, "body");
            Assert.Equal(model.author.Id, 1);
            Assert.Equal(model.comments[0].id, 1);
            Assert.Equal(model.comments[1].id, 2);
        }

        [Fact]
        public void Case8()
        {
            var a = new Article() { IdJson = 1, title = "titre", body = "body" };
            JsonApiParser parser = this.GetParser(a);
            parser.AddMeta("metaString", "test");
            parser.AddMeta("metaInt", 1);
            parser.AddMeta("metaList", new List<string>() { "chaine1", "chaine2", "chaine3" });
            parser.AddMeta("article", a);
            parser.AddMeta("listarticle", new List<Article>() { a, a });

            var json = (JObject)parser.GetJson();

            Assert.Equal(json["meta"]["metaString"].ToString(), "test");
            Assert.Equal(json["meta"]["metaInt"].ToString(), "1");
            Assert.Equal(json["meta"]["metaList"].Type.ToString(), "Array");
            Assert.Equal(json["meta"]["metaList"][0].ToString(), "chaine1");
            Assert.Equal(json["meta"]["article"].Type.ToString(), "Object");
            Assert.Equal(json["meta"]["article"]["type"].ToString(), "article");
            Assert.Equal(json["meta"]["listarticle"].Type.ToString(), "Array");
            Assert.Equal(json["meta"]["listarticle"][0]["type"].ToString(), "article");
            Assert.Equal(json["meta"]["listarticle"][1]["type"].ToString(), "article");
        }

        [Fact]
        public void Case9()
        {
            var dictionnary = new Dictionary<string, string> {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            JsonApiParser parser = this.GetParser(dictionnary);
            parser.AddMeta("dico", dictionnary);

            var json = (JObject)parser.GetJson();
            Assert.Equal(json["data"].Type.ToString(), "Object");
            Assert.Equal(json["data"]["attributes"]["key1"].ToString(), "value1");
            Assert.Equal(json["meta"]["dico"]["attributes"]["key1"].ToString(), "value1");
        }

        [Fact]
        public void Case10()
        {
            var dictionnary = new Dictionary<string, string> {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            var model = new List<Dictionary<string, string>>
            {
                dictionnary,dictionnary
            };

            JsonApiParser parser = this.GetParser(model);

            var json = (JObject)parser.GetJson();
            Assert.Equal(json["data"].Type.ToString(), "Array");
            Assert.Equal(json["data"][0]["attributes"]["key1"].ToString(), "value1");
            Assert.Equal(json["data"][1]["attributes"]["key2"].ToString(), "value2");
        }

        [Fact]
        public void Case11()
        {
            var a = new Article() { codes = new List<string> { "un", "deux", "trois" }, propIgnore = 1 };
            JsonApiParser parser = this.GetParser(a);
            var json = (JObject)parser.GetJson();

            Assert.Equal(json["data"]["attributes"]["codes"].Type.ToString(), "Array");
            Assert.Equal(json["data"]["attributes"]["codes"][0], "un");
            Assert.Equal(json["data"]["attributes"]["codes"][1], "deux");
            Assert.Equal(json["data"]["attributes"]["codes"][2], "trois");
            Assert.Throws<NullReferenceException>(() => json["data"]["attributes"]["propIgnore"].ToString());

            var articlemodel = new Article();
            var model = (Article)new JsonApiReader(json.ToString(), articlemodel).GetModel();

            Assert.Equal(model.codes.Count, 3);
        }

        [Fact]
        public void Case12()
        {
            var a = new Biblio() { identidiant = 4, nom = "biliotheque", int_article = new Article { Id = 4, body = "body" }, article_comment = new Article { body = "convertible2" } };
            a.list_person = new PersonModel { Name = "Franck", Password = "password" };
            JsonApiParser parser = this.GetParser(a);
            parser.AddIncludeQuery("article_comment");
            var json = (JObject)parser.GetJson();
            
            Assert.Equal(json["data"]["id"], 4);
            Assert.Equal(json["data"]["type"], "bibi");
            Assert.Equal(json["data"]["attributes"]["name"], "biliotheque");
            Assert.Equal(json["data"]["relationships"]["article_comment"]["data"]["type"], "article");
            Assert.Equal(json["included"][0]["type"], "article");
            Assert.Equal(json["included"][0]["attributes"]["body"], "convertible2");

            var biblioModel = new Biblio();
            var model = (Biblio)new JsonApiReader(json.ToString(), biblioModel).GetModel();

            Assert.Equal(model.identidiant, 4);
            Assert.Equal(model.nom, "biliotheque");
        }

        [Fact]
        public void Case13()
        {
            var a = new Article() { IdJson = 1, title = "titre", body = "body" };
            a.author = new PersonModel() { Id = 1, Name = "Franck", Parent = new PersonModel() { Id = 55, Name = "Parent Included" } };
            JsonApiParser parser = this.GetParser(a);
            var json = (JObject)parser.GetJson();
            var articlemodel = new Article();

            json["data"]["attributes"] = (JToken.Parse(@"{ ""bad_property"" : ""value""}"));
            Assert.Throws<JsonApiException>(() => (Article)new JsonApiReader(json.ToString(), articlemodel).GetModel());

            json = (JObject)parser.GetJson();
            json["data"]["relationships"] = (JToken.Parse(@"{ ""bad_relationship"" : {} }"));
            Assert.Throws<InvalidPropertyException>(() => (Article)new JsonApiReader(json.ToString(), articlemodel).GetModel());

            json = (JObject)parser.GetJson();
            json["data"]["type"] = "faketype";
            Assert.Throws<InvalidPropertyException>(() => (Article)new JsonApiReader(json.ToString(), articlemodel).GetModel());
        }
    }
}
