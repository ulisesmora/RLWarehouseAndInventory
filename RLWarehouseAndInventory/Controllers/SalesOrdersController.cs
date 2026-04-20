using Inventory.Application.SalesOrders.Commands;
using Inventory.Application.SalesOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SalesOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene el listado paginado de pedidos de venta.
        /// Soporta filtros por SearchTerm (número, cliente, referencia), Status y Channel.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetSalesOrdersWithPaginationQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        /// <summary>
        /// Obtiene el detalle de un pedido de venta con sus líneas y tareas de picking.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetSalesOrderByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo pedido de venta e inmediatamente reserva stock (FEFO)
        /// generando tareas de picking outbound.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesOrderCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error interno al crear el pedido." });
            }
        }

        /// <summary>
        /// Confirma una tarea de picking outbound: valida el LPN escaneado,
        /// actualiza la cantidad recogida y avanza el estado del pedido.
        /// </summary>
        [HttpPost("{salesOrderId}/tasks/{taskId}/confirm")]
        public async Task<IActionResult> ConfirmPickTask(
            Guid salesOrderId,
            Guid taskId,
            [FromBody] ConfirmOutboundPickTaskRequest body)
        {
            try
            {
                var command = new ConfirmOutboundPickTaskCommand(
                    SalesOrderId:   salesOrderId,
                    TaskId:         taskId,
                    ScannedLpn:     body.ScannedLpn,
                    PickedQuantity: body.PickedQuantity
                );

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception)                    { return StatusCode(500, new { message = "Error interno al confirmar la tarea." }); }
        }

        /// <summary>
        /// Embarca el pedido: crea movimientos de inventario tipo SalesShipment,
        /// descuenta QuantityOnHand y libera AllocatedQuantity. Status → Shipped.
        /// </summary>
        [HttpPost("{id}/ship")]
        public async Task<IActionResult> Ship(Guid id, [FromBody] ShipSalesOrderRequest body)
        {
            try
            {
                var command = new ShipSalesOrderCommand(
                    SalesOrderId:   id,
                    TrackingNumber: body.TrackingNumber,
                    CarrierName:    body.CarrierName,
                    Notes:          body.Notes
                );

                await _mediator.Send(command);
                return Ok(new { message = "Pedido embarcado correctamente. Stock actualizado." });
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception)                    { return StatusCode(500, new { message = "Error interno al embarcar el pedido." }); }
        }

        /// <summary>
        /// Cancela un pedido y libera el stock reservado (AllocatedQuantity → stock libre).
        /// No es posible cancelar pedidos ya embarcados o entregados.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSalesOrderRequest? body)
        {
            try
            {
                await _mediator.Send(new CancelSalesOrderCommand(id, body?.Reason));
                return Ok(new { message = "Pedido cancelado. Inventario reservado liberado." });
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception)                    { return StatusCode(500, new { message = "Error interno al cancelar el pedido." }); }
        }
    }

    /// <summary>Body de ConfirmPickTask outbound.</summary>
    public record ConfirmOutboundPickTaskRequest(string ScannedLpn, decimal PickedQuantity);

    /// <summary>Body del endpoint Ship.</summary>
    public record ShipSalesOrderRequest(string? TrackingNumber, string? CarrierName, string? Notes);

    /// <summary>Body del endpoint Cancel.</summary>
    public record CancelSalesOrderRequest(string? Reason);
}
