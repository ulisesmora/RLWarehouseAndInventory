using Inventory.Application.Materials.Commons.Models;
using Inventory.Application.Categories.Commands;
using Inventory.Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/Categories?PageNumber=1&PageSize=10&SearchTerm=Tela
        [HttpGet]
        public async Task<ActionResult<PaginatedList<CategoryDto>>> GetPaged([FromQuery] GetCategoriesWithPaginationQuery query)
        {
            return await _mediator.Send(query);
        }

        // GET: api/Categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id));
            return Ok(result);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateCategoryCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        // PUT: api/Categories/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateCategoryCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            await _mediator.Send(command);
            return NoContent();
        }

      
        

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteCategoryCommand(id));
            return NoContent(); // 204 No Content es el estándar para un Delete exitoso
        }
    }
}
