using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IReservationReceiptService _receiptService;
        private readonly INotificationService _notificationService;
        private readonly IWhatsAppServiceFactory _whatsAppFactory;

        public ReservationService(
            ApplicationDbContext context,
            IEmailService emailService,
            IReservationReceiptService receiptService,
            INotificationService notificationService,
            IWhatsAppServiceFactory whatsAppFactory)
        {
            _context = context;
            _emailService = emailService;
            _receiptService = receiptService;
            _notificationService = notificationService;
            _whatsAppFactory = whatsAppFactory;
        }

        public async Task<List<ReservationListDto>> GetReservationsByCompanyAsync(int companyId, string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .Where(r => r.CompanyId == companyId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            if (startDate.HasValue)
                query = query.Where(r => r.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.CheckOutDate <= endDate.Value);

            var reservations = await query
                .OrderByDescending(r => r.CheckInDate)
                .Select(r => new ReservationListDto
                {
                    Id = r.Id,
                    GuestName = r.Customer != null ? r.Customer.FullName : "Guest",
                    GuestEmail = r.Customer != null ? r.Customer.Email : "",
                    GuestAvatar = r.Customer != null ? r.Customer.Avatar : null,
                    RoomName = r.Room != null ? r.Room.Name : "Room",
                    RoomType = r.Room != null ? r.Room.RoomType : "",
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status,
                    NumberOfNights = r.NumberOfNights
                })
                .ToListAsync();

            return reservations;
        }

        public async Task<ReservationDetailsDto?> GetReservationByIdAsync(int id, int companyId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                    .ThenInclude(c => c.Addresses)
                .Include(r => r.Room)
                .Include(r => r.BillingAddress)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (reservation == null)
                return null;

            var defaultAddress = reservation.Customer.Addresses.FirstOrDefault(a => a.IsDefault);
            var billingAddress = reservation.BillingAddress ?? defaultAddress;

            return new ReservationDetailsDto
            {
                Id = reservation.Id,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt,
                GuestInfo = new GuestInfoDto
                {
                    FullName = reservation.Customer.FullName,
                    Email = reservation.Customer.Email,
                    Phone = reservation.Customer.Phone ?? "",
                    Address = defaultAddress != null ? $"{defaultAddress.Street}, {defaultAddress.City}" : "",
                    Country = reservation.Customer.Country,
                    Avatar = reservation.Customer.Avatar
                },
                BillingInfo = new BillingInfoDto
                {
                    Address = reservation.CustomBillingAddress ?? billingAddress?.Street,
                    City = reservation.CustomBillingCity ?? billingAddress?.City,
                    State = reservation.CustomBillingState ?? billingAddress?.State,
                    PostalCode = reservation.CustomBillingPostalCode ?? billingAddress?.PostalCode,
                    Country = reservation.CustomBillingCountry ?? billingAddress?.Country,
                    IsDefaultAddress = reservation.BillingAddressId != null
                },
                ReservationInfo = new ReservationInfoDto
                {
                    RoomName = reservation.Room.Name,
                    RoomType = reservation.Room.RoomType,
                    NumberOfGuests = reservation.NumberOfGuests,
                    CheckInDate = reservation.CheckInDate,
                    CheckOutDate = reservation.CheckOutDate,
                    CheckInTime = reservation.CheckInTime.ToString(@"hh\:mm"),
                    CheckOutTime = reservation.CheckOutTime.ToString(@"hh\:mm"),
                    NumberOfNights = reservation.NumberOfNights,
                    SpecialRequests = reservation.SpecialRequests,
                    InternalNotes = reservation.InternalNotes
                },
                PaymentSummary = new PaymentSummaryDto
                {
                    RoomRate = reservation.RoomRate,
                    NumberOfNights = reservation.NumberOfNights,
                    TotalAmount = reservation.TotalAmount,
                    TotalPaid = reservation.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount),
                    Balance = reservation.TotalAmount - reservation.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount)
                },
                Payments = reservation.Payments.Select(p => new ReservationPaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId,
                    Notes = p.Notes
                }).ToList()
            };
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(int companyId, CreateReservationDto dto, int? userId = null)
        {
            try
            {
                // Validar disponibilidad de habitación
                var isAvailable = await CheckRoomAvailabilityAsync(dto.RoomId, dto.CheckInDate, dto.CheckOutDate);
                if (!isAvailable)
                {
                    return new ReservationResponseDto
                    {
                        Success = false,
                        Message = "La habitación no está disponible para las fechas seleccionadas"
                    };
                }

                // Obtener la habitación para el precio
                var room = await _context.Rooms.FindAsync(dto.RoomId);
                if (room == null || room.CompanyId != companyId)
                {
                    return new ReservationResponseDto
                    {
                        Success = false,
                        Message = "Habitación no encontrada"
                    };
                }

                // Validar que el cliente existe
                var customer = await _context.Customers.FindAsync(dto.CustomerId);
                if (customer == null || customer.CompanyId != companyId)
                {
                    return new ReservationResponseDto
                    {
                        Success = false,
                        Message = "Cliente no encontrado"
                    };
                }

                // Ensure dates are properly converted to UTC
                var checkInDate = dto.CheckInDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dto.CheckInDate, DateTimeKind.Utc)
                    : dto.CheckInDate.ToUniversalTime();
                    
                var checkOutDate = dto.CheckOutDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dto.CheckOutDate, DateTimeKind.Utc)
                    : dto.CheckOutDate.ToUniversalTime();

                var reservation = new Reservation
                {
                    CompanyId = companyId,
                    CustomerId = dto.CustomerId,
                    RoomId = dto.RoomId,
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    NumberOfGuests = dto.NumberOfGuests,
                    Status = "Pending",
                    RoomRate = room.BasePrice,
                    SpecialRequests = dto.SpecialRequests,
                    InternalNotes = dto.InternalNotes,
                    BillingAddressId = dto.BillingAddressId,
                    CustomBillingAddress = dto.CustomBillingAddress,
                    CustomBillingCity = dto.CustomBillingCity,
                    CustomBillingState = dto.CustomBillingState,
                    CustomBillingPostalCode = dto.CustomBillingPostalCode,
                    CustomBillingCountry = dto.CustomBillingCountry,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // Calcular noches y total
                reservation.CalculateTotal();

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                var details = await GetReservationByIdAsync(reservation.Id, companyId);

                return new ReservationResponseDto
                {
                    Id = reservation.Id,
                    Success = true,
                    Message = $"Reservación #{reservation.Id} creada exitosamente",
                    Data = details
                };
            }
            catch (Exception ex)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = $"Error al crear la reservación: {ex.Message}"
                };
            }
        }

        public async Task<ReservationResponseDto> UpdateReservationAsync(int id, int companyId, UpdateReservationDto dto)
        {
            try
            {
                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

                if (reservation == null)
                {
                    return new ReservationResponseDto
                    {
                        Success = false,
                        Message = "Reservación no encontrada"
                    };
                }

                // Validar disponibilidad si cambian las fechas
                if ((dto.CheckInDate.HasValue || dto.CheckOutDate.HasValue || dto.RoomId.HasValue))
                {
                    var checkIn = dto.CheckInDate ?? reservation.CheckInDate;
                    var checkOut = dto.CheckOutDate ?? reservation.CheckOutDate;
                    var roomId = dto.RoomId ?? reservation.RoomId;

                    var isAvailable = await CheckRoomAvailabilityAsync(roomId, checkIn, checkOut, id);
                    if (!isAvailable)
                    {
                        return new ReservationResponseDto
                        {
                            Success = false,
                            Message = "La habitación no está disponible para las nuevas fechas"
                        };
                    }
                }

                // Actualizar campos
                if (dto.CustomerId.HasValue)
                    reservation.CustomerId = dto.CustomerId.Value;
                
                if (dto.RoomId.HasValue)
                {
                    reservation.RoomId = dto.RoomId.Value;
                    var room = await _context.Rooms.FindAsync(dto.RoomId.Value);
                    if (room != null)
                        reservation.RoomRate = room.BasePrice;
                }

                if (dto.CheckInDate.HasValue)
                {
                    reservation.CheckInDate = dto.CheckInDate.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(dto.CheckInDate.Value, DateTimeKind.Utc)
                        : dto.CheckInDate.Value.ToUniversalTime();
                }
                
                if (dto.CheckOutDate.HasValue)
                {
                    reservation.CheckOutDate = dto.CheckOutDate.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(dto.CheckOutDate.Value, DateTimeKind.Utc)
                        : dto.CheckOutDate.Value.ToUniversalTime();
                }
                
                if (dto.NumberOfGuests.HasValue)
                    reservation.NumberOfGuests = dto.NumberOfGuests.Value;
                
                if (!string.IsNullOrEmpty(dto.Status))
                    reservation.Status = dto.Status;
                
                if (dto.SpecialRequests != null)
                    reservation.SpecialRequests = dto.SpecialRequests;
                
                if (dto.InternalNotes != null)
                    reservation.InternalNotes = dto.InternalNotes;

                // Actualizar facturación
                if (dto.BillingAddressId.HasValue)
                    reservation.BillingAddressId = dto.BillingAddressId.Value;
                
                if (dto.CustomBillingAddress != null)
                    reservation.CustomBillingAddress = dto.CustomBillingAddress;
                
                if (dto.CustomBillingCity != null)
                    reservation.CustomBillingCity = dto.CustomBillingCity;
                
                if (dto.CustomBillingState != null)
                    reservation.CustomBillingState = dto.CustomBillingState;
                
                if (dto.CustomBillingPostalCode != null)
                    reservation.CustomBillingPostalCode = dto.CustomBillingPostalCode;
                
                if (dto.CustomBillingCountry != null)
                    reservation.CustomBillingCountry = dto.CustomBillingCountry;

                reservation.UpdatedAt = DateTime.UtcNow;
                
                // Recalcular si cambiaron las fechas
                if (dto.CheckInDate.HasValue || dto.CheckOutDate.HasValue)
                    reservation.CalculateTotal();

                await _context.SaveChangesAsync();

                var details = await GetReservationByIdAsync(reservation.Id, companyId);

                return new ReservationResponseDto
                {
                    Id = reservation.Id,
                    Success = true,
                    Message = "Reservación actualizada exitosamente",
                    Data = details
                };
            }
            catch (Exception ex)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = $"Error al actualizar la reservación: {ex.Message}"
                };
            }
        }

        public async Task<ReservationResponseDto> DeleteReservationAsync(int id, int companyId)
        {
            try
            {
                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

                if (reservation == null)
                {
                    return new ReservationResponseDto
                    {
                        Success = false,
                        Message = "Reservación no encontrada"
                    };
                }

                if (reservation.Status == "CheckedIn")
                {
                    return new ReservationResponseDto
                    {
                        Success = false,
                        Message = "No se puede eliminar una reservación con check-in realizado"
                    };
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();

                return new ReservationResponseDto
                {
                    Id = id,
                    Success = true,
                    Message = "Reservación eliminada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = $"Error al eliminar la reservación: {ex.Message}"
                };
            }
        }

        public async Task<ReservationResponseDto> ConfirmReservationAsync(int id, int companyId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (reservation == null)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "Reservación no encontrada"
                };
            }

            if (reservation.Status != "Pending")
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "Solo se pueden confirmar reservaciones pendientes"
                };
            }

            reservation.Status = "Confirmed";
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ReservationResponseDto
            {
                Id = id,
                Success = true,
                Message = "Reservación confirmada exitosamente"
            };
        }

        public async Task<ReservationResponseDto> CheckInAsync(int id, int companyId, int? userId = null)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (reservation == null)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "Reservación no encontrada"
                };
            }

            if (reservation.Status != "Confirmed")
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "Solo se puede hacer check-in de reservaciones confirmadas"
                };
            }

            reservation.Status = "CheckedIn";
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ReservationResponseDto
            {
                Id = id,
                Success = true,
                Message = "Check-in realizado exitosamente"
            };
        }

        public async Task<ReservationResponseDto> CheckOutAsync(int id, int companyId, int? userId = null)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (reservation == null)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "Reservación no encontrada"
                };
            }

            if (reservation.Status != "CheckedIn")
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "Solo se puede hacer check-out de reservaciones con check-in"
                };
            }

            // Verificar si hay balance pendiente
            var totalPaid = reservation.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            if (totalPaid < reservation.TotalAmount)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = $"Hay un balance pendiente de ${reservation.TotalAmount - totalPaid:N2}"
                };
            }

            reservation.Status = "CheckedOut";
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ReservationResponseDto
            {
                Id = id,
                Success = true,
                Message = "Check-out realizado exitosamente"
            };
        }

        public async Task<ReservationResponseDto> CancelReservationAsync(int id, int companyId, string reason)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (reservation == null)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "Reservación no encontrada"
                };
            }

            if (reservation.Status == "CheckedIn" || reservation.Status == "CheckedOut")
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = "No se puede cancelar una reservación con check-in o check-out"
                };
            }

            reservation.Status = "Cancelled";
            reservation.InternalNotes = $"{reservation.InternalNotes}\nCancelada: {reason}";
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ReservationResponseDto
            {
                Id = id,
                Success = true,
                Message = "Reservación cancelada exitosamente"
            };
        }

        public async Task<ReservationResponseDto> AddPaymentAsync(int reservationId, int companyId, CreatePaymentDto dto, int? userId = null)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Payments)
                    .FirstOrDefaultAsync(r => r.Id == reservationId && r.CompanyId == companyId);

                if (reservation == null)
                {
                    return new ReservationResponseDto
                    {
                        Success = false,
                        Message = "Reservación no encontrada"
                    };
                }

                var payment = new ReservationPayment
                {
                    ReservationId = reservationId,
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    Status = "Completed",
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = dto.TransactionId,
                    Notes = dto.Notes,
                    ProcessedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ReservationPayments.Add(payment);
                await _context.SaveChangesAsync();

                // Side-effects: send receipt email with PDF, WhatsApp notify, and app notification
                try
                {
                    var full = await _context.Reservations
                        .Include(r => r.Customer)
                        .Include(r => r.Room)
                        .Include(r => r.Company)
                        .FirstOrDefaultAsync(r => r.Id == reservationId && r.CompanyId == companyId);
                    
                    if (full != null)
                    {
                        // Send email to customer with professional HTML template
                        if (!string.IsNullOrEmpty(full.Customer.Email))
                        {
                            var pdf = await _receiptService.GenerateReceiptPdfAsync(reservationId, companyId);
                            var attachment = new EmailAttachment
                            {
                                FileName = $"Reserva_{reservationId:D6}.pdf",
                                ContentType = "application/pdf",
                                Content = pdf
                            };
                            var subject = $"Confirmación de Reservación #{reservationId:D6}";
                            
                            // Use professional HTML template
                            var body = EmailTemplates.GenerateReservationConfirmationHtml(
                                full, full.Customer, full.Room, full.Company, payment);
                            
                            await _emailService.SendEmailAsync(full.Customer.Email, subject, body, null, new[] { attachment });
                        }

                        // Send email to admin with payment notification
                        var adminEmail = full.Company?.ContactEmail ?? full.Company?.SenderEmail;
                        if (!string.IsNullOrWhiteSpace(adminEmail))
                        {
                            var adminSubject = $"💰 Nuevo Pago Recibido - Reservación #{reservationId:D6}";
                            var adminBody = EmailTemplates.GenerateAdminPaymentNotificationHtml(
                                full, full.Customer, full.Room, full.Company, payment);
                            await _emailService.SendEmailAsync(adminEmail, adminSubject, adminBody);
                        }

                        // WhatsApp notification to business phone with improved format
                        try
                        {
                            var adminPhone = full.Company?.PhoneNumber;
                            if (!string.IsNullOrWhiteSpace(adminPhone))
                            {
                                var wa = _whatsAppFactory.GetService();
                                var msg = $@"💳 *PAGO CONFIRMADO*
Reservación: #{reservationId:D6}
Cliente: {full.Customer?.FullName ?? "N/A"}
Habitación: {full.Room?.Name ?? "N/A"}
Check-in: {full.CheckInDate:dd/MM/yyyy}
Check-out: {full.CheckOutDate:dd/MM/yyyy}
Noches: {full.NumberOfNights}
Huéspedes: {full.NumberOfGuests}
Total: ${full.TotalAmount:N2}
Pago: ${payment.Amount:N2}
Método: {payment.PaymentMethod}
Estado: Confirmado ✅";
                                var waDto = new DTOs.WhatsApp.SendWhatsAppMessageDto { To = adminPhone, Body = msg };
                                await wa.SendMessageAsync(companyId, waDto);
                            }
                        }
                        catch { /* ignore WhatsApp errors here */ }

                        // App notification (bell) with improved details
                        await _notificationService.CreateAsync(companyId,
                            type: "reservation_paid",
                            title: $"Reservación #{reservationId:D6} pagada",
                            message: $"Cliente: {full.Customer?.FullName ?? "N/A"} • Habitación: {full.Room?.Name ?? "N/A"} • Total: ${full.TotalAmount:N2}",
                            data: new { reservationId, customerName = full.Customer?.FullName, roomName = full.Room?.Name, amount = full.TotalAmount },
                            relatedType: "reservation",
                            relatedId: reservationId.ToString());
                    }
                }
                catch (Exception)
                {
                    // Errors in side-effects should not break payment recording; they are logged by the services
                }

                return new ReservationResponseDto
                {
                    Id = reservationId,
                    Success = true,
                    Message = $"Pago de ${dto.Amount:N2} registrado exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ReservationResponseDto
                {
                    Success = false,
                    Message = $"Error al registrar el pago: {ex.Message}"
                };
            }
        }

        public async Task<List<ReservationPaymentDto>> GetPaymentsByReservationAsync(int reservationId, int companyId)
        {
            var payments = await _context.ReservationPayments
                .Include(p => p.Reservation)
                .Where(p => p.ReservationId == reservationId && p.Reservation.CompanyId == companyId)
                .Select(p => new ReservationPaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId,
                    Notes = p.Notes
                })
                .ToListAsync();

            return payments;
        }

        public async Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
        {
            // Ensure dates are in UTC for comparison
            var checkInUtc = checkIn.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(checkIn, DateTimeKind.Utc)
                : checkIn.ToUniversalTime();
                
            var checkOutUtc = checkOut.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(checkOut, DateTimeKind.Utc)
                : checkOut.ToUniversalTime();
                
            var query = _context.Reservations
                .Where(r => r.RoomId == roomId &&
                           r.Status != "Cancelled" &&
                           r.CheckInDate < checkOutUtc &&
                           r.CheckOutDate > checkInUtc);

            if (excludeReservationId.HasValue)
                query = query.Where(r => r.Id != excludeReservationId.Value);

            var hasConflict = await query.AnyAsync();
            return !hasConflict;
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(int companyId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
        {
            // Get all rooms for the company
            var allRooms = await _context.Rooms
                .Where(r => r.CompanyId == companyId && r.IsActive)
                .ToListAsync();

            var availableRooms = new List<Room>();

            foreach (var room in allRooms)
            {
                var isAvailable = await CheckRoomAvailabilityAsync(room.Id, checkIn, checkOut, excludeReservationId);
                if (isAvailable)
                {
                    availableRooms.Add(room);
                }
            }

            return availableRooms;
        }

        public async Task<Dictionary<string, object>> GetReservationStatsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Reservations.Where(r => r.CompanyId == companyId);

            if (startDate.HasValue)
                query = query.Where(r => r.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.CheckOutDate <= endDate.Value);

            var stats = new Dictionary<string, object>
            {
                ["totalReservations"] = await query.CountAsync(),
                ["pendingReservations"] = await query.CountAsync(r => r.Status == "Pending"),
                ["confirmedReservations"] = await query.CountAsync(r => r.Status == "Confirmed"),
                ["checkedInReservations"] = await query.CountAsync(r => r.Status == "CheckedIn"),
                ["cancelledReservations"] = await query.CountAsync(r => r.Status == "Cancelled"),
                ["totalRevenue"] = await query.Where(r => r.Status != "Cancelled").SumAsync(r => r.TotalAmount),
                ["averageStayLength"] = await query.Where(r => r.Status != "Cancelled").AverageAsync(r => (double)r.NumberOfNights)
            };

            return stats;
        }
    }
}
