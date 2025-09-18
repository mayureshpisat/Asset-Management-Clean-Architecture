using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AssetInfo
    {
        public int Id { get; set; }
        
        public int AssetId { get; set; }

        public Asset Asset { get; set; }

        public int Temperature { get; set; }

        public int Power { get; set; }
    }
}
