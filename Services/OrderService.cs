using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderResponseDto> GetByIdAsync(int companyId, int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderPayments)
                .Include(o => o.OrderStatusHistories)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

            if (order == null)
                throw new KeyNotFoundException($"Order {id} not found");

            return MapToResponseDto(order);
        }

        public async Task<List<OrderResponseDto>> GetAllAsync(int companyId, OrderFilterDto filter)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.CompanyId == companyId);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(o => 
                    o.OrderNumber.Contains(filter.Search) ||
                    o.CustomerName.Contains(filter.Search) ||
                    o.CustomerEmail.Contains(filter.Search));
            }

            if (!string.IsNullOrEmpty(filter.OrderStatus))
                query = query.Where(o => o.OrderStatus == filter.OrderStatus);

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                query = query.Where(o => o.PaymentStatus == filter.PaymentStatus);

            // Ordenamiento
            query = filter.SortDescending 
                ? query.OrderByDescending(o => o.OrderDate) 
                : query.OrderBy(o => o.OrderDate);

            // Paginación
            var orders = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return orders.Select(MapToResponseDto).ToList();
        }

        public async Task<OrderResponseDto> CreateAsync(int companyId, CreateOrderDto dto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(dto.CustomerId);
                if (customer == null || customer.CompanyId != companyId)
                    throw new InvalidOperationException("Customer not found");

                var orderNumber = await GenerateOrderNumberAsync(companyId);

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CompanyId = companyId,
                    CustomerId = dto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = "Pending",
                    PaymentStatus = "Pending",
                    DeliveryStatus = "Pending",
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    CustomerEmail = customer.Email,
                    CustomerPhone = customer.Phone,
                    CustomerAvatar = customer.Avatar,
                    ShippingAddressId = dto.ShippingAddressId,
                    BillingAddressId = dto.BillingAddressId,
                    Notes = dto.Notes,
                    OrderSource = dto.OrderSource ?? "Web",
                    Currency = "USD",
                    DiscountAmount = dto.DiscountAmount ?? 0
                };

                // Agregar items
                decimal subTotal = 0;
                decimal taxTotal = 0;

                foreach (var itemDto in dto.Items)
                {
                    var product = await _context.Products
                        .Include(p => p.Variants)
                        .FirstOrDefaultAsync(p => p.Id == itemDto.ProductId && p.CompanyId == companyId);

                    if (product == null)
                        throw new InvalidOperationException($"Product {itemDto.ProductId} not found");

                    decimal unitPrice = product.BasePrice;
                    string productName = product.Name;

                    if (itemDto.ProductVariantId.HasValue)
                    {
                        var variant = product.Variants.FirstOrDefault(v => v.Id == itemDto.ProductVariantId.Value);
                        if (variant != null)
                        {
                            unitPrice = variant.Price;
                            productName = $"{product.Name} - {variant.Name}";
                        }
                    }

                    var itemTotal = unitPrice * itemDto.Quantity;
                    var itemTax = itemTotal * 0.18m;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        ProductVariantId = itemDto.ProductVariantId,
                        ProductName = productName,
                        ProductImage = product.Images?.FirstOrDefault(),
                        ProductSku = product.SKU,
                        Quantity = itemDto.Quantity,
                        UnitPrice = unitPrice,
                        TaxAmount = itemTax,
                        TotalPrice = itemTotal + itemTax,
                        Notes = itemDto.Notes
                    });

                    subTotal += itemTotal;
                    taxTotal += itemTax;
                }

                order.SubTotal = subTotal;
                order.TaxAmount = taxTotal;
                order.ShippingAmount = 10;
                order.TotalAmount = subTotal - order.DiscountAmount + taxTotal + order.ShippingAmount;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await AddStatusHistoryAsync(order.Id, "Order Placed", "OrderPlaced", 
                    $"Order #{order.OrderNumber} has been placed successfully");

                customer.TotalOrders++;
                customer.TotalSpent += order.TotalAmount;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {order.OrderNumber} created successfully");
                return await GetByIdAsync(companyId, order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                throw;
            }
        }

        public async Task<OrderResponseDto> UpdateAsync(int companyId, int id, UpdateOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.CompanyId != companyId)
                throw new KeyNotFoundException($"Order {id} not found");

            if (!string.IsNullOrEmpty(dto.OrderStatus))
            {
                order.OrderStatus = dto.OrderStatus;
                await AddStatusHistoryAsync(order.Id, dto.OrderStatus, dto.OrderStatus, 
                    $"Order status changed to {dto.OrderStatus}");
            }

            if (!string.IsNullOrEmpty(dto.PaymentStatus))
                order.PaymentStatus = dto.PaymentStatus;

            if (!string.IsNullOrEmpty(dto.DeliveryStatus))
                order.DeliveryStatus = dto.DeliveryStatus;

            if (!string.IsNullOrEmpty(dto.TrackingNumber))
                order.TrackingNumber = dto.TrackingNumber;

            if (!string.IsNullOrEmpty(dto.Notes))
                order.Notes = dto.Notes;

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(companyId, id);
        }

        public async Task<bool> DeleteAsync(int companyId, int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.CompanyId != companyId)
                return false;

            if (!order.CanDelete)
                throw new InvalidOperationException("This order cannot be deleted");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Order {order.OrderNumber} deleted");
            return true;
        }

        public async Task<OrderMetricsDto> GetMetricsAsync(int companyId)
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var todayUtc = DateTime.SpecifyKind(today, DateTimeKind.Utc);

            // Obtener todas las órdenes de la compañía para evitar múltiples queries con comparaciones de fecha
            var orders = await _context.Orders
                .Where(o => o.CompanyId == companyId)
                .Select(o => new
                {
                    o.OrderStatus,
                    o.PaymentStatus,
                    o.OrderDate,
                    o.TotalAmount
                })
                .ToListAsync();

            // Filtrar en memoria para evitar problemas con timezone
            var todayOrders = orders.Where(o => o.OrderDate.Date == today.Date);
            var thisMonthOrders = orders.Where(o => o.OrderDate >= startOfMonth);
            var paidOrders = orders.Where(o => o.PaymentStatus == "Paid");

            return new OrderMetricsDto
            {
                PendingPayment = orders.Count(o => o.PaymentStatus == "Pending"),
                Completed = orders.Count(o => o.OrderStatus == "Completed"),
                Refunded = orders.Count(o => o.PaymentStatus == "Refunded"),
                Failed = orders.Count(o => o.PaymentStatus == "Failed"),
                TodayOrders = todayOrders.Count(),
                ThisMonthOrders = thisMonthOrders.Count(),
                TotalRevenue = paidOrders.Sum(o => o.TotalAmount),
                AverageOrderValue = paidOrders.Any() ? paidOrders.Average(o => o.TotalAmount) : 0
            };
        }

        public async Task<OrderResponseDto> UpdatePaymentStatusAsync(int companyId, int id, UpdatePaymentStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.CompanyId != companyId)
                throw new KeyNotFoundException($"Order {id} not found");

            order.PaymentStatus = dto.PaymentStatus;

            if (!string.IsNullOrEmpty(dto.PaymentMethod))
                order.PaymentMethod = dto.PaymentMethod;

            if (!string.IsNullOrEmpty(dto.CardLast4))
                order.PaymentMethodLast4 = dto.CardLast4;

            if (dto.PaymentStatus == "Paid")
            {
                _context.OrderPayments.Add(new OrderPayment
                {
                    OrderId = order.Id,
                    TransactionId = dto.TransactionId,
                    Amount = order.TotalAmount,
                    Currency = order.Currency,
                    Status = "Completed",
                    PaymentDate = DateTime.UtcNow,
                    Notes = dto.Notes
                });
            }

            await AddStatusHistoryAsync(order.Id, $"Payment {dto.PaymentStatus}", "PaymentUpdate", dto.Notes);
            
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Payment status updated for order {order.OrderNumber}");
            return await GetByIdAsync(companyId, id);
        }

        public async Task<OrderResponseDto> UpdateShippingStatusAsync(int companyId, int id, UpdateShippingStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.CompanyId != companyId)
                throw new KeyNotFoundException($"Order {id} not found");

            order.DeliveryStatus = dto.DeliveryStatus;

            if (!string.IsNullOrEmpty(dto.TrackingNumber))
                order.TrackingNumber = dto.TrackingNumber;

            if (!string.IsNullOrEmpty(dto.TrackingUrl))
                order.TrackingUrl = dto.TrackingUrl;

            if (dto.EstimatedDeliveryDate.HasValue)
                order.EstimatedDeliveryDate = dto.EstimatedDeliveryDate;

            var statusType = dto.DeliveryStatus switch
            {
                "ReadyToPickup" => "PickupScheduled",
                "Dispatched" => "Dispatched",
                "OutForDelivery" => "DispatchedForDelivery",
                "Delivered" => "Delivered",
                _ => dto.DeliveryStatus
            };

            await AddStatusHistoryAsync(order.Id, dto.DeliveryStatus, statusType, dto.Notes, null);

            if (dto.DeliveryStatus == "Delivered")
            {
                order.ActualDeliveryDate = DateTime.UtcNow;
                order.OrderStatus = "Completed";
            }

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Shipping status updated for order {order.OrderNumber}");
            return await GetByIdAsync(companyId, id);
        }

        public async Task<OrderResponseDto> ProcessRefundAsync(int companyId, int id, ProcessRefundDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);
                
            if (order == null)
                throw new KeyNotFoundException($"Order {id} not found");

            if (!order.CanRefund)
                throw new InvalidOperationException("This order cannot be refunded");

            _context.OrderPayments.Add(new OrderPayment
            {
                OrderId = order.Id,
                Amount = -dto.RefundAmount,
                Currency = order.Currency,
                Status = "Refunded",
                RefundAmount = dto.RefundAmount,
                RefundReason = dto.RefundReason,
                RefundDate = DateTime.UtcNow,
                Notes = dto.Notes
            });

            order.PaymentStatus = dto.RefundAmount >= order.TotalAmount ? "Refunded" : "PartialRefund";
            
            await AddStatusHistoryAsync(order.Id, "Refund Processed", "RefundProcessed", 
                $"Refund of {dto.RefundAmount:C} - Reason: {dto.RefundReason}");

            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Refund processed for order {order.OrderNumber}");
            return await GetByIdAsync(companyId, id);
        }

        public async Task<bool> AddStatusHistoryAsync(int orderId, string status, string statusType, string? description = null, int? userId = null)
        {
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                Status = status,
                StatusType = statusType,
                Description = description,
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                IsCompleted = true
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateOrderNumberAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            var prefix = company?.OrderIdPrefix ?? "";
            var suffix = company?.OrderIdSuffix ?? "";
            
            var lastOrder = await _context.Orders
                .Where(o => o.CompanyId == companyId)
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();
            
            var nextNumber = (lastOrder?.Id ?? 0) + 10000;
            return $"{prefix}{nextNumber}{suffix}";
        }

        public async Task<bool> ValidateOrderForCompanyAsync(int companyId, int orderId)
        {
            return await _context.Orders.AnyAsync(o => o.Id == orderId && o.CompanyId == companyId);
        }

        public async Task<bool> CanCancelOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            return order?.CanCancel ?? false;
        }

        public async Task<bool> CanRefundOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            return order?.CanRefund ?? false;
        }

        private OrderResponseDto MapToResponseDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                FormattedOrderNumber = order.FormattedOrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,
                CustomerAvatar = order.CustomerAvatar,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                DeliveryStatus = order.DeliveryStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentMethodLast4 = order.PaymentMethodLast4,
                CountryCode = order.CountryCode,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                ShippingAmount = order.ShippingAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                TrackingNumber = order.TrackingNumber,
                TrackingUrl = order.TrackingUrl,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                ActualDeliveryDate = order.ActualDeliveryDate,
                Notes = order.Notes,
                ItemCount = order.ItemCount,
                CanCancel = order.CanCancel,
                CanRefund = order.CanRefund,
                CanEdit = order.CanEdit,
                CanDelete = order.CanDelete,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems?.Select(i => new OrderItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductName,
                    ProductImage = i.ProductImage,
                    ProductSku = i.ProductSku,
                    ProductAttributes = i.ProductAttributes,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount,
                    TaxAmount = i.TaxAmount,
                    TotalPrice = i.TotalPrice,
                    IsSelected = i.IsSelected,
                    Notes = i.Notes
                }).ToList() ?? new List<OrderItemResponseDto>(),
                Payments = order.OrderPayments?.Select(p => new OrderPaymentResponseDto
                {
                    Id = p.Id,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    CardType = p.CardType,
                    CardLast4 = p.CardLast4,
                    PaymentDate = p.PaymentDate,
                    RefundDate = p.RefundDate,
                    RefundAmount = p.RefundAmount,
                    RefundReason = p.RefundReason
                }).ToList() ?? new List<OrderPaymentResponseDto>(),
                StatusHistory = order.OrderStatusHistories?.Select(h => new OrderStatusHistoryDto
                {
                    Id = h.Id,
                    Status = h.Status,
                    StatusType = h.StatusType,
                    Description = h.Description,
                    Location = h.Location,
                    Timestamp = h.Timestamp,
                    EstimatedDate = h.EstimatedDate,
                    IsCompleted = h.IsCompleted,
                    UserName = h.User != null ? $"{h.User.FirstName} {h.User.LastName}" : null
                }).ToList() ?? new List<OrderStatusHistoryDto>(),
                ShippingAddress = order.ShippingAddress != null ? new AddressDto
                {
                    Id = order.ShippingAddress.Id,
                    Street = order.ShippingAddress.Street,
                    Street2 = order.ShippingAddress.Apartment,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    PostalCode = order.ShippingAddress.PostalCode,
                    Country = order.ShippingAddress.Country
                } : null,
                BillingAddress = order.BillingAddress != null ? new AddressDto
                {
                    Id = order.BillingAddress.Id,
                    Street = order.BillingAddress.Street,
                    Street2 = order.BillingAddress.Apartment,
                    City = order.BillingAddress.City,
                    State = order.BillingAddress.State,
                    PostalCode = order.BillingAddress.PostalCode,
                    Country = order.BillingAddress.Country
                } : null
            };
        }
    }
}