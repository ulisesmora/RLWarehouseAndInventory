using Inventory.Application.StockMovements.Commands;
using Inventory.Application.StockMovements.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StocksController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<List<StockItemDto>>> GetStockByMaterial(Guid materialId)
        {
            return await _mediator.Send(new GetStockByMaterialQuery(materialId));
        }

        [HttpGet("kardex/{materialId}")]
        public async Task<ActionResult<List<StockMovementDto>>> GetKardex(
        [FromQuery] Guid? materialId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? lotId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
        {
            var query = new GetKardexQuery(materialId, warehouseId, lotId, startDate, endDate);
            return await _mediator.Send(query);
        }

        // --- COMMANDS (Transacciones) ---

        [HttpPost("receive")]
        public async Task<ActionResult<Guid>> Receive(ReceiveStockCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("issue")]
        public async Task<ActionResult<Guid>> Issue(IssueStockCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult> Transfer(TransferStockCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("adjust")]
        public async Task<ActionResult> Adjust(AdjustStockCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
