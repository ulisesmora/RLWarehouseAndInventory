using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.StockMovements.Commands;
using Inventory.Application.StockMovements.Queries;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
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

        [HttpGet]
        public async Task<ActionResult<PaginatedList<StockItemDto>>> GetStockItems([FromQuery] GetStockItemsQuery query)
        {
            return await _mediator.Send(query);
        }

        [HttpGet("{stockItemId}/history")]
        public async Task<ActionResult<List<StockMovementDto>>> GetStockItemHistory(Guid stockItemId)
        {
            return await _mediator.Send(new GetStockItemHistoryQuery(stockItemId));
        }

        // --- COMMANDS (Transacciones) ---

        [HttpPost("receive")]
        public async Task<ActionResult<bool>> Receive(ReceiveStockCommand command)
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

        [HttpPost("bulk-transfer")]
        public async Task<ActionResult<BulkTransferResult>> BulkTransfer([FromBody] BulkTransferCommand command)
        {
            // El handler procesará todo, aplicará el ACID y nos devolverá el reporte
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
