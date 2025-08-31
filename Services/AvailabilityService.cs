using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Availability;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AvailabilityGridDto> GetAvailabilityGridAsync(int companyId, DateTime startDate, DateTime endDate, int? roomId = null)
        {
            try
            {
                // Convert dates to UTC to avoid PostgreSQL timezone issues
                startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
                endDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
                
                Console.WriteLine($"[AVAILABILITY DEBUG] GetAvailabilityGridAsync called with:");
                Console.WriteLine($"[AVAILABILITY DEBUG] CompanyId: {companyId}");
                Console.WriteLine($"[AVAILABILITY DEBUG] StartDate: {startDate:yyyy-MM-dd} (Kind: {startDate.Kind})");
                Console.WriteLine($"[AVAILABILITY DEBUG] EndDate: {endDate:yyyy-MM-dd} (Kind: {endDate.Kind})");
                Console.WriteLine($"[AVAILABILITY DEBUG] RoomId: {roomId?.ToString() ?? "null"}");

                var query = _context.Rooms
                    .Where(r => r.CompanyId == companyId && r.IsActive);

                if (roomId.HasValue)
                {
                    query = query.Where(r => r.Id == roomId.Value);
                }

                var rooms = await query
                    .OrderBy(r => r.FloorNumber)
                    .ThenBy(r => r.RoomCode)
                    .Select(r => new RoomDto
                    {
                        Id = r.Id,
                        Name = r.Name ?? "Room " + r.Id,
                        RoomCode = r.RoomCode ?? "",
                        RoomType = r.RoomType ?? "Standard",
                        BasePrice = r.BasePrice,
                        MaxOccupancy = r.MaxOccupancy,
                        FloorNumber = r.FloorNumber
                    })
                    .ToListAsync();

                Console.WriteLine($"[AVAILABILITY DEBUG] Found {rooms.Count} rooms");
                if (rooms.Count == 0)
                {
                    Console.WriteLine($"[AVAILABILITY DEBUG] No rooms found! Checking raw query...");
                    var totalRooms = await _context.Rooms.CountAsync();
                    var companyRooms = await _context.Rooms.Where(r => r.CompanyId == companyId).CountAsync();
                    var activeRooms = await _context.Rooms.Where(r => r.IsActive).CountAsync();
                    var companyActiveRooms = await _context.Rooms.Where(r => r.CompanyId == companyId && r.IsActive).CountAsync();
                    
                    Console.WriteLine($"[AVAILABILITY DEBUG] Total rooms in DB: {totalRooms}");
                    Console.WriteLine($"[AVAILABILITY DEBUG] Rooms with CompanyId={companyId}: {companyRooms}");
                    Console.WriteLine($"[AVAILABILITY DEBUG] Active rooms: {activeRooms}");
                    Console.WriteLine($"[AVAILABILITY DEBUG] Active rooms with CompanyId={companyId}: {companyActiveRooms}");
                }
                else
                {
                    Console.WriteLine($"[AVAILABILITY DEBUG] Room IDs found: {string.Join(", ", rooms.Select(r => r.Id))}");
                }

            var dates = new List<DateTime>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            var availabilities = await _context.RoomAvailabilities
                .Where(ra => ra.CompanyId == companyId &&
                            ra.Date >= startDate && ra.Date <= endDate)
                .Include(ra => ra.Reservation)
                    .ThenInclude(r => r!.Customer)
                .ToListAsync();

            var blockPeriods = await _context.RoomBlockPeriods
                .Where(bp => bp.CompanyId == companyId && bp.IsActive &&
                            bp.StartDate <= endDate && bp.EndDate >= startDate)
                .ToListAsync();

            var reservations = await _context.Reservations
                .Include(r => r.Customer)
                .Where(r => r.CompanyId == companyId &&
                           r.CheckInDate <= endDate && r.CheckOutDate >= startDate &&
                           (r.Status.ToLower() == "confirmed" || r.Status == "Confirmed")) // SOLO reservaciones confirmadas (case insensitive)
                .ToListAsync();
            
            Console.WriteLine($"[AVAILABILITY DEBUG] Found {reservations.Count} confirmed reservations for CompanyId={companyId}");
            Console.WriteLine($"[AVAILABILITY DEBUG] Date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            foreach (var res in reservations)
            {
                Console.WriteLine($"[AVAILABILITY DEBUG] Reservation ID={res.Id}, RoomId={res.RoomId}, Customer={res.Customer?.FullName}, CheckIn={res.CheckInDate:yyyy-MM-dd}, CheckOut={res.CheckOutDate:yyyy-MM-dd}, Status={res.Status}");
            }

            var availabilityDict = new Dictionary<string, DayAvailabilityDto>();

            foreach (var room in rooms)
            {
                foreach (var date in dates)
                {
                    var key = $"{room.Id}_{date:yyyy-MM-dd}";
                    var availability = availabilities.FirstOrDefault(a => a.RoomId == room.Id && a.Date.Date == date.Date);
                    var blockPeriod = blockPeriods.FirstOrDefault(bp => 
                        (bp.RoomId == null || bp.RoomId == room.Id) &&
                        bp.StartDate.Date <= date.Date && bp.EndDate.Date >= date.Date);
                    
                    // SIMPLIFICADO: Buscar reservación para este día específico
                    // Una reservación ocupa desde CheckInDate hasta CheckOutDate-1 (el día de checkout la habitación está disponible)
                    var reservation = reservations.FirstOrDefault(r => 
                        r.RoomId == room.Id &&
                        date.Date >= r.CheckInDate.Date && 
                        date.Date < r.CheckOutDate.Date);
                    
                    // Debug para TODAS las habitaciones y días
                    if (reservation != null)
                    {
                        Console.WriteLine($"[FOUND RESERVATION] Room {room.Id} on {date:yyyy-MM-dd}: Reservation ID={reservation.Id}, Guest={reservation.Customer?.FullName}, CheckIn={reservation.CheckInDate:yyyy-MM-dd}, CheckOut={reservation.CheckOutDate:yyyy-MM-dd}");
                    }

                    var dayAvailability = new DayAvailabilityDto
                    {
                        Date = date,
                        IsAvailable = reservation == null && blockPeriod == null,
                        IsBlocked = blockPeriod != null,
                        HasReservation = reservation != null,
                        BlockReason = blockPeriod?.Reason ?? availability?.BlockReason,
                        CustomPrice = availability?.CustomPrice ?? room.BasePrice,
                        ReservationId = reservation?.Id,
                        GuestName = reservation?.Customer?.FullName,
                        GuestInitials = GetInitials(reservation?.Customer?.FullName),
                        IsCheckIn = reservation != null && reservation.CheckInDate.Date == date.Date,
                        IsCheckOut = reservation != null && reservation.CheckOutDate.Date == date.Date,
                        IsToday = date.Date == DateTime.UtcNow.Date,
                        MinNights = availability?.MinNights
                    };

                    availabilityDict[key] = dayAvailability;
                }
            }

                var result = new AvailabilityGridDto
                {
                    Rooms = rooms,
                    Dates = dates,
                    Availability = availabilityDict,
                    TotalRooms = rooms.Count,
                    TotalDays = dates.Count,
                    StartDate = startDate,
                    EndDate = endDate
                };
                
                Console.WriteLine($"[AVAILABILITY DEBUG] Returning grid with:");
                Console.WriteLine($"[AVAILABILITY DEBUG] - TotalRooms: {result.TotalRooms}");
                Console.WriteLine($"[AVAILABILITY DEBUG] - TotalDays: {result.TotalDays}");
                Console.WriteLine($"[AVAILABILITY DEBUG] - Availability entries: {result.Availability.Count}");
                
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"[AVAILABILITY ERROR] Exception in GetAvailabilityGridAsync: {ex.Message}");
                Console.WriteLine($"[AVAILABILITY ERROR] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[AVAILABILITY ERROR] Inner exception: {ex.InnerException?.Message}");
                
                // Return empty grid instead of throwing
                return new AvailabilityGridDto
                {
                    Rooms = new List<RoomDto>(),
                    Dates = new List<DateTime>(),
                    Availability = new Dictionary<string, DayAvailabilityDto>(),
                    TotalRooms = 0,
                    TotalDays = 0,
                    StartDate = startDate,
                    EndDate = endDate
                };
            }
        }

        public async Task<CheckAvailabilityResponseDto> CheckRoomAvailabilityAsync(int companyId, CheckAvailabilityRequestDto request)
        {
            var response = new CheckAvailabilityResponseDto
            {
                IsAvailable = true,
                BlockedDates = new List<DateTime>(),
                ReservedDates = new List<DateTime>(),
                AppliedRules = new List<string>()
            };

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.CompanyId == companyId);

            if (room == null)
            {
                response.IsAvailable = false;
                response.UnavailableReason = "Room not found";
                return response;
            }

            var dates = new List<DateTime>();
            for (var date = request.CheckIn.Date; date < request.CheckOut.Date; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            var availabilities = await _context.RoomAvailabilities
                .Where(ra => ra.RoomId == request.RoomId &&
                            ra.Date >= request.CheckIn.Date && ra.Date < request.CheckOut.Date)
                .ToListAsync();

            var blockPeriods = await _context.RoomBlockPeriods
                .Where(bp => bp.CompanyId == companyId && bp.IsActive &&
                            (bp.RoomId == null || bp.RoomId == request.RoomId) &&
                            bp.StartDate < request.CheckOut && bp.EndDate >= request.CheckIn)
                .ToListAsync();

            var existingReservations = await _context.Reservations
                .Where(r => r.RoomId == request.RoomId &&
                           r.CheckInDate < request.CheckOut && r.CheckOutDate > request.CheckIn &&
                           r.Status == "confirmed") // SOLO reservaciones confirmadas
                .ToListAsync();

            if (request.ExcludeReservationId.HasValue)
            {
                existingReservations = existingReservations
                    .Where(r => r.Id != request.ExcludeReservationId.Value)
                    .ToList();
            }

            foreach (var date in dates)
            {
                var availability = availabilities.FirstOrDefault(a => a.Date.Date == date.Date);
                var blockPeriod = blockPeriods.FirstOrDefault(bp =>
                    bp.StartDate.Date <= date.Date && bp.EndDate.Date >= date.Date);
                var reservation = existingReservations.FirstOrDefault(r =>
                    r.CheckInDate.Date <= date.Date && r.CheckOutDate.Date > date.Date);

                if (blockPeriod != null || (availability != null && availability.IsBlocked))
                {
                    response.BlockedDates.Add(date);
                    response.IsAvailable = false;
                    response.UnavailableReason = blockPeriod?.Reason ?? availability?.BlockReason ?? "Room is blocked";
                }

                if (reservation != null)
                {
                    response.ReservedDates.Add(date);
                    response.IsAvailable = false;
                    response.UnavailableReason = "Room is already reserved";
                }

                if (availability != null && !availability.IsAvailable)
                {
                    response.IsAvailable = false;
                    response.UnavailableReason = "Room is not available";
                }
            }

            var rules = await _context.AvailabilityRules
                .Where(r => r.CompanyId == companyId && r.IsActive &&
                           (r.RoomId == null || r.RoomId == request.RoomId))
                .OrderBy(r => r.Priority)
                .ToListAsync();

            var nights = (request.CheckOut - request.CheckIn).Days;
            decimal totalPrice = 0;

            foreach (var date in dates)
            {
                var availability = availabilities.FirstOrDefault(a => a.Date.Date == date.Date);
                var price = availability?.CustomPrice ?? room.BasePrice;
                totalPrice += price;
            }

            response.TotalPrice = totalPrice;

            foreach (var rule in rules)
            {
                if (rule.StartDate.HasValue && request.CheckIn < rule.StartDate.Value)
                    continue;
                if (rule.EndDate.HasValue && request.CheckOut > rule.EndDate.Value)
                    continue;

                var ruleConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(rule.RuleValue) ?? new Dictionary<string, object>();

                switch (rule.RuleType)
                {
                    case "min_nights":
                        if (ruleConfig.TryGetValue("minNights", out var minNightsObj))
                        {
                            var minNights = Convert.ToInt32(minNightsObj);
                            if (nights < minNights)
                            {
                                response.IsAvailable = false;
                                response.UnavailableReason = $"Minimum {minNights} nights required";
                                response.MinNightsRequired = minNights;
                                response.AppliedRules.Add($"Minimum nights: {minNights}");
                            }
                        }
                        break;

                    case "no_checkin_days":
                        if (ruleConfig.TryGetValue("days", out var daysObj))
                        {
                            var days = JsonSerializer.Deserialize<List<int>>(daysObj.ToString()) ?? new List<int>();
                            if (days.Contains((int)request.CheckIn.DayOfWeek))
                            {
                                response.IsAvailable = false;
                                response.UnavailableReason = "Check-in not allowed on this day";
                                response.AppliedRules.Add("Check-in day restriction");
                            }
                        }
                        break;

                    case "advance_booking":
                        if (ruleConfig.TryGetValue("maxDays", out var maxDaysObj))
                        {
                            var maxDays = Convert.ToInt32(maxDaysObj);
                            var daysInAdvance = (request.CheckIn - DateTime.UtcNow).Days;
                            if (daysInAdvance > maxDays)
                            {
                                response.IsAvailable = false;
                                response.UnavailableReason = $"Cannot book more than {maxDays} days in advance";
                                response.AppliedRules.Add($"Max advance booking: {maxDays} days");
                            }
                        }
                        break;
                }
            }

            return response;
        }

        public async Task<List<RoomAvailabilityDto>> GetRoomAvailabilityAsync(int companyId, int roomId, DateTime startDate, DateTime endDate)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomId && r.CompanyId == companyId);

            if (room == null)
                return new List<RoomAvailabilityDto>();

            var availabilities = await _context.RoomAvailabilities
                .Where(ra => ra.RoomId == roomId &&
                            ra.Date >= startDate.Date && ra.Date <= endDate.Date)
                .Include(ra => ra.Reservation)
                    .ThenInclude(r => r!.Customer)
                .ToListAsync();
            
            Console.WriteLine($"[AVAILABILITY DEBUG] Found {availabilities.Count} availability records for room {roomId} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");
            
            // Also check for reservations directly
            var reservations = await _context.Reservations
                .Where(r => r.RoomId == roomId && 
                           r.CheckInDate <= endDate && 
                           r.CheckOutDate >= startDate &&
                           r.Status != "cancelled")
                .ToListAsync();
            
            Console.WriteLine($"[AVAILABILITY DEBUG] Found {reservations.Count} reservations for room {roomId} in date range");
            if (reservations.Any())
            {
                foreach (var res in reservations.Take(3))
                {
                    Console.WriteLine($"  - Reservation {res.Id}: CheckIn={res.CheckInDate:yyyy-MM-dd}, CheckOut={res.CheckOutDate:yyyy-MM-dd}, Status={res.Status}");
                    Console.WriteLine($"    Note: CheckOut date {res.CheckOutDate:yyyy-MM-dd} is AVAILABLE for new reservations");
                }
            }

            var result = new List<RoomAvailabilityDto>();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var availability = availabilities.FirstOrDefault(a => a.Date.Date == date.Date);
                
                // Check if this date has a reservation
                // Note: CheckOut date is available for new reservations (guest leaves in the morning)
                var hasReservation = reservations.Any(r => 
                    date >= r.CheckInDate.Date && 
                    date < r.CheckOutDate.Date &&  // Changed from <= to < (checkout day is available)
                    r.Status != "cancelled");
                
                var reservation = reservations.FirstOrDefault(r => 
                    date >= r.CheckInDate.Date && 
                    date < r.CheckOutDate.Date &&  // Changed from <= to < (checkout day is available)
                    r.Status != "cancelled");
                
                // Determine if the date is available
                var isAvailable = availability?.IsAvailable ?? true;
                var isBlocked = availability?.IsBlocked ?? false;
                
                // If there's a reservation, the date is not available
                if (hasReservation)
                {
                    isAvailable = false;
                }
                
                result.Add(new RoomAvailabilityDto
                {
                    RoomId = roomId,
                    RoomName = room.Name,
                    RoomCode = room.RoomCode ?? "",
                    Date = date,
                    IsAvailable = isAvailable && !isBlocked && !hasReservation,
                    IsBlocked = isBlocked,
                    HasReservation = hasReservation || availability?.ReservationId != null,
                    BlockReason = availability?.BlockReason,
                    Price = availability?.CustomPrice ?? room.BasePrice,
                    ReservationId = reservation?.Id ?? availability?.ReservationId,
                    GuestName = reservation?.Customer?.FullName ?? availability?.Reservation?.Customer?.FullName,
                    ReservationStatus = reservation?.Status ?? availability?.Reservation?.Status,
                    IsCheckIn = reservations.Any(r => r.CheckInDate.Date == date.Date && r.Status != "cancelled"),
                    IsCheckOut = reservations.Any(r => r.CheckOutDate.Date == date.Date && r.Status != "cancelled")
                });
            }

            return result;
        }

        public async Task<bool> UpdateRoomAvailabilityAsync(int companyId, int roomId, DateTime date, bool isAvailable, decimal? customPrice = null, int? minNights = null)
        {
            var availability = await _context.RoomAvailabilities
                .FirstOrDefaultAsync(ra => ra.CompanyId == companyId && ra.RoomId == roomId && ra.Date.Date == date.Date);

            if (availability == null)
            {
                availability = new RoomAvailability
                {
                    CompanyId = companyId,
                    RoomId = roomId,
                    Date = date.Date,
                    IsAvailable = isAvailable,
                    CustomPrice = customPrice,
                    MinNights = minNights,
                    CreatedAt = DateTime.UtcNow
                };
                _context.RoomAvailabilities.Add(availability);
            }
            else
            {
                availability.IsAvailable = isAvailable;
                availability.CustomPrice = customPrice;
                availability.MinNights = minNights;
                availability.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkUpdateAvailabilityAsync(int companyId, BulkAvailabilityUpdateDto request)
        {
            var dates = new List<DateTime>();
            for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            foreach (var roomId in request.RoomIds)
            {
                var existingAvailabilities = await _context.RoomAvailabilities
                    .Where(ra => ra.CompanyId == companyId && ra.RoomId == roomId &&
                                ra.Date >= request.StartDate.Date && ra.Date <= request.EndDate.Date)
                    .ToDictionaryAsync(ra => ra.Date.Date);

                foreach (var date in dates)
                {
                    if (existingAvailabilities.TryGetValue(date, out var availability))
                    {
                        if (request.IsAvailable.HasValue)
                            availability.IsAvailable = request.IsAvailable.Value;
                        if (request.CustomPrice.HasValue)
                            availability.CustomPrice = request.CustomPrice.Value;
                        if (request.MinNights.HasValue)
                            availability.MinNights = request.MinNights.Value;
                        availability.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.RoomAvailabilities.Add(new RoomAvailability
                        {
                            CompanyId = companyId,
                            RoomId = roomId,
                            Date = date,
                            IsAvailable = request.IsAvailable ?? true,
                            CustomPrice = request.CustomPrice,
                            MinNights = request.MinNights,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BlockPeriodDto> CreateBlockPeriodAsync(int companyId, int userId, CreateBlockPeriodDto request)
        {
            var blockPeriod = new RoomBlockPeriod
            {
                CompanyId = companyId,
                RoomId = request.RoomIds?.FirstOrDefault(),
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date,
                Reason = request.Reason,
                Notes = request.Notes,
                IsRecurring = request.IsRecurring,
                RecurrencePattern = request.RecurrencePattern,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                IsActive = true
            };

            _context.RoomBlockPeriods.Add(blockPeriod);

            if (request.RoomIds != null && request.RoomIds.Any())
            {
                var dates = new List<DateTime>();
                for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
                {
                    dates.Add(date);
                }

                foreach (var roomId in request.RoomIds)
                {
                    foreach (var date in dates)
                    {
                        var availability = await _context.RoomAvailabilities
                            .FirstOrDefaultAsync(ra => ra.CompanyId == companyId && ra.RoomId == roomId && ra.Date.Date == date.Date);

                        if (availability == null)
                        {
                            availability = new RoomAvailability
                            {
                                CompanyId = companyId,
                                RoomId = roomId,
                                Date = date,
                                IsAvailable = false,
                                IsBlocked = true,
                                BlockReason = request.Reason,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.RoomAvailabilities.Add(availability);
                        }
                        else
                        {
                            availability.IsAvailable = false;
                            availability.IsBlocked = true;
                            availability.BlockReason = request.Reason;
                            availability.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new BlockPeriodDto
            {
                Id = blockPeriod.Id,
                RoomIds = request.RoomIds ?? new List<int>(),
                StartDate = blockPeriod.StartDate,
                EndDate = blockPeriod.EndDate,
                Reason = blockPeriod.Reason,
                Notes = blockPeriod.Notes,
                IsRecurring = blockPeriod.IsRecurring,
                RecurrencePattern = blockPeriod.RecurrencePattern,
                CreatedAt = blockPeriod.CreatedAt,
                CreatedByName = user?.FullName ?? "System",
                IsActive = blockPeriod.IsActive
            };
        }

        public async Task<List<BlockPeriodDto>> GetBlockPeriodsAsync(int companyId, DateTime? startDate = null, DateTime? endDate = null)
        {
            IQueryable<RoomBlockPeriod> query = _context.RoomBlockPeriods
                .Where(bp => bp.CompanyId == companyId && bp.IsActive)
                .Include(bp => bp.Room)
                .Include(bp => bp.CreatedBy);

            if (startDate.HasValue)
                query = query.Where(bp => bp.EndDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(bp => bp.StartDate <= endDate.Value);

            var blockPeriods = await query.OrderBy(bp => bp.StartDate).ToListAsync();

            return blockPeriods.Select(bp => new BlockPeriodDto
            {
                Id = bp.Id,
                RoomIds = bp.RoomId.HasValue ? new List<int> { bp.RoomId.Value } : new List<int>(),
                RoomNames = bp.Room?.Name,
                StartDate = bp.StartDate,
                EndDate = bp.EndDate,
                Reason = bp.Reason,
                Notes = bp.Notes,
                IsRecurring = bp.IsRecurring,
                RecurrencePattern = bp.RecurrencePattern,
                CreatedAt = bp.CreatedAt,
                CreatedByName = bp.CreatedBy?.FullName ?? "System",
                IsActive = bp.IsActive
            }).ToList();
        }

        public async Task<bool> UpdateBlockPeriodAsync(int companyId, int blockPeriodId, CreateBlockPeriodDto request)
        {
            var blockPeriod = await _context.RoomBlockPeriods
                .FirstOrDefaultAsync(bp => bp.Id == blockPeriodId && bp.CompanyId == companyId);

            if (blockPeriod == null)
                return false;

            blockPeriod.RoomId = request.RoomIds?.FirstOrDefault();
            blockPeriod.StartDate = request.StartDate.Date;
            blockPeriod.EndDate = request.EndDate.Date;
            blockPeriod.Reason = request.Reason;
            blockPeriod.Notes = request.Notes;
            blockPeriod.IsRecurring = request.IsRecurring;
            blockPeriod.RecurrencePattern = request.RecurrencePattern;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBlockPeriodAsync(int companyId, int blockPeriodId)
        {
            var blockPeriod = await _context.RoomBlockPeriods
                .FirstOrDefaultAsync(bp => bp.Id == blockPeriodId && bp.CompanyId == companyId);

            if (blockPeriod == null)
                return false;

            blockPeriod.IsActive = false;

            if (blockPeriod.RoomId.HasValue)
            {
                var availabilities = await _context.RoomAvailabilities
                    .Where(ra => ra.CompanyId == companyId && 
                                ra.RoomId == blockPeriod.RoomId.Value &&
                                ra.Date >= blockPeriod.StartDate.Date && 
                                ra.Date <= blockPeriod.EndDate.Date &&
                                ra.BlockReason == blockPeriod.Reason)
                    .ToListAsync();

                foreach (var availability in availabilities)
                {
                    availability.IsBlocked = false;
                    availability.BlockReason = null;
                    availability.IsAvailable = availability.ReservationId == null;
                    availability.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AvailabilityRuleDto> CreateAvailabilityRuleAsync(int companyId, UpdateAvailabilityRuleDto request)
        {
            var rule = new AvailabilityRule
            {
                CompanyId = companyId,
                RoomId = request.RoomId,
                RuleType = request.RuleType,
                RuleValue = JsonSerializer.Serialize(request.RuleConfig),
                IsActive = request.IsActive,
                Priority = request.Priority,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.AvailabilityRules.Add(rule);
            await _context.SaveChangesAsync();

            var room = request.RoomId.HasValue 
                ? await _context.Rooms.FindAsync(request.RoomId.Value) 
                : null;

            return new AvailabilityRuleDto
            {
                Id = rule.Id,
                RoomId = rule.RoomId,
                RoomName = room?.Name,
                RuleType = rule.RuleType,
                RuleTypeLabel = GetRuleTypeLabel(rule.RuleType),
                RuleConfig = request.RuleConfig,
                IsActive = rule.IsActive,
                Priority = rule.Priority,
                StartDate = rule.StartDate,
                EndDate = rule.EndDate,
                CreatedAt = rule.CreatedAt
            };
        }

        public async Task<List<AvailabilityRuleDto>> GetAvailabilityRulesAsync(int companyId, int? roomId = null)
        {
            IQueryable<AvailabilityRule> query = _context.AvailabilityRules
                .Where(r => r.CompanyId == companyId)
                .Include(r => r.Room);

            if (roomId.HasValue)
                query = query.Where(r => r.RoomId == null || r.RoomId == roomId.Value);

            var rules = await query.OrderBy(r => r.Priority).ToListAsync();

            return rules.Select(r => new AvailabilityRuleDto
            {
                Id = r.Id,
                RoomId = r.RoomId,
                RoomName = r.Room?.Name,
                RuleType = r.RuleType,
                RuleTypeLabel = GetRuleTypeLabel(r.RuleType),
                RuleConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(r.RuleValue) ?? new Dictionary<string, object>(),
                IsActive = r.IsActive,
                Priority = r.Priority,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<bool> UpdateAvailabilityRuleAsync(int companyId, int ruleId, UpdateAvailabilityRuleDto request)
        {
            var rule = await _context.AvailabilityRules
                .FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId);

            if (rule == null)
                return false;

            rule.RoomId = request.RoomId;
            rule.RuleType = request.RuleType;
            rule.RuleValue = JsonSerializer.Serialize(request.RuleConfig);
            rule.IsActive = request.IsActive;
            rule.Priority = request.Priority;
            rule.StartDate = request.StartDate;
            rule.EndDate = request.EndDate;
            rule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAvailabilityRuleAsync(int companyId, int ruleId)
        {
            var rule = await _context.AvailabilityRules
                .FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId);

            if (rule == null)
                return false;

            _context.AvailabilityRules.Remove(rule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OccupancyStatsDto> GetOccupancyStatsAsync(int companyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Convert dates to UTC to avoid PostgreSQL timezone issues
                startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
                endDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);
                
                var rooms = await _context.Rooms
                    .Where(r => r.CompanyId == companyId && r.IsActive)
                    .ToListAsync();

            var reservations = await _context.Reservations
                .Include(r => r.Customer)
                .Where(r => r.CompanyId == companyId &&
                           r.CheckInDate <= endDate && r.CheckOutDate >= startDate &&
                           (r.Status.ToLower() == "confirmed" || r.Status == "Confirmed")) // SOLO reservaciones confirmadas (case insensitive)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var stats = new OccupancyStatsDto
            {
                TotalRooms = rooms.Count,
                TotalNights = (endDate - startDate).Days * rooms.Count,
                DailyStats = new List<DailyOccupancyDto>()
            };

            var occupancyByRoomType = new Dictionary<string, (int occupied, int total)>();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dailyStat = new DailyOccupancyDto
                {
                    Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                    AvailableRooms = rooms.Count,
                    OccupiedRooms = 0,
                    BlockedRooms = 0,
                    Revenue = 0
                };

                foreach (var room in rooms)
                {
                    var reservation = reservations.FirstOrDefault(r =>
                        r.RoomId == room.Id &&
                        r.CheckInDate.Date <= date.Date && r.CheckOutDate.Date > date.Date);

                    if (reservation != null)
                    {
                        dailyStat.OccupiedRooms++;
                        dailyStat.AvailableRooms--;
                        dailyStat.Revenue += room.BasePrice;

                        if (!string.IsNullOrEmpty(room.RoomType))
                        {
                            if (!occupancyByRoomType.ContainsKey(room.RoomType))
                                occupancyByRoomType[room.RoomType] = (0, 0);
                            
                            var (occupied, total) = occupancyByRoomType[room.RoomType];
                            occupancyByRoomType[room.RoomType] = (occupied + 1, total + 1);
                        }
                    }
                    else if (!string.IsNullOrEmpty(room.RoomType))
                    {
                        if (!occupancyByRoomType.ContainsKey(room.RoomType))
                            occupancyByRoomType[room.RoomType] = (0, 0);
                        
                        var (occupied, total) = occupancyByRoomType[room.RoomType];
                        occupancyByRoomType[room.RoomType] = (occupied, total + 1);
                    }

                    if (date == today)
                    {
                        if (reservation?.CheckInDate.Date == today)
                            stats.CheckInsToday++;
                        if (reservation?.CheckOutDate.Date == today)
                            stats.CheckOutsToday++;
                    }
                }

                dailyStat.OccupancyRate = rooms.Count > 0 
                    ? (decimal)dailyStat.OccupiedRooms / rooms.Count * 100 
                    : 0;

                stats.DailyStats.Add(dailyStat);
                stats.OccupiedNights += dailyStat.OccupiedRooms;
                stats.ProjectedRevenue += dailyStat.Revenue;
            }

            stats.OccupancyRate = stats.TotalNights > 0 
                ? (decimal)stats.OccupiedNights / stats.TotalNights * 100 
                : 0;

            stats.OccupancyByRoomType = occupancyByRoomType.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.total > 0 ? (decimal)kvp.Value.occupied / kvp.Value.total * 100 : 0
            );

            var todayAvailability = await _context.RoomAvailabilities
                .Where(ra => ra.CompanyId == companyId && ra.Date.Date == today)
                .ToListAsync();

            stats.AvailableToday = rooms.Count - reservations.Count(r => 
                r.CheckInDate.Date <= today && r.CheckOutDate.Date > today);
            stats.BlockedToday = todayAvailability.Count(ra => ra.IsBlocked);

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOccupancyStatsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Return empty stats instead of throwing
                return new OccupancyStatsDto
                {
                    TotalRooms = 0,
                    TotalNights = 0,
                    OccupiedNights = 0,
                    OccupancyRate = 0,
                    AvailableToday = 0,
                    BlockedToday = 0,
                    CheckInsToday = 0,
                    CheckOutsToday = 0,
                    ProjectedRevenue = 0,
                    DailyStats = new List<DailyOccupancyDto>(),
                    OccupancyByRoomType = new Dictionary<string, decimal>()
                };
            }
        }

        public async Task SyncAvailabilityWithReservationsAsync(int companyId)
        {
            try
            {
                var reservations = await _context.Reservations
                    .Where(r => r.CompanyId == companyId && r.Status == "confirmed") // SOLO reservaciones confirmadas
                    .ToListAsync();

                foreach (var reservation in reservations)
                {
                for (var date = reservation.CheckInDate.Date; date < reservation.CheckOutDate.Date; date = date.AddDays(1))
                {
                    var availability = await _context.RoomAvailabilities
                        .FirstOrDefaultAsync(ra => ra.CompanyId == companyId && 
                                                   ra.RoomId == reservation.RoomId && 
                                                   ra.Date.Date == date.Date);

                    if (availability == null)
                    {
                        availability = new RoomAvailability
                        {
                            CompanyId = companyId,
                            RoomId = reservation.RoomId,
                            Date = date,
                            IsAvailable = false,
                            ReservationId = reservation.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.RoomAvailabilities.Add(availability);
                    }
                    else
                    {
                        availability.IsAvailable = false;
                        availability.ReservationId = reservation.Id;
                        availability.UpdatedAt = DateTime.UtcNow;
                    }
                }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SyncAvailabilityWithReservationsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public async Task<bool> ValidateAvailabilityRulesAsync(int companyId, int roomId, DateTime checkIn, DateTime checkOut)
        {
            var checkRequest = new CheckAvailabilityRequestDto
            {
                RoomId = roomId,
                CheckIn = checkIn,
                CheckOut = checkOut
            };

            var response = await CheckRoomAvailabilityAsync(companyId, checkRequest);
            return response.IsAvailable;
        }

        private string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "??";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            else if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            
            return "??";
        }

        private string GetRuleTypeLabel(string ruleType)
        {
            return ruleType switch
            {
                "min_nights" => "Minimum Nights",
                "max_nights" => "Maximum Nights",
                "no_checkin_days" => "Check-in Day Restrictions",
                "no_checkout_days" => "Check-out Day Restrictions",
                "advance_booking" => "Advance Booking Limit",
                "last_minute_booking" => "Last Minute Booking",
                "seasonal_pricing" => "Seasonal Pricing",
                _ => ruleType
            };
        }
    }
}