using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class HierarchyVersion
    {
        public int Id   { get; set; }

        public DateTime EditedTime { get; set; } = DateTime.UtcNow;

        public string SnapshotJson  { get; set; } = string.Empty;

        //What action lead to versoning of Hierarchy    
        public string Action { get; set; }




    }
}
