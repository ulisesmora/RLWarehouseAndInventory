using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.StorageBins.Commands
{
    public record DeleteStorageBinCommand(Guid Id) : IRequest;
}
