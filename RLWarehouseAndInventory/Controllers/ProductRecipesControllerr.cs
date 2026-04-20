using Inventory.Application.ProductRecipes.Commands;
using Inventory.Application.ProductRecipes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    // Opcional: [Authorize] si ya tienes tu JWT configurado para proteger estos endpoints
    public class ProductRecipesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductRecipesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene una lista paginada de recetas de producción.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetProductRecipesWithPaginationQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el detalle completo de una receta por su ID, incluyendo ingredientes y costos.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProductRecipeByIdQuery(id));

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Crea una nueva receta de producción.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRecipeCommand command)
        {
            // El validador (FluentValidation) actuará automáticamente antes de llegar aquí 
            // si tienes configurado el middleware de validación.
            var recipeId = await _mediator.Send(command);

            // Retornamos 201 Created con la ruta para consultar el nuevo recurso
            return CreatedAtAction(nameof(GetById), new { id = recipeId }, recipeId);
        }

        /// <summary>
        /// Actualiza una receta existente, reemplazando sus ingredientes y costos.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRecipeCommand command)
        {
            // Verificación de seguridad básica
            if (id != command.Id)
            {
                return BadRequest(new { message = "El ID de la ruta no coincide con el ID del cuerpo de la petición." });
            }

            await _mediator.Send(command);

            return NoContent(); // 204 No Content es el estándar para un PUT exitoso
        }

        /// <summary>
        /// Realiza un borrado lógico (Soft Delete) de la receta.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteProductRecipeCommand(id));

            return NoContent(); // 204 No Content
        }
    }
}
