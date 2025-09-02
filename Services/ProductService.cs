using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Constants;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Products;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ProductResponseDto>> GetProductsAsync(
            int companyId, int page, int pageSize, string? search, int? collectionId, bool? isActive)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.CollectionProducts)
                        .ThenInclude(cp => cp.Collection)
                    .Where(p => p.CompanyId == companyId);

                // Filtros
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p => 
                        p.Name.Contains(search) || 
                        p.Description!.Contains(search) ||
                        p.SKU!.Contains(search) ||
                        p.Barcode!.Contains(search));
                }

                if (collectionId.HasValue)
                {
                    query = query.Where(p => p.CollectionProducts.Any(cp => cp.CollectionId == collectionId.Value));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == isActive.Value);
                }

                var totalItems = await query.CountAsync();
                
                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => MapToResponseDto(p))
                    .ToListAsync();

                return new PagedResult<ProductResponseDto>
                {
                    Items = products,
                    TotalCount = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ProductMessages.Database.ConnectionFailed);
                throw new Exception(ProductMessages.Database.ConnectionFailed);
            }
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int companyId, int productId)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.CollectionProducts)
                        .ThenInclude(cp => cp.Collection)
                    .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId);

                if (product == null)
                {
                    _logger.LogWarning($"Product {productId} not found for company {companyId}");
                    return null;
                }

                return MapToResponseDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ProductMessages.Database.ConnectionFailed);
                throw new Exception(ProductMessages.Database.ConnectionFailed);
            }
        }

        public async Task<ProductResponseDto> CreateProductAsync(int companyId, CreateProductDto dto)
        {
            try
            {
                // Validar nombre único
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Name == dto.Name);
                
                if (existingProduct != null)
                {
                    throw new InvalidOperationException(ProductMessages.Validation.NameAlreadyExists);
                }

                // Validar SKU si se proporciona
                if (!string.IsNullOrWhiteSpace(dto.SKU))
                {
                    var existingSKU = await _context.Products
                        .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.SKU == dto.SKU);
                    
                    if (existingSKU != null)
                    {
                        throw new InvalidOperationException(ProductMessages.Validation.SKUAlreadyExists);
                    }
                }

                // Validar código de barras si se proporciona
                if (!string.IsNullOrWhiteSpace(dto.Barcode))
                {
                    var existingBarcode = await _context.Products
                        .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Barcode == dto.Barcode);
                    
                    if (existingBarcode != null)
                    {
                        throw new InvalidOperationException(ProductMessages.Validation.BarcodeAlreadyExists);
                    }
                }

                // Validar precios
                if (dto.ComparePrice.HasValue && dto.BasePrice.HasValue && dto.ComparePrice.Value <= dto.BasePrice.Value)
                {
                    throw new InvalidOperationException(ProductMessages.Validation.InvalidComparePrice);
                }

                // Validar imágenes
                if (dto.Images != null && dto.Images.Count > 10)
                {
                    throw new InvalidOperationException(ProductMessages.Validation.TooManyImages);
                }

                // Validar tags
                if (dto.Tags != null && dto.Tags.Count > 20)
                {
                    throw new InvalidOperationException(ProductMessages.Validation.TooManyTags);
                }

                var product = new Product
                {
                    CompanyId = companyId,
                    Name = dto.Name,
                    Description = dto.Description,
                    BasePrice = dto.BasePrice ?? 0,
                    ComparePrice = dto.ComparePrice,
                    CostPerItem = dto.CostPerItem,
                    Stock = dto.Stock ?? 0,
                    SKU = dto.SKU,
                    Barcode = dto.Barcode,
                    TrackQuantity = dto.TrackQuantity ?? true,
                    ContinueSellingWhenOutOfStock = dto.ContinueSellingWhenOutOfStock ?? false,
                    ProductType = dto.ProductType,
                    Vendor = dto.Vendor,
                    Tags = dto.Tags,
                    Images = dto.Images,
                    Weight = dto.Weight,
                    WeightUnit = dto.WeightUnit ?? "kg",
                    RequiresShipping = dto.RequiresShipping ?? true,
                    IsActive = dto.IsActive ?? true,
                    CreatedAt = DateTime.UtcNow // Ya está en UTC
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {product.Id} created successfully for company {companyId}");
                
                return MapToResponseDto(product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ProductMessages.Database.SaveFailed);
                throw new Exception(ProductMessages.Database.SaveFailed);
            }
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(int companyId, int productId, UpdateProductDto dto)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.CollectionProducts)
                    .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId);

                if (product == null)
                {
                    _logger.LogWarning($"Product {productId} not found for update");
                    return null;
                }

                // Validar nombre único si cambió
                if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != product.Name)
                {
                    var existingProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Name == dto.Name && p.Id != productId);
                    
                    if (existingProduct != null)
                    {
                        throw new InvalidOperationException(ProductMessages.Validation.NameAlreadyExists);
                    }
                }

                // Validar SKU si cambió
                if (!string.IsNullOrWhiteSpace(dto.SKU) && dto.SKU != product.SKU)
                {
                    var existingSKU = await _context.Products
                        .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.SKU == dto.SKU && p.Id != productId);
                    
                    if (existingSKU != null)
                    {
                        throw new InvalidOperationException(ProductMessages.Validation.SKUAlreadyExists);
                    }
                }

                // Validar precios
                var newBasePrice = dto.BasePrice ?? product.BasePrice;
                var newComparePrice = dto.ComparePrice ?? product.ComparePrice;
                if (newComparePrice.HasValue && newComparePrice.Value <= newBasePrice)
                {
                    throw new InvalidOperationException(ProductMessages.Validation.InvalidComparePrice);
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name;
                if (dto.Description != null) product.Description = dto.Description;
                if (dto.BasePrice.HasValue) product.BasePrice = dto.BasePrice.Value;
                if (dto.ComparePrice != null) product.ComparePrice = dto.ComparePrice;
                if (dto.CostPerItem != null) product.CostPerItem = dto.CostPerItem;
                if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
                if (dto.SKU != null) product.SKU = dto.SKU;
                if (dto.Barcode != null) product.Barcode = dto.Barcode;
                if (dto.TrackQuantity.HasValue) product.TrackQuantity = dto.TrackQuantity.Value;
                if (dto.ContinueSellingWhenOutOfStock.HasValue) product.ContinueSellingWhenOutOfStock = dto.ContinueSellingWhenOutOfStock.Value;
                if (dto.ProductType != null) product.ProductType = dto.ProductType;
                if (dto.Vendor != null) product.Vendor = dto.Vendor;
                if (dto.Tags != null) product.Tags = dto.Tags;
                if (dto.Images != null) product.Images = dto.Images;
                if (dto.Weight != null) product.Weight = dto.Weight;
                if (dto.WeightUnit != null) product.WeightUnit = dto.WeightUnit;
                if (dto.RequiresShipping.HasValue) product.RequiresShipping = dto.RequiresShipping.Value;
                if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;
                
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {productId} updated successfully");
                
                return MapToResponseDto(product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError(ProductMessages.Database.ConcurrencyError);
                throw new Exception(ProductMessages.Database.ConcurrencyError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ProductMessages.Database.UpdateFailed);
                throw new Exception(ProductMessages.Database.UpdateFailed);
            }
        }

        public async Task<bool> DeleteProductAsync(int companyId, int productId)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId);

                if (product == null)
                {
                    _logger.LogWarning($"Product {productId} not found for deletion");
                    return false;
                }

                // TODO: Verificar si el producto tiene pedidos antes de eliminar
                // if (await HasOrders(productId))
                // {
                //     throw new InvalidOperationException(ProductMessages.Business.ProductHasOrders);
                // }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {productId} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ProductMessages.Database.DeleteFailed);
                throw new Exception(ProductMessages.Database.DeleteFailed);
            }
        }

        public async Task<bool> BulkDeleteProductsAsync(int companyId, List<int> productIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var products = await _context.Products
                    .Where(p => p.CompanyId == companyId && productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    throw new InvalidOperationException(ProductMessages.Business.ProductsNotFound);
                }

                _context.Products.RemoveRange(products);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Bulk deleted {products.Count} products for company {companyId}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, ProductMessages.Database.TransactionFailed);
                throw new Exception(ProductMessages.Database.TransactionFailed);
            }
        }

        public async Task<bool> ToggleActiveStatusAsync(int companyId, int productId)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId);

                if (product == null)
                {
                    return false;
                }

                // Si se va a desactivar y tiene stock, advertir
                if (product.IsActive && product.Stock > 0 && product.TrackQuantity)
                {
                    _logger.LogWarning(ProductMessages.Business.CannotDeactivateWithStock);
                }

                product.IsActive = !product.IsActive;
                product.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Product {productId} status toggled to {product.IsActive}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ProductMessages.Database.UpdateFailed);
                throw new Exception(ProductMessages.Database.UpdateFailed);
            }
        }

        public async Task<bool> UpdateProductImagesAsync(int companyId, int productId, List<string> imageUrls)
        {
            try
            {
                if (imageUrls.Count > 10)
                {
                    throw new InvalidOperationException(ProductMessages.Validation.TooManyImages);
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId);

                if (product == null)
                {
                    return false;
                }

                product.Images = imageUrls;
                product.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Updated images for product {productId}");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ProductMessages.FileOperation.ImageUploadFailed);
                throw new Exception(ProductMessages.FileOperation.ImageUploadFailed);
            }
        }

        public async Task<bool> AssignToCollectionsAsync(int companyId, int productId, List<int> collectionIds)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.CollectionProducts)
                    .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId);

                if (product == null)
                {
                    return false;
                }

                // Verificar que todas las colecciones existen
                var collections = await _context.Collections
                    .Where(c => c.CompanyId == companyId && collectionIds.Contains(c.Id))
                    .ToListAsync();

                if (collections.Count != collectionIds.Count)
                {
                    throw new InvalidOperationException(ProductMessages.Business.CollectionNotFound);
                }

                // Eliminar asignaciones existentes
                _context.CollectionProducts.RemoveRange(product.CollectionProducts);

                // Crear nuevas asignaciones
                foreach (var collectionId in collectionIds)
                {
                    _context.CollectionProducts.Add(new CollectionProduct
                    {
                        ProductId = productId,
                        CollectionId = collectionId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Product {productId} assigned to {collectionIds.Count} collections");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, ProductMessages.Database.TransactionFailed);
                throw new Exception(ProductMessages.Database.TransactionFailed);
            }
        }

        private static ProductResponseDto MapToResponseDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                CompanyId = product.CompanyId,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                ComparePrice = product.ComparePrice,
                CostPerItem = product.CostPerItem,
                Stock = product.Stock,
                SKU = product.SKU,
                Barcode = product.Barcode,
                TrackQuantity = product.TrackQuantity,
                ContinueSellingWhenOutOfStock = product.ContinueSellingWhenOutOfStock,
                ProductType = product.ProductType,
                Vendor = product.Vendor,
                Tags = product.Tags,
                Images = product.Images,
                Weight = product.Weight,
                WeightUnit = product.WeightUnit,
                RequiresShipping = product.RequiresShipping,
                HasVariants = product.HasVariants,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Variants = product.Variants?.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    ComparePrice = v.ComparePrice,
                    Stock = v.Stock,
                    SKU = v.SKU,
                    Barcode = v.Barcode,
                    ImageUrl = v.ImageUrl,
                    Attributes = v.GetAttributes(),
                    IsActive = v.IsActive
                }).ToList(),
                Collections = product.CollectionProducts?.Select(cp => cp.Collection?.Title ?? "").ToList()
            };
        }
    }
}