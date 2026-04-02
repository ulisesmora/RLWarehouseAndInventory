using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Queries
{

    public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
    public record GetUsersQuery() : IRequest<IEnumerable<UserDto>>;
}
