using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderAPI.DTOs.Collections;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        public CollectionsController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCollections(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _collectionService.GetPagedAsync(companyId, page, pageSize, search, isActive);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollection(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var collection = await _collectionService.GetByIdAsync(companyId, id);
                if (collection == null)
                    return NotFound(new { error = "Collection not found" });

                return Ok(collection);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("by-handle/{handle}")]
        public async Task<IActionResult> GetCollectionByHandle(string handle)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var collection = await _collectionService.GetByHandleAsync(companyId, handle);
                if (collection == null)
                    return NotFound(new { error = "Collection not found" });

                return Ok(collection);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .Select(x => new { field = x.Key, errors = x.Value!.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var collection = await _collectionService.CreateAsync(companyId, dto);
                return CreatedAtAction(nameof(GetCollection), new { id = collection.Id }, collection);
            }
            catch (InvalidOperationException ex)
            {
                // Log the full exception details
                Console.WriteLine($"InvalidOperationException in CreateCollection: {ex}");
                return BadRequest(new { error = ex.Message, innerError = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"Exception in CreateCollection: {ex}");
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(int id, [FromBody] UpdateCollectionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .Select(x => new { field = x.Key, errors = x.Value!.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var collection = await _collectionService.UpdateAsync(companyId, id, dto);
                if (collection == null)
                    return NotFound(new { error = "Collection not found" });

                return Ok(collection);
            }
            catch (InvalidOperationException ex)
            {
                // Log the full exception details
                Console.WriteLine($"InvalidOperationException in UpdateCollection: {ex}");
                return BadRequest(new { error = ex.Message, innerError = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"Exception in UpdateCollection: {ex}");
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var success = await _collectionService.DeleteAsync(companyId, id);
                if (!success)
                    return NotFound(new { error = "Collection not found" });

                return Ok(new { message = "Collection deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDeleteCollections([FromBody] List<int> collectionIds)
        {
            try
            {
                if (collectionIds == null || collectionIds.Count == 0)
                    return BadRequest(new { error = "No collection IDs provided" });

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var success = await _collectionService.BulkDeleteAsync(companyId, collectionIds);
                if (!success)
                    return NotFound(new { error = "No collections found to delete" });

                return Ok(new { message = $"{collectionIds.Count} collections deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetCollectionProducts(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var products = await _collectionService.GetCollectionProductsAsync(companyId, id);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/products")]
        public async Task<IActionResult> AddProductsToCollection(int id, [FromBody] ManageCollectionProductsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var success = await _collectionService.AddProductsAsync(companyId, id, dto.ProductIds);
                if (!success)
                    return NotFound(new { error = "Collection not found" });

                return Ok(new { message = "Products added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}/products")]
        public async Task<IActionResult> RemoveProductsFromCollection(int id, [FromBody] ManageCollectionProductsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var success = await _collectionService.RemoveProductsAsync(companyId, id, dto.ProductIds);
                if (!success)
                    return NotFound(new { error = "Collection not found" });

                return Ok(new { message = "Products removed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id}/products/positions")]
        public async Task<IActionResult> UpdateProductPositions(int id, [FromBody] BulkUpdateProductPositionsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var success = await _collectionService.UpdateProductPositionsAsync(companyId, id, dto);
                if (!success)
                    return NotFound(new { error = "Collection not found" });

                return Ok(new { message = "Product positions updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("check-handle/{handle}")]
        public async Task<IActionResult> CheckHandleExists(string handle, [FromQuery] int? excludeId = null)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var exists = await _collectionService.HandleExistsAsync(companyId, handle, excludeId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}