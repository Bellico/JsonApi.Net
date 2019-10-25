using JsonApi.Attributes;

namespace JsonApi.Tests.Models
{
    [PropertyJsonApi(Label = "bibi")]
    class Biblio
    {
        [IdJsonApi]
        public decimal identidiant { get; set; }
        [PropertyJsonApi(Label = "name")]
        public string nom { get; set; }
        public Article int_article { get; set; }
        public Article article_comment { get; set; }
        public PersonModel list_person { get; set; }
    }
}