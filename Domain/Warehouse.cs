using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal? Capacity { get; set; }
        public bool? IsMain { get; set; }
        public ICollection<StockItem> StockItems { get; set; }
        public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
    }
}
