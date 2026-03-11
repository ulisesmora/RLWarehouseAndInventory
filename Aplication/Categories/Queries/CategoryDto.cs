using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.Queries
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; } // Para mostrar el nombre del padre
    }
}
