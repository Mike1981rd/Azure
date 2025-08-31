using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public interface IReservationService
    {
        // CRUD Operations
        Task<List<ReservationListDto>> GetReservationsByCompanyAsync(int companyId, string? status = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<ReservationDetailsDto?> GetReservationByIdAsync(int id, int companyId);
        Task<ReservationResponseDto> CreateReservationAsync(int companyId, CreateReservationDto dto, int? userId = null);
        Task<ReservationResponseDto> UpdateReservationAsync(int id, int companyId, UpdateReservationDto dto);
        Task<ReservationResponseDto> DeleteReservationAsync(int id, int companyId);
        
        // Status Management
        Task<ReservationResponseDto> ConfirmReservationAsync(int id, int companyId);
        Task<ReservationResponseDto> CheckInAsync(int id, int companyId, int? userId = null);
        Task<ReservationResponseDto> CheckOutAsync(int id, int companyId, int? userId = null);
        Task<ReservationResponseDto> CancelReservationAsync(int id, int companyId, string reason);
        
        // Payment Operations
        Task<ReservationResponseDto> AddPaymentAsync(int reservationId, int companyId, CreatePaymentDto dto, int? userId = null);
        Task<List<ReservationPaymentDto>> GetPaymentsByReservationAsync(int reservationId, int companyId);
        
        // Availability Check
        Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(int companyId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
        
        // Reports
        Task<Dictionary<string, object>> GetReservationStatsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null);
    }
}