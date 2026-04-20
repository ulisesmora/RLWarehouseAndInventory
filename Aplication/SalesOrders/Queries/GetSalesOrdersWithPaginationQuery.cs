using Inventory.Application.Materials.Commons.Models;
using MediatR;

namespace Inventory.Application.SalesOrders.Queries
{
    public record GetSalesOrdersWithPaginationQuery : IRequest<PaginatedList<SalesOrderDto>>
    {
        public int     PageNumber  { get; init; } = 1;
        public int     PageSize    { get; init; } = 20;
        public string? SearchTerm { get; init; }
        public string? Status     { get; init; }   // Filtrar por estado
        public string? Channel    { get; init; }   // Filtrar por canal
    }
}
