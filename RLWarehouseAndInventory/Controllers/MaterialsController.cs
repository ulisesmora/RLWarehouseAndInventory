using Inventory.Application.Materials.Commands;
using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.Materials.Queries;
using Inventory.Application.UnitOfMesaure.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MaterialsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedList<MaterialDto>>> GetMaterials([FromQuery] GetMaterialsWithPaginationQuery query)
        {
            return await _mediator.Send(query);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialDto>> GetMaterial(Guid id)
        {
            return await _mediator.Send(new GetMaterialByIdQuery(id));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateMaterialCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateMaterialCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del comando.");
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteMaterialCommand(id));
            return NoContent(); // 204 No Content es el estándar para un Delete exitoso
        }
    }
}
