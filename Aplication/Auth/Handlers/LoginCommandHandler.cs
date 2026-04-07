using Inventory.Application.Auth.Commands;
using Inventory.Application.Auth.Interfaces;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;

namespace Inventory.Application.Auth.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponses>
    {
        private readonly InventoryDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(InventoryDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponses> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.User.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales incorrectas.");
            }

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponses(token, user.FullName, user.Role.ToString(), user.OrganizationId);
        }
    }
}
