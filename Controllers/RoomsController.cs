using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.Rooms;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Obtener companyId del token (minúscula con fallback)
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Usar company por defecto
                }

                var rooms = await _roomService.GetAllAsync(companyId);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener las habitaciones", details = ex.Message });
            }
        }

        [HttpGet("company/{companyId}/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicRooms(int companyId)
        {
            try
            {
                var rooms = await _roomService.GetAllAsync(companyId);
                // Filter only active/available rooms for public view
                var publicRooms = rooms.Where(r => r.IsActive).ToList();
                return Ok(publicRooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener las habitaciones", details = ex.Message });
            }
        }

        [HttpGet("company/{companyId}/first-active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFirstActiveRoom(int companyId)
        {
            try
            {
                var rooms = await _roomService.GetAllAsync(companyId);
                var firstActive = rooms.FirstOrDefault(r => r.IsActive);
                
                if (firstActive == null)
                    return NotFound(new { error = "No hay habitaciones activas" });
                    
                return Ok(firstActive);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener la habitación", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var room = await _roomService.GetByIdAsync(companyId, id);
                
                if (room == null)
                    return NotFound(new { error = "Habitación no encontrada" });

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener la habitación", details = ex.Message });
            }
        }

        [HttpGet("company/{companyId}/slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug(int companyId, string slug)
        {
            try
            {
                var room = await _roomService.GetBySlugAsync(companyId, slug);
                
                if (room == null)
                    return NotFound(new { error = "Habitación no encontrada", slug = slug });

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener la habitación por slug", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );
                    return BadRequest(new { 
                        error = "Datos inválidos. Por favor revise los campos del formulario.", 
                        validationErrors = errors 
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                // Verificar si ya existe una habitación con el mismo código
                if (!string.IsNullOrEmpty(dto.RoomCode))
                {
                    var existingRooms = await _roomService.GetAllAsync(companyId);
                    if (existingRooms.Any(r => r.RoomCode?.ToLower() == dto.RoomCode.ToLower()))
                    {
                        return BadRequest(new { 
                            error = $"Ya existe una habitación con el código '{dto.RoomCode}'. Por favor use un código diferente." 
                        });
                    }
                }

                var room = await _roomService.CreateAsync(companyId, dto);
                return CreatedAtAction(nameof(GetById), new { id = room.Id }, new
                {
                    data = room,
                    message = $"Habitación '{room.Name}' creada exitosamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Ocurrió un error inesperado al crear la habitación. Por favor intente nuevamente.", 
                    details = ex.Message 
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );
                    return BadRequest(new { 
                        error = "Datos inválidos. Por favor revise los campos del formulario.", 
                        validationErrors = errors 
                    });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                // Verificar si el código ya existe en otra habitación
                if (!string.IsNullOrEmpty(dto.RoomCode))
                {
                    var existingRooms = await _roomService.GetAllAsync(companyId);
                    var duplicateRoom = existingRooms.FirstOrDefault(r => 
                        r.RoomCode?.ToLower() == dto.RoomCode.ToLower() && r.Id != id);
                    
                    if (duplicateRoom != null)
                    {
                        return BadRequest(new { 
                            error = $"El código '{dto.RoomCode}' ya está siendo usado por la habitación '{duplicateRoom.Name}'." 
                        });
                    }
                }

                var room = await _roomService.UpdateAsync(companyId, id, dto);
                
                if (room == null)
                    return NotFound(new { 
                        error = $"No se encontró la habitación con ID {id}. Es posible que haya sido eliminada." 
                    });

                return Ok(new {
                    data = room,
                    message = $"Habitación '{room.Name}' actualizada exitosamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Ocurrió un error inesperado al actualizar la habitación. Por favor intente nuevamente.", 
                    details = ex.Message 
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                // Obtener la habitación antes de eliminar para mostrar su nombre
                var room = await _roomService.GetByIdAsync(companyId, id);
                if (room == null)
                {
                    return NotFound(new { 
                        error = $"No se encontró la habitación con ID {id}. Es posible que ya haya sido eliminada." 
                    });
                }

                var roomName = room.Name;
                var result = await _roomService.DeleteAsync(companyId, id);
                
                if (!result)
                    return NotFound(new { 
                        error = "No se pudo eliminar la habitación. Por favor intente nuevamente." 
                    });

                return Ok(new { 
                    message = $"La habitación '{roomName}' fue eliminada exitosamente." 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Ocurrió un error inesperado al eliminar la habitación. Por favor intente nuevamente.", 
                    details = ex.Message 
                });
            }
        }
    }
}