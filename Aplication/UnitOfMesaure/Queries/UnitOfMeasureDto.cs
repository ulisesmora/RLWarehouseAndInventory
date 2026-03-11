using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.UnitOfMesaure.Queries
{
    public class UnitOfMeasureDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public bool IsBaseUnit { get; set; }
        public decimal ConversionFactor { get; set; }
    }
}
