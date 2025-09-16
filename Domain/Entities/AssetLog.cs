using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class AssetLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public string? Asset {get; set;}

        public string? Signal {get; set;}

        public string Action { get; set; }

        public DateTime LogTime { get; set; } = DateTime.UtcNow;

    }
}
