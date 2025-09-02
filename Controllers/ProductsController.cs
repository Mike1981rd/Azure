using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Constants;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Products;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista paginada de productos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? collectionId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                // CRÍTICO: Usar minúscula "companyId" con fallback según Guardado.md
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    // Fallback a company 1 para single-tenant
                    companyId = 1;
                    _logger.LogWarning("CompanyId not found in token, using default: 1");
                }

                var result = await _productService.GetProductsAsync(companyId, page, pageSize, search, collectionId, isActive);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.ConnectionFailed));
            }
        }

        /// <summary>
        /// Obtiene un producto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var product = await _productService.GetProductByIdAsync(companyId, id);
                
                if (product == null)
                {
                    return NotFound(ApiResponse.ErrorResponse(ProductMessages.Business.ProductNotFound, 404));
                }

                return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product {id}");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.ConnectionFailed));
            }
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.ValidationErrorResponse(
                        ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    ));
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var product = await _productService.CreateProductAsync(companyId, dto);
                
                return CreatedAtAction(
                    nameof(GetProduct), 
                    new { id = product.Id }, 
                    ApiResponse<ProductResponseDto>.SuccessResponse(product, ProductMessages.Success.ProductCreated)
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.SaveFailed));
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var product = await _productService.UpdateProductAsync(companyId, id, dto);
                
                if (product == null)
                {
                    return NotFound(ApiResponse.ErrorResponse(ProductMessages.Business.ProductNotFound, 404));
                }

                return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, ProductMessages.Success.ProductUpdated));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id}");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.UpdateFailed));
            }
        }

        /// <summary>
        /// Elimina un producto
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var success = await _productService.DeleteProductAsync(companyId, id);
                
                if (!success)
                {
                    return NotFound(ApiResponse.ErrorResponse(ProductMessages.Business.ProductNotFound, 404));
                }

                return Ok(ApiResponse.SuccessResponse(ProductMessages.Success.ProductDeleted));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id}");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.DeleteFailed));
            }
        }

        /// <summary>
        /// Elimina múltiples productos
        /// </summary>
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDeleteProducts([FromBody] List<int> productIds)
        {
            try
            {
                if (productIds == null || productIds.Count == 0)
                {
                    return BadRequest(ApiResponse.ErrorResponse("No product IDs provided"));
                }

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var success = await _productService.BulkDeleteProductsAsync(companyId, productIds);
                
                if (!success)
                {
                    return BadRequest(ApiResponse.ErrorResponse(ProductMessages.Business.ProductsNotFound));
                }

                return Ok(ApiResponse.SuccessResponse(ProductMessages.Success.ProductsDeleted));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk delete");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.TransactionFailed));
            }
        }

        /// <summary>
        /// Cambia el estado activo/inactivo de un producto
        /// </summary>
        [HttpPost("{id}/toggle-status")]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var success = await _productService.ToggleActiveStatusAsync(companyId, id);
                
                if (!success)
                {
                    return NotFound(ApiResponse.ErrorResponse(ProductMessages.Business.ProductNotFound, 404));
                }

                return Ok(ApiResponse.SuccessResponse("Product status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling product status {id}");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.UpdateFailed));
            }
        }

        /// <summary>
        /// Actualiza las imágenes de un producto
        /// </summary>
        [HttpPost("{id}/images")]
        public async Task<IActionResult> UpdateProductImages(int id, [FromBody] List<string> imageUrls)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var success = await _productService.UpdateProductImagesAsync(companyId, id, imageUrls);
                
                if (!success)
                {
                    return NotFound(ApiResponse.ErrorResponse(ProductMessages.Business.ProductNotFound, 404));
                }

                return Ok(ApiResponse.SuccessResponse(ProductMessages.Success.ImagesUpdated));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product images {id}");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.FileOperation.ImageUploadFailed));
            }
        }

        /// <summary>
        /// Asigna un producto a colecciones
        /// </summary>
        [HttpPost("{id}/collections")]
        public async Task<IActionResult> AssignToCollections(int id, [FromBody] List<int> collectionIds)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1;
                }

                var success = await _productService.AssignToCollectionsAsync(companyId, id, collectionIds);
                
                if (!success)
                {
                    return NotFound(ApiResponse.ErrorResponse(ProductMessages.Business.ProductNotFound, 404));
                }

                return Ok(ApiResponse.SuccessResponse(ProductMessages.Success.CollectionsAssigned));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning product {id} to collections");
                return StatusCode(500, ApiResponse.ErrorResponse(ProductMessages.Database.TransactionFailed));
            }
        }
    }
}