using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.UnitOfMesaure.Commands;
using Inventory.Application.UnitOfMesaure.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UnitOfMeasuresController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitOfMeasuresController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/UnitOfMeasures?PageNumber=1&PageSize=10
        [HttpGet]
        public async Task<ActionResult<List<UnitOfMeasureDto>>> GetPaged([FromQuery] GetUnitsOfMeasureQuery query)
        {
            return await _mediator.Send(query);
        }

        // GET: api/UnitOfMeasures/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UnitOfMeasureDto>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUnitOfMeasureByIdQuery(id));
            return Ok(result);
        }

        // POST: api/UnitOfMeasures
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateUnitOfMeasureCommand command)
        {
            var id = await _mediator.Send(command);
            // Retorna 201 Created y la URL para consultar el recurso creado
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        // PUT: api/UnitOfMeasures/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateUnitOfMeasureCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            await _mediator.Send(command);
            return NoContent(); // 204 No Content (Estándar para Updates exitosos)
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteUnitOfMeasureCommand(id));
            return NoContent(); // 204 No Content es el estándar para un Delete exitoso
        }
    }
}
