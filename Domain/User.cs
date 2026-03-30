using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain
{
    public enum SystemRole
    {
        Admin,       // Acceso total al SaaS, facturación, creación de catálogos
        Supervisor,  // Ve métricas, hace ajustes, pero no configura el sistema
        Operator     // Solo App Móvil (Recibir, Acomodar, Extraer)
    }
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Contraseña encriptada

        public SystemRole Role { get; set; } = SystemRole.Operator;

        public Guid? RestrictedWarehouseId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
