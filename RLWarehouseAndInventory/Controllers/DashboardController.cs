using Inventory.Application.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
            => _mediator = mediator;

        /// <summary>
        /// Retorna el resumen completo del dashboard (KPIs, alertas, gráficas).
        /// Todas las consultas se ejecutan en paralelo para minimizar latencia.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSummary()
            => Ok(await _mediator.Send(new GetDashboardSummaryQuery()));
    }
}
