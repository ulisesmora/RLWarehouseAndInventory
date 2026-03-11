using Inventory.Application.SupplierMaterials.Commands;
using Inventory.Application.SupplierMaterials.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierMaterialsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupplierMaterialsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("material/{materialId}")]
        public async Task<ActionResult<List<SupplierMaterialDto>>> GetByMaterial(Guid materialId)
        {
            return await _mediator.Send(new GetSuppliersByMaterialQuery(materialId));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateSupplierMaterialCommand command)
        {
            return await _mediator.Send(command);
        }

        // 🔥 NUEVO: Endpoint para Actualizar
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateSupplierMaterialCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del comando.");
            }

            await _mediator.Send(command);
            return NoContent();
        }

        // 🔥 NUEVO: Endpoint para Eliminar
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteSupplierMaterialCommand(id));
            return NoContent();
        }


    }
}
