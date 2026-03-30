using Inventory.Application.Lots.Commands;
using Inventory.Application.Lots.Queries;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.Materials.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LotsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateLotCommand command)
        {
            return await _mediator.Send(command);
        }

        // 1. Obtener lotes de un material (Para seleccionar cuál vender)
        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<List<LotDto>>> GetByMaterial(Guid materialId)
        {
            return await _mediator.Send(new GetActiveLotsByMaterialQuery(materialId));
        }

        // 2. Obtener alertas de caducidad (Dashboard)
        [HttpGet("expiring")]
        public async Task<ActionResult<List<LotDto>>> GetExpiring([FromQuery] int days = 30)
        {
            return await _mediator.Send(new GetExpiringLotsQuery(days));
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedList<LotDto>>> GetLots([FromQuery] GetLotsQuery query)
        {
            return await _mediator.Send(query);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateLotCommand command)
        {
            if (id != command.Id) return BadRequest();
            await _mediator.Send(command);
            return NoContent();
        }

       
    }
}
