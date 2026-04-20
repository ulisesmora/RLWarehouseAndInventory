using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.WorkOrders.Queries
{
    public class WorkOrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string RecipeName { get; set; } = string.Empty;
        public string FinishedGoodName { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal ProducedQuantity { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<ProductionPickTaskDto> PickTasks { get; set; } = new();
    }
}
