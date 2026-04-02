namespace Domain
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Por defecto hoy
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } // Soft Delete

        // Opcional: Para saber QUIÉN hizo el cambio (Auditoría completa)
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public interface ITenantEntity
    {
        Guid OrganizationId { get; set; }
    }

    public abstract class BaseTenantEntity : BaseEntity, ITenantEntity
    {
        public Guid OrganizationId { get; set; }
    }
}
