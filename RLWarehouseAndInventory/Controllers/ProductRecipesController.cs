using Inventory.Application.ProductsRecipes.Commands;
using Inventory.Application.ProductsRecipes.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductRecipesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductRecipesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("finishedgood/{finishedGoodId}")]
        public async Task<ActionResult<ProductRecipeDto>> GetByFinishedGood(Guid finishedGoodId)
        {
            var recipe = await _mediator.Send(new GetRecipeByFinishedGoodQuery(finishedGoodId));

            if (recipe == null)
                return NotFound("No se encontró una receta de producción para este material.");

            return Ok(recipe);
        }

        // POST: api/productrecipes
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateProductRecipeCommand command)
        {
            return await _mediator.Send(command);
        }


    }
}
