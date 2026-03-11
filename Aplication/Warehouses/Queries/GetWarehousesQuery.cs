using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Warehouses.Queries
{
    public record GetWarehousesQuery : IRequest<List<WarehouseDto>>;
}
