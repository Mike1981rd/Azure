using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;

namespace WebsiteBuilderAPI.Services
{
    public interface IOrderService
    {
        // Operaciones CRUD básicas
        Task<OrderResponseDto> GetByIdAsync(int companyId, int id);
        Task<List<OrderResponseDto>> GetAllAsync(int companyId, OrderFilterDto filter);
        Task<OrderResponseDto> CreateAsync(int companyId, CreateOrderDto dto);
        Task<OrderResponseDto> UpdateAsync(int companyId, int id, UpdateOrderDto dto);
        Task<bool> DeleteAsync(int companyId, int id);
        
        // Métricas y estadísticas
        Task<OrderMetricsDto> GetMetricsAsync(int companyId);
        
        // Operaciones de estado
        Task<OrderResponseDto> UpdatePaymentStatusAsync(int companyId, int id, UpdatePaymentStatusDto dto);
        Task<OrderResponseDto> UpdateShippingStatusAsync(int companyId, int id, UpdateShippingStatusDto dto);
        Task<OrderResponseDto> ProcessRefundAsync(int companyId, int id, ProcessRefundDto dto);
        
        // Historial de estados
        Task<bool> AddStatusHistoryAsync(int orderId, string status, string statusType, string? description = null, int? userId = null);
        
        // Generación de número de orden
        Task<string> GenerateOrderNumberAsync(int companyId);
        
        // Validaciones
        Task<bool> ValidateOrderForCompanyAsync(int companyId, int orderId);
        Task<bool> CanCancelOrderAsync(int orderId);
        Task<bool> CanRefundOrderAsync(int orderId);
    }
}