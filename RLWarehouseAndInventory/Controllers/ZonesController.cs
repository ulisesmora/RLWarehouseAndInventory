using Inventory.Application.Zones.Commands;
using Inventory.Application.Zones.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZonesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ZonesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateZoneCommand command)
        {
            return await _mediator.Send(command);
        }

        // GET api/zones/warehouse/{warehouseId}
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<List<ZoneDetailDto>>> GetByWarehouse(Guid warehouseId)
        {
            return await _mediator.Send(new GetZonesByWarehouseQuery(warehouseId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ZoneDetailDto>> GetById(Guid id)
        {
            return await _mediator.Send(new GetZoneByIdQuery(id));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateZoneCommand command)
        {
            if (id != command.Id) return BadRequest();
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteZoneCommand(id));
            return NoContent();
        }


    }
}
