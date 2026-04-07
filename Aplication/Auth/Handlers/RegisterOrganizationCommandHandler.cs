using Inventory.Application.Auth.Commands;
using Inventory.Application.Auth.Interfaces;
using Inventory.Domain;
using Inventory.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Auth.Handlers
{
    public class RegisterOrganizationCommandHandler : IRequestHandler<RegisterOrganizationCommand, AuthResponses>
    {
        private readonly InventoryDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RegisterOrganizationCommandHandler(InventoryDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponses> Handle(RegisterOrganizationCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar si el correo ya existe globalmente
            if (await _context.User.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email, cancellationToken))
            {
                throw new Exception("El correo ya está registrado."); // Aquí podrías usar una Custom Exception
            }

            // 2. Crear la Organización
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.OrganizationName,
                SubscriptionTier = "Free"
            };

            // 3. Crear el Usuario Admin asociado
            var user = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id, // 🔥 Aquí unimos al usuario con su Tenant
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = SystemRole.Admin
            };

            _context.Organization.Add(organization);
            _context.User.Add(user);

            await _context.SaveChangesAsync(cancellationToken);

            // 4. Generar el Token
            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponses(token, user.FullName, user.Role.ToString(), user.OrganizationId);
        }
    }
}
