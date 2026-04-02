using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class Category : BaseTenantEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        // Auto-referencia para categorías padres e hijos (Jerarquía)
        public Guid? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; }
    }
}
