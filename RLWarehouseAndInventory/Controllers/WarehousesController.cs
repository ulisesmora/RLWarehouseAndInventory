using Inventory.Application.Warehouses.Commands;
using Inventory.Application.Warehouses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WarehousesController : ControllerBase
    {

        private readonly IMediator _mediator;
        public WarehousesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<ActionResult<List<WarehouseDto>>> Get()
        {
            return await this._mediator.Send(new GetWarehousesQuery());
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateWarehouseCommand command)
        {
            return await this._mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateWarehouseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await this._mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await this._mediator.Send(new DeleteWarehouseCommand(id));
            return NoContent();
        }
    }
}
