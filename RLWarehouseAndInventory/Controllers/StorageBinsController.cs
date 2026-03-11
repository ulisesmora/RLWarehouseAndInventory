using Inventory.Application.StorageBins.Commands;
using Inventory.Application.StorageBins.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageBinsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StorageBinsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateStorageBinCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpGet("zone/{zoneId}")]
        public async Task<ActionResult<List<StorageBinDto>>> GetByZone(Guid zoneId)
        {
            return await _mediator.Send(new GetBinsByZoneQuery(zoneId));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateStorageBinCommand command)
        {
            if (id != command.Id) return BadRequest();
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("move-rack")]
        public async Task<ActionResult> MoveRack( MoveRackCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound(new { message = "No se encontraron bins para este Rack." });

            return Ok(new { message = "Rack movido exitosamente." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteStorageBinCommand(id));
            return NoContent();
        }
    }
}
