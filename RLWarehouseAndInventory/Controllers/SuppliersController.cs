using Inventory.Application.Suppliers.Commands;
using Inventory.Application.Suppliers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuppliersController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<ActionResult<List<SupplierDto>>> Get()
        {
            return await _mediator.Send(new GetSuppliersQuery());
        }

        // Si necesitas GetById, puedes crear el Query similar a como lo hicimos en Zones

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateSupplierCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateSupplierCommand command)
        {
            if (id != command.Id) return BadRequest();
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteSupplierCommand(id));
            return NoContent();
        }
    }
}
