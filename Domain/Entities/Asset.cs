using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Domain.Entities
{
    [XmlRoot("asset")]  
    public class Asset
    {
        
        [XmlAttribute("id")] //specifying xml attribute so that xml serializer doesn't throw unknow attribute error as it excepts attributes
        public int Id { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9 ]{1,30}$")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [JsonIgnore]
        [XmlIgnore] 
        public int? ParentId { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public Asset? Parent { get; set; }

        [XmlElement("asset")] //include case insensitiviy
        public List<Asset> Children { get; set; } = new List<Asset>();

        [Required]
        [XmlIgnore]
        public List<Signal> Signals { get; set; } = new List<Signal>();
    }
}   
