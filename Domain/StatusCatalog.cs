using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class StatusCatalog : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsBlocker   { get; set; }
        public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    }
}
