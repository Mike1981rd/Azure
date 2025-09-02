using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Collections;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ApplicationDbContext _context;

        public CollectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<CollectionListDto>> GetPagedAsync(int companyId, int page, int pageSize, string? search = null, bool? isActive = null)
        {
            var query = _context.Collections
                .Where(c => c.CompanyId == companyId && c.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => 
                    c.Title.ToLower().Contains(search.ToLower()) ||
                    c.Handle.ToLower().Contains(search.ToLower()) ||
                    (c.Description != null && c.Description.ToLower().Contains(search.ToLower())));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var collections = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CollectionListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Handle = c.Handle,
                    Image = c.Image,
                    IsActive = c.IsActive,
                    ProductCount = c.ProductCount,
                    ConditionsSummary = "Todas las condiciones",
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return new PagedResult<CollectionListDto>
            {
                Items = collections,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalItems
            };
        }

        public async Task<CollectionDto?> GetByIdAsync(int companyId, int collectionId)
        {
            var collection = await _context.Collections
                .Where(c => c.Id == collectionId && c.CompanyId == companyId && c.DeletedAt == null)
                .Select(c => new CollectionDto
                {
                    Id = c.Id,
                    CompanyId = c.CompanyId,
                    Title = c.Title,
                    Description = c.Description,
                    Handle = c.Handle,
                    IsActive = c.IsActive,
                    Image = c.Image,
                    OnlineStore = c.OnlineStore,
                    PointOfSale = c.PointOfSale,
                    Facebook = c.Facebook,
                    Instagram = c.Instagram,
                    TikTok = c.TikTok,
                    WhatsAppBusiness = c.WhatsAppBusiness,
                    SeoTitle = c.SeoTitle,
                    SeoDescription = c.SeoDescription,
                    SeoKeywords = c.SeoKeywords,
                    PublishToSearchEngines = c.PublishToSearchEngines,
                    SortOrder = c.SortOrder,
                    ProductCount = c.ProductCount,
                    ConditionsSummary = "Todas las condiciones",
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return collection;
        }

        public async Task<CollectionDto?> GetByHandleAsync(int companyId, string handle)
        {
            var collection = await _context.Collections
                .Where(c => c.Handle == handle && c.CompanyId == companyId && c.DeletedAt == null)
                .Select(c => new CollectionDto
                {
                    Id = c.Id,
                    CompanyId = c.CompanyId,
                    Title = c.Title,
                    Description = c.Description,
                    Handle = c.Handle,
                    IsActive = c.IsActive,
                    Image = c.Image,
                    OnlineStore = c.OnlineStore,
                    PointOfSale = c.PointOfSale,
                    Facebook = c.Facebook,
                    Instagram = c.Instagram,
                    TikTok = c.TikTok,
                    WhatsAppBusiness = c.WhatsAppBusiness,
                    SeoTitle = c.SeoTitle,
                    SeoDescription = c.SeoDescription,
                    SeoKeywords = c.SeoKeywords,
                    PublishToSearchEngines = c.PublishToSearchEngines,
                    SortOrder = c.SortOrder,
                    ProductCount = c.ProductCount,
                    ConditionsSummary = "Todas las condiciones",
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return collection;
        }

        public async Task<CollectionDto> CreateAsync(int companyId, CreateCollectionDto dto)
        {
            try
            {
                // Generate handle if not provided
                var handle = !string.IsNullOrWhiteSpace(dto.Handle) 
                    ? dto.Handle 
                    : Collection.GenerateHandle(dto.Title);

                // Ensure handle is unique
                var counter = 1;
                var originalHandle = handle;
                while (await HandleExistsAsync(companyId, handle))
                {
                    handle = $"{originalHandle}-{counter}";
                    counter++;
                }

                var collection = new Collection
                {
                    CompanyId = companyId,
                    Title = dto.Title,
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
                    Handle = handle,
                    IsActive = dto.IsActive ?? true,  // Default to active
                    Image = string.IsNullOrWhiteSpace(dto.Image) ? null : dto.Image,
                    OnlineStore = dto.OnlineStore ?? true,  // Default to true
                    PointOfSale = dto.PointOfSale ?? false,
                    Facebook = dto.Facebook ?? false,
                    Instagram = dto.Instagram ?? false,
                    TikTok = dto.TikTok ?? false,
                    WhatsAppBusiness = dto.WhatsAppBusiness ?? false,
                    SeoTitle = string.IsNullOrWhiteSpace(dto.SeoTitle) ? dto.Title : dto.SeoTitle,
                    SeoDescription = string.IsNullOrWhiteSpace(dto.SeoDescription) ? null : dto.SeoDescription,
                    SeoKeywords = string.IsNullOrWhiteSpace(dto.SeoKeywords) ? null : dto.SeoKeywords,
                    PublishToSearchEngines = dto.PublishToSearchEngines ?? true,  // Default to true
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Collections.Add(collection);
                await _context.SaveChangesAsync();

                // Add products if provided (skip if no products)
                if (dto.ProductIds != null && dto.ProductIds.Any())
                {
                    try
                    {
                        await AddProductsAsync(companyId, collection.Id, dto.ProductIds);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue - collection is already created
                        Console.WriteLine($"Warning: Could not add products to collection: {ex.Message}");
                    }
                }

                return await GetByIdAsync(companyId, collection.Id) ?? throw new InvalidOperationException("Failed to retrieve created collection");
            }
            catch (DbUpdateException dbEx)
            {
                // Check for specific database errors
                if (dbEx.InnerException != null)
                {
                    throw new InvalidOperationException($"Database error: {dbEx.InnerException.Message}", dbEx);
                }
                throw new InvalidOperationException($"Error saving collection: {dbEx.Message}", dbEx);
            }
        }

        public async Task<CollectionDto?> UpdateAsync(int companyId, int collectionId, UpdateCollectionDto dto)
        {
            var collection = await _context.Collections
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.CompanyId == companyId && c.DeletedAt == null);

            if (collection == null)
                return null;

            // Update fields if provided - handle empty strings properly
            if (!string.IsNullOrWhiteSpace(dto.Title))
                collection.Title = dto.Title;

            // For nullable fields, handle empty strings as null
            if (dto.Description != null)
                collection.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Handle))
            {
                // Ensure new handle is unique
                if (await HandleExistsAsync(companyId, dto.Handle, collectionId))
                    throw new InvalidOperationException($"Handle '{dto.Handle}' already exists");
                collection.Handle = dto.Handle;
            }

            if (dto.IsActive.HasValue)
                collection.IsActive = dto.IsActive.Value;

            if (dto.Image != null)
                collection.Image = string.IsNullOrWhiteSpace(dto.Image) ? null : dto.Image;

            if (dto.OnlineStore.HasValue)
                collection.OnlineStore = dto.OnlineStore.Value;

            if (dto.PointOfSale.HasValue)
                collection.PointOfSale = dto.PointOfSale.Value;

            if (dto.Facebook.HasValue)
                collection.Facebook = dto.Facebook.Value;

            if (dto.Instagram.HasValue)
                collection.Instagram = dto.Instagram.Value;

            if (dto.TikTok.HasValue)
                collection.TikTok = dto.TikTok.Value;

            if (dto.WhatsAppBusiness.HasValue)
                collection.WhatsAppBusiness = dto.WhatsAppBusiness.Value;

            if (dto.SeoTitle != null)
                collection.SeoTitle = string.IsNullOrWhiteSpace(dto.SeoTitle) ? null : dto.SeoTitle;

            if (dto.SeoDescription != null)
                collection.SeoDescription = string.IsNullOrWhiteSpace(dto.SeoDescription) ? null : dto.SeoDescription;

            if (dto.SeoKeywords != null)
                collection.SeoKeywords = string.IsNullOrWhiteSpace(dto.SeoKeywords) ? null : dto.SeoKeywords;

            if (dto.PublishToSearchEngines.HasValue)
                collection.PublishToSearchEngines = dto.PublishToSearchEngines.Value;

            if (dto.SortOrder.HasValue)
                collection.SortOrder = dto.SortOrder.Value;

            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update products if provided
            if (dto.ProductIds != null)
            {
                // Remove existing product associations
                var existingAssociations = await _context.CollectionProducts
                    .Where(cp => cp.CollectionId == collectionId)
                    .ToListAsync();
                _context.CollectionProducts.RemoveRange(existingAssociations);

                // Add new product associations
                if (dto.ProductIds.Any())
                {
                    var newAssociations = dto.ProductIds.Select((productId, index) => new CollectionProduct
                    {
                        CollectionId = collectionId,
                        ProductId = productId,
                        Position = index
                    });
                    _context.CollectionProducts.AddRange(newAssociations);
                }

                // Update product count
                collection.ProductCount = dto.ProductIds.Count;
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(companyId, collectionId);
        }

        public async Task<bool> DeleteAsync(int companyId, int collectionId)
        {
            var collection = await _context.Collections
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.CompanyId == companyId && c.DeletedAt == null);

            if (collection == null)
                return false;

            // Soft delete
            collection.DeletedAt = DateTime.UtcNow;
            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkDeleteAsync(int companyId, List<int> collectionIds)
        {
            var collections = await _context.Collections
                .Where(c => collectionIds.Contains(c.Id) && c.CompanyId == companyId && c.DeletedAt == null)
                .ToListAsync();

            if (!collections.Any())
                return false;

            var now = DateTime.UtcNow;
            foreach (var collection in collections)
            {
                collection.DeletedAt = now;
                collection.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddProductsAsync(int companyId, int collectionId, List<int> productIds)
        {
            var collection = await _context.Collections
                .Include(c => c.CollectionProducts)
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.CompanyId == companyId && c.DeletedAt == null);

            if (collection == null)
                return false;

            // Get existing product IDs in collection
            var existingProductIds = collection.CollectionProducts.Select(cp => cp.ProductId).ToHashSet();

            // Get the max position
            var maxPosition = collection.CollectionProducts.Any() 
                ? collection.CollectionProducts.Max(cp => cp.Position) 
                : 0;

            // Add only new products
            foreach (var productId in productIds.Where(id => !existingProductIds.Contains(id)))
            {
                // Verify product exists and belongs to same company
                var productExists = await _context.Products
                    .AnyAsync(p => p.Id == productId && p.CompanyId == companyId);

                if (!productExists)
                    continue;

                collection.CollectionProducts.Add(new CollectionProduct
                {
                    CollectionId = collectionId,
                    ProductId = productId,
                    Position = ++maxPosition,
                    AddedAt = DateTime.UtcNow
                });
            }

            // Update product count
            collection.ProductCount = collection.CollectionProducts.Count;
            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveProductsAsync(int companyId, int collectionId, List<int> productIds)
        {
            var collection = await _context.Collections
                .Include(c => c.CollectionProducts)
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.CompanyId == companyId && c.DeletedAt == null);

            if (collection == null)
                return false;

            var productsToRemove = collection.CollectionProducts
                .Where(cp => productIds.Contains(cp.ProductId))
                .ToList();

            if (!productsToRemove.Any())
                return false;

            _context.CollectionProducts.RemoveRange(productsToRemove);

            // Update product count
            collection.ProductCount = collection.CollectionProducts.Count - productsToRemove.Count;
            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProductPositionsAsync(int companyId, int collectionId, BulkUpdateProductPositionsDto dto)
        {
            var collection = await _context.Collections
                .Include(c => c.CollectionProducts)
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.CompanyId == companyId && c.DeletedAt == null);

            if (collection == null)
                return false;

            foreach (var position in dto.Positions)
            {
                var collectionProduct = collection.CollectionProducts
                    .FirstOrDefault(cp => cp.ProductId == position.ProductId);

                if (collectionProduct != null)
                {
                    collectionProduct.Position = position.Position;
                }
            }

            collection.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CollectionProductDto>> GetCollectionProductsAsync(int companyId, int collectionId)
        {
            var products = await _context.CollectionProducts
                .Include(cp => cp.Product)
                .Where(cp => cp.CollectionId == collectionId && cp.Collection.CompanyId == companyId)
                .OrderBy(cp => cp.Position)
                .Select(cp => new CollectionProductDto
                {
                    Id = cp.Product.Id,
                    Name = cp.Product.Name,
                    BasePrice = cp.Product.BasePrice,
                    Images = cp.Product.Images,
                    Stock = cp.Product.Stock,
                    Sku = null, // Product model doesn't have Sku field yet
                    IsActive = cp.Product.IsActive
                })
                .ToListAsync();

            return products;
        }

        public async Task<bool> HandleExistsAsync(int companyId, string handle, int? excludeId = null)
        {
            var query = _context.Collections
                .Where(c => c.CompanyId == companyId && 
                       c.Handle == handle && 
                       c.DeletedAt == null);  // This already excludes soft-deleted items

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> UpdateProductCountsAsync(int companyId)
        {
            var collections = await _context.Collections
                .Include(c => c.CollectionProducts)
                .Where(c => c.CompanyId == companyId && c.DeletedAt == null)
                .ToListAsync();

            foreach (var collection in collections)
            {
                collection.ProductCount = collection.CollectionProducts.Count;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}