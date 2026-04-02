using AutoMapper;
using Inventory.Application.Users.Commands;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Handlers
{
    public class CreateUserCommandHandler(
    InventoryDbContext context,
    IMapper mapper,
    ICurrentUserService currentUserService) : IRequestHandler<CreateUserCommand, Guid>
    {
        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Validación de correo duplicado
            if (await context.User.AnyAsync(u => u.Email == request.Email, cancellationToken))
                throw new Exception("El correo electrónico ya está registrado.");

            var entity = mapper.Map<User>(request);

            // Hashear la contraseña (ejemplo con BCrypt)
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Multi-Tenant: Asignar el usuario a la organización del creador
            var tenantId = currentUserService.GetTenantId()
                ?? throw new UnauthorizedAccessException("Tenant no identificado.");
            entity.OrganizationId = tenantId;

            context.User.Add(entity);
            await context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
