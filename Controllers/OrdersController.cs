using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public partial class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;
        private readonly ICompanyService _companyService;
        private readonly IWebHostEnvironment _environment;

        public OrdersController(
            IOrderService orderService, 
            ILogger<OrdersController> logger,
            ICompanyService companyService,
            IWebHostEnvironment environment)
        {
            _orderService = orderService;
            _logger = logger;
            _companyService = companyService;
            _environment = environment;
        }

        /// <summary>
        /// Obtiene todas las órdenes con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDto>>> GetAll([FromQuery] OrderFilterDto filter)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var orders = await _orderService.GetAllAsync(companyId, filter);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, new { error = "Error al obtener las órdenes", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una orden por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetById(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var order = await _orderService.GetByIdAsync(companyId, id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order {id}");
                return StatusCode(500, new { error = "Error al obtener la orden", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las métricas del dashboard
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<OrderMetricsDto>> GetMetrics()
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var metrics = await _orderService.GetMetricsAsync(companyId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order metrics");
                return StatusCode(500, new { error = "Error al obtener las métricas", details = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva orden
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Datos inválidos", details = ModelState });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var order = await _orderService.CreateAsync(companyId, dto);
                
                _logger.LogInformation($"Order {order.Id} created successfully");
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { error = "Error al crear la orden", details = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una orden existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderResponseDto>> Update(int id, [FromBody] UpdateOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Datos inválidos", details = ModelState });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var order = await _orderService.UpdateAsync(companyId, id, dto);
                
                _logger.LogInformation($"Order {id} updated successfully");
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order {id}");
                return StatusCode(500, new { error = "Error al actualizar la orden", details = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el estado de pago de una orden
        /// </summary>
        [HttpPut("{id}/payment-status")]
        public async Task<ActionResult<OrderResponseDto>> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Datos inválidos", details = ModelState });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var order = await _orderService.UpdatePaymentStatusAsync(companyId, id, dto);
                
                _logger.LogInformation($"Payment status updated for order {id} to {dto.PaymentStatus}");
                return Ok(new { 
                    success = true, 
                    message = $"Estado de pago actualizado a {dto.PaymentStatus}",
                    data = order 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating payment status for order {id}");
                return StatusCode(500, new { error = "Error al actualizar el estado de pago", details = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el estado de envío de una orden
        /// </summary>
        [HttpPut("{id}/shipping-status")]
        public async Task<ActionResult<OrderResponseDto>> UpdateShippingStatus(int id, [FromBody] UpdateShippingStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Datos inválidos", details = ModelState });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var order = await _orderService.UpdateShippingStatusAsync(companyId, id, dto);
                
                _logger.LogInformation($"Shipping status updated for order {id} to {dto.DeliveryStatus}");
                return Ok(new { 
                    success = true, 
                    message = $"Estado de envío actualizado a {dto.DeliveryStatus}",
                    data = order 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating shipping status for order {id}");
                return StatusCode(500, new { error = "Error al actualizar el estado de envío", details = ex.Message });
            }
        }

        /// <summary>
        /// Procesa un reembolso para una orden
        /// </summary>
        [HttpPost("{id}/refund")]
        public async Task<ActionResult<OrderResponseDto>> ProcessRefund(int id, [FromBody] ProcessRefundDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Datos inválidos", details = ModelState });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var order = await _orderService.ProcessRefundAsync(companyId, id, dto);
                
                _logger.LogInformation($"Refund processed for order {id}: ${dto.RefundAmount}");
                return Ok(new { 
                    success = true, 
                    message = $"Reembolso de ${dto.RefundAmount} procesado exitosamente",
                    data = order 
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for order {id}");
                return StatusCode(500, new { error = "Error al procesar el reembolso", details = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una orden
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _orderService.DeleteAsync(companyId, id);
                
                if (!result)
                {
                    return NotFound(new { error = $"Orden {id} no encontrada" });
                }

                _logger.LogInformation($"Order {id} deleted successfully");
                return Ok(new { 
                    success = true, 
                    message = "Orden eliminada exitosamente" 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order {id}");
                return StatusCode(500, new { error = "Error al eliminar la orden", details = ex.Message });
            }
        }
    }
}