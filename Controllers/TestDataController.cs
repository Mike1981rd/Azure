using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("check-rooms")]
        public async Task<IActionResult> CheckRooms()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return Ok(new 
            { 
                totalRooms = rooms.Count,
                rooms = rooms.Select(r => new 
                {
                    r.Id,
                    r.Name,
                    r.CompanyId,
                    r.RoomCode,
                    r.IsActive
                })
            });
        }

        [HttpPost("create-sample-rooms")]
        public async Task<IActionResult> CreateSampleRooms()
        {
            // Check if rooms already exist
            var existingRooms = await _context.Rooms.Where(r => r.CompanyId == 1).ToListAsync();
            if (existingRooms.Any())
            {
                return Ok(new { message = $"Ya existen {existingRooms.Count} habitaciones", rooms = existingRooms });
            }

            // Create sample rooms
            var sampleRooms = new List<Room>
            {
                new Room
                {
                    CompanyId = 1,
                    Name = "Habitación Deluxe 101",
                    RoomCode = "101",
                    RoomType = "Deluxe",
                    Description = "Habitación deluxe con vista al mar",
                    BasePrice = 150.00m,
                    MaxOccupancy = 2,
                    SquareMeters = 35,
                    FloorNumber = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    CompanyId = 1,
                    Name = "Habitación Estándar 102",
                    RoomCode = "102",
                    RoomType = "Standard",
                    Description = "Habitación estándar cómoda",
                    BasePrice = 100.00m,
                    MaxOccupancy = 2,
                    SquareMeters = 25,
                    FloorNumber = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    CompanyId = 1,
                    Name = "Suite Presidencial 201",
                    RoomCode = "201",
                    RoomType = "Suite",
                    Description = "Suite presidencial de lujo",
                    BasePrice = 300.00m,
                    MaxOccupancy = 4,
                    SquareMeters = 60,
                    FloorNumber = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    CompanyId = 1,
                    Name = "Habitación Familiar 103",
                    RoomCode = "103",
                    RoomType = "Family",
                    Description = "Habitación amplia para familias",
                    BasePrice = 180.00m,
                    MaxOccupancy = 4,
                    SquareMeters = 45,
                    FloorNumber = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Room
                {
                    CompanyId = 1,
                    Name = "Habitación Económica 104",
                    RoomCode = "104",
                    RoomType = "Economy",
                    Description = "Habitación económica básica",
                    BasePrice = 75.00m,
                    MaxOccupancy = 1,
                    SquareMeters = 20,
                    FloorNumber = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Rooms.AddRange(sampleRooms);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = $"Se crearon {sampleRooms.Count} habitaciones de prueba",
                rooms = sampleRooms.Select(r => new 
                {
                    r.Id,
                    r.Name,
                    r.RoomCode,
                    r.RoomType,
                    r.BasePrice
                })
            });
        }

        [HttpDelete("delete-all-rooms")]
        public async Task<IActionResult> DeleteAllRooms()
        {
            var rooms = await _context.Rooms.Where(r => r.CompanyId == 1).ToListAsync();
            _context.Rooms.RemoveRange(rooms);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Se eliminaron {rooms.Count} habitaciones" });
        }

        [HttpPost("create-sample-reservations")]
        public async Task<IActionResult> CreateSampleReservations()
        {
            // Obtener las habitaciones existentes
            var rooms = await _context.Rooms.Where(r => r.CompanyId == 1 && r.IsActive).ToListAsync();
            if (!rooms.Any())
            {
                return BadRequest(new { message = "No hay habitaciones disponibles. Primero crea habitaciones de prueba." });
            }

            // Obtener o crear un cliente de prueba
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == "test@example.com");
            if (customer == null)
            {
                customer = new Customer
                {
                    CompanyId = 1,
                    CustomerId = "#" + new Random().Next(100000, 999999),
                    FirstName = "Juan",
                    LastName = "Pérez",
                    Username = "juanperez",
                    Email = "test@example.com",
                    PasswordHash = "dummy", // Solo para pruebas
                    Phone = "809-555-0001",
                    Country = "Dominican Republic",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // Crear reservaciones de prueba para agosto 2025
            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    CompanyId = 1,
                    RoomId = rooms[0].Id,
                    CustomerId = customer.Id,
                    CheckInDate = new DateTime(2025, 8, 5, 0, 0, 0, DateTimeKind.Utc),
                    CheckOutDate = new DateTime(2025, 8, 8, 0, 0, 0, DateTimeKind.Utc),
                    NumberOfGuests = 2,
                    TotalAmount = rooms[0].BasePrice * 3,
                    Status = "confirmed", // IMPORTANTE: Status confirmado
                    CreatedAt = DateTime.UtcNow
                },
                new Reservation
                {
                    CompanyId = 1,
                    RoomId = rooms[0].Id,
                    CustomerId = customer.Id,
                    CheckInDate = new DateTime(2025, 8, 15, 0, 0, 0, DateTimeKind.Utc),
                    CheckOutDate = new DateTime(2025, 8, 18, 0, 0, 0, DateTimeKind.Utc),
                    NumberOfGuests = 2,
                    TotalAmount = rooms[0].BasePrice * 3,
                    Status = "confirmed", // IMPORTANTE: Status confirmado
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Si hay más de una habitación, agregar más reservaciones
            if (rooms.Count > 1)
            {
                // Crear otro cliente
                var customer2 = await _context.Customers.FirstOrDefaultAsync(c => c.Email == "maria@example.com");
                if (customer2 == null)
                {
                    customer2 = new Customer
                    {
                        CompanyId = 1,
                        CustomerId = "#" + new Random().Next(100000, 999999),
                        FirstName = "María",
                        LastName = "García",
                        Username = "mariagarcia",
                        Email = "maria@example.com",
                        PasswordHash = "dummy", // Solo para pruebas
                        Phone = "809-555-0002",
                        Country = "Dominican Republic",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Customers.Add(customer2);
                    await _context.SaveChangesAsync();
                }

                reservations.Add(new Reservation
                {
                    CompanyId = 1,
                    RoomId = rooms[1].Id,
                    CustomerId = customer2.Id,
                    CheckInDate = new DateTime(2025, 8, 10, 0, 0, 0, DateTimeKind.Utc),
                    CheckOutDate = new DateTime(2025, 8, 14, 0, 0, 0, DateTimeKind.Utc),
                    NumberOfGuests = 2,
                    TotalAmount = rooms[1].BasePrice * 4,
                    Status = "confirmed", // IMPORTANTE: Status confirmado
                    CreatedAt = DateTime.UtcNow
                });

                reservations.Add(new Reservation
                {
                    CompanyId = 1,
                    RoomId = rooms[1].Id,
                    CustomerId = customer2.Id,
                    CheckInDate = new DateTime(2025, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                    CheckOutDate = new DateTime(2025, 8, 25, 0, 0, 0, DateTimeKind.Utc),
                    NumberOfGuests = 3,
                    TotalAmount = rooms[1].BasePrice * 5,
                    Status = "confirmed", // IMPORTANTE: Status confirmado
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Reservations.AddRange(reservations);
            await _context.SaveChangesAsync();

            // Sincronizar disponibilidad
            var availabilityService = HttpContext.RequestServices.GetService<WebsiteBuilderAPI.Services.IAvailabilityService>();
            if (availabilityService != null)
            {
                await availabilityService.SyncAvailabilityWithReservationsAsync(1);
            }

            return Ok(new 
            { 
                message = $"Se crearon {reservations.Count} reservaciones confirmadas de prueba",
                reservations = reservations.Select(r => new 
                {
                    r.Id,
                    RoomId = r.RoomId,
                    RoomName = rooms.FirstOrDefault(room => room.Id == r.RoomId)?.Name,
                    CustomerName = r.CustomerId == customer.Id ? customer.FullName : "María García",
                    CheckIn = r.CheckInDate.ToString("yyyy-MM-dd"),
                    CheckOut = r.CheckOutDate.ToString("yyyy-MM-dd"),
                    Status = r.Status
                })
            });
        }

        [HttpGet("check-reservations")]
        public async Task<IActionResult> CheckReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .Where(r => r.CompanyId == 1)
                .OrderBy(r => r.CheckInDate)
                .ToListAsync();

            // Verificar también todas las reservaciones sin filtro
            var allReservations = await _context.Reservations.CountAsync();
            var allConfirmed = await _context.Reservations.Where(r => r.Status == "confirmed").CountAsync();

            return Ok(new 
            { 
                totalReservations = reservations.Count,
                confirmedReservations = reservations.Count(r => r.Status == "confirmed"),
                totalInDatabase = allReservations,
                totalConfirmedInDatabase = allConfirmed,
                reservations = reservations.Select(r => new 
                {
                    r.Id,
                    r.RoomId,
                    RoomName = r.Room?.Name,
                    CustomerName = r.Customer?.FullName,
                    CheckIn = r.CheckInDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    CheckOut = r.CheckOutDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = r.Status,
                    r.CompanyId
                })
            });
        }
    }
}