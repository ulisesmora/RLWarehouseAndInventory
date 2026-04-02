using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public class Organization : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // Ej. "Empaques Nacionales S.A."
        public string? TaxId { get; set; } // RFC, CIF, NIT

        // Control del modelo Freemium
        public string SubscriptionTier { get; set; } = "Free"; // Free, Pro, Enterprise
        public int MaxAllowedLpns { get; set; } = 1000; // Límite de la capa gratuita

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Relación: Una organización tiene muchos usuarios
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
