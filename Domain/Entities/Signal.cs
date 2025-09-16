using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Signal
    {

        public int Id { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9 ]{1,30}$")]
        public string Name { get; set; }
        public string ValueType { get; set; }
        public string? Description { get; set; }
        public int AssetId { get; set; }

        [JsonIgnore]
        public Asset Asset { get; set; }

    }
}
