using Inventory.Application.WorkOrders.Commands;
using Inventory.Application.WorkOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene el listado paginado de órdenes de trabajo (MRP).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetWorkOrdersWithPaginationQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        /// <summary>
        /// Obtiene el detalle de una orden, incluyendo las tareas de picking para el Gemelo 3D.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetWorkOrderByIdQuery(id));
            return result != null ? Ok(result) : NotFound();
        }

        /// <summary>
        /// Crea una nueva orden de trabajo e inmediatamente reserva stock (FIFO) generando tareas de picking.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkOrderCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>
        /// Actualiza metadatos de la orden (Fechas programadas, notas).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkOrderCommand command)
        {
            if (id != command.Id)
                return BadRequest(new { message = "El ID de la ruta no coincide con el comando." });

            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Cierra una orden de trabajo: registra consumos, genera lote de producto terminado y descuenta inventario.
        /// </summary>
        /// <summary>
        /// Cierra una orden de trabajo: descuenta consumos desde las PickTasks, genera el Lote
        /// del producto terminado listo para acomodarse con el flujo de Putaway existente.
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteWorkOrder(Guid id, [FromBody] CompleteWorkOrderRequest body)
        {
            try
            {
                var command = new CompleteWorkOrderCommand(
                    WorkOrderId:               id,
                    ActualFinishedGoodQuantity: body.ActualFinishedGoodQuantity,
                    LotNumber:                 body.LotNumber
                );

                var lotId = await _mediator.Send(command);
                return Ok(new
                {
                    message = "Orden completada. El lote está listo para acomodarse.",
                    lotId
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error interno al cerrar la orden." });
            }
        }

        /// <summary>
        /// Envía una orden de InProgress → QualityControl para revisión antes de cerrar.
        /// </summary>
        [HttpPost("{id}/quality-control")]
        public async Task<IActionResult> SendToQualityControl(Guid id)
        {
            try
            {
                await _mediator.Send(new SendToQualityControlCommand(id));
                return Ok(new { message = "Orden enviada a Control de Calidad." });
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception)                    { return StatusCode(500, new { message = "Error interno." }); }
        }

        /// <summary>
        /// Confirma una tarea de picking individual: valida el LPN escaneado, marca la tarea como Completada
        /// y si todas las tareas de la orden se completan, cambia la orden a 'InProgress'.
        /// </summary>
        [HttpPost("{workOrderId}/tasks/{taskId}/confirm")]
        public async Task<IActionResult> ConfirmPickTask(Guid workOrderId, Guid taskId, [FromBody] ConfirmPickTaskRequest body)
        {
            try
            {
                var command = new ConfirmPickTaskCommand(
                    WorkOrderId: workOrderId,
                    TaskId: taskId,
                    ScannedLpn: body.ScannedLpn,
                    PickedQuantity: body.PickedQuantity
                );

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error interno al confirmar la tarea de picking." });
            }
        }

        /// <summary>
        /// Cancela una orden de trabajo y libera el inventario reservado (Regresa el stock a 'disponible').
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteWorkOrderCommand(id));
            return NoContent();
        }
    }

    /// <summary>Body del endpoint ConfirmPickTask.</summary>
    public record ConfirmPickTaskRequest(string ScannedLpn, decimal PickedQuantity);

    /// <summary>Body del endpoint CompleteWorkOrder (simplificado).</summary>
    public record CompleteWorkOrderRequest(decimal ActualFinishedGoodQuantity, string? LotNumber);
}
