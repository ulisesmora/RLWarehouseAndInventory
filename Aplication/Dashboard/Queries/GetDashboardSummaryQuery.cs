using MediatR;

namespace Inventory.Application.Dashboard.Queries
{
    public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
}
