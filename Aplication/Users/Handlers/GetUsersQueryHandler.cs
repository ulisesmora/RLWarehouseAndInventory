using AutoMapper;
using Inventory.Application.Users.Queries;
using Inventory.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Handlers
{
    public class GetUsersQueryHandler(
    InventoryDbContext context,
    IMapper mapper) : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
    {
        public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await context.User
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return mapper.Map<IEnumerable<UserDto>>(users);
        }


    }

    public class GetUserByIdQueryHandler(
    InventoryDbContext context,
    IMapper mapper) : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await context.User
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"User con Id {request.Id} no encontrado.");

            return mapper.Map<UserDto>(user);
        }
    }
}
