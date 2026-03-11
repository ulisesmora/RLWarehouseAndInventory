using Inventory.Application.StatusCatalogs.Commands;
using Inventory.Application.StatusCatalogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusCatalog : ControllerBase
    {
        private readonly IMediator _mediator;

        public StatusCatalog(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult<List<StatusCatalogDto>>> Get()
        {
            return await _mediator.Send(new GetStatusCatalogsQuery());
        }

        // POST: api/statuscatalogs
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateStatusCatalogCommand command)
        {
            return await _mediator.Send(command);
        }

        // PUT: api/statuscatalogs/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateStatusCatalogCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del comando.");
            }

            await _mediator.Send(command);
            return NoContent();
        }
    }
}
