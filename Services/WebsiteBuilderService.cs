using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service implementation for website builder operations
    /// </summary>
    public class WebsiteBuilderService : IWebsiteBuilderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WebsiteBuilderService> _logger;
        private readonly IWebsiteBuilderCacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly bool _autoGenerateSnapshots;

        public WebsiteBuilderService(
            ApplicationDbContext context,
            ILogger<WebsiteBuilderService> logger,
            IWebsiteBuilderCacheService cacheService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
            _configuration = configuration;
            _autoGenerateSnapshots = _configuration.GetValue<bool>("WebsiteBuilder:Snapshot:AutoGenerate", true);
            
            // Log initialization for debugging
            _logger.LogInformation("[SNAPSHOT-INIT] WebsiteBuilderService initialized with AutoGenerateSnapshots: {AutoGenerate}", 
                _autoGenerateSnapshots);
        }

        #region Page Operations

        public async Task<List<WebsitePageDto>> GetPagesByCompanyIdAsync(int companyId)
        {
            var pages = await _context.WebsitePages
                .Where(p => p.CompanyId == companyId)
                .Include(p => p.Sections)
                    .ThenInclude(s => s.Children)
                .OrderBy(p => p.PageType)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return pages.Select(MapToPageDto).ToList();
        }

        public async Task<WebsitePageDto?> GetPageByIdAsync(int pageId)
        {
            var page = await _context.WebsitePages
                .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Children.OrderBy(c => c.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == pageId);

            return page != null ? MapToPageDto(page) : null;
        }

        public async Task<WebsitePageDto?> GetPageBySlugAsync(int companyId, string slug)
        {
            var page = await _context.WebsitePages
                .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                    .ThenInclude(s => s.Children.OrderBy(c => c.SortOrder))
                .FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Slug == slug);

            return page != null ? MapToPageDto(page) : null;
        }

        public async Task<WebsitePageDto> CreatePageAsync(int companyId, CreateWebsitePageDto dto)
        {
            // Validate page type
            if (!PageTypes.IsValidPageType(dto.PageType))
            {
                throw new ArgumentException($"Invalid page type: {dto.PageType}");
            }

            // Check if company can create this page type
            if (!await CanCreatePageTypeAsync(companyId, dto.PageType))
            {
                throw new InvalidOperationException($"Cannot create another {dto.PageType} page for this company");
            }

            var page = new WebsitePage
            {
                CompanyId = companyId,
                PageType = dto.PageType,
                Name = dto.Name,
                Slug = dto.Slug ?? GenerateSlug(dto.Name),
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                IsActive = true,
                IsPublished = false,
                TemplateId = dto.TemplateId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WebsitePages.Add(page);
            await _context.SaveChangesAsync();

            // Add default sections based on page type
            await AddDefaultSectionsAsync(page.Id, dto.PageType);

            // Reload with sections
            page = await _context.WebsitePages
                .Include(p => p.Sections)
                    .ThenInclude(s => s.Children)
                .FirstAsync(p => p.Id == page.Id);

            _logger.LogInformation("Created page {PageId} of type {PageType} for company {CompanyId}", 
                page.Id, page.PageType, companyId);

            return MapToPageDto(page);
        }

        public async Task<WebsitePageDto?> UpdatePageAsync(int pageId, UpdateWebsitePageDto dto)
        {
            var page = await _context.WebsitePages.FindAsync(pageId);
            if (page == null) return null;

            page.Name = dto.Name ?? page.Name;
            page.Slug = dto.Slug ?? page.Slug;
            page.MetaTitle = dto.MetaTitle ?? page.MetaTitle;
            page.MetaDescription = dto.MetaDescription ?? page.MetaDescription;
            page.IsActive = dto.IsActive ?? page.IsActive;
            page.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cacheService.InvalidatePageCacheAsync(pageId);
            
            // Generate snapshot automatically if enabled
            // TODO: restore Task.Run after debug - currently running inline to catch errors
            if (_autoGenerateSnapshots)
            {
                try
                {
                    _logger.LogInformation("[DEBUG] About to generate snapshot inline for page {PageId}", pageId);
                    await GenerateSnapshotAsync(pageId, page.CompanyId);
                    _logger.LogInformation("[DEBUG] Auto-generated snapshot inline for page {PageId}", pageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DEBUG] Failed to auto-generate snapshot inline for page {PageId}", pageId);
                    throw; // Re-throw to see error in response during debug
                }
            }

            _logger.LogInformation("Updated page {PageId}", pageId);

            return await GetPageByIdAsync(pageId);
        }

        public async Task<WebsitePageDto?> ReplacePageSectionsAsync(int pageId, UpdatePageSectionsDto dto)
        {
            var page = await _context.WebsitePages
                .Include(p => p.Sections)
                .FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null) return null;

            // Remove existing sections
            var existingSections = await _context.PageSections.Where(s => s.PageId == pageId).ToListAsync();
            _context.PageSections.RemoveRange(existingSections);
            await _context.SaveChangesAsync();

            // Add new sections from editor payload
            int sortOrderBase = 10;
            foreach (var incoming in dto.Sections.OrderBy(s => s.SortOrder))
            {
                // Map editor type (snake_case) to backend constant if necessary
                var normalized = (incoming.Type ?? string.Empty).Trim();
                string sectionType = normalized switch
                {
                    "image_banner" => SectionTypes.IMAGE_BANNER,
                    "image_with_text" => SectionTypes.IMAGE_WITH_TEXT,
                    "rich_text" => SectionTypes.RICH_TEXT,
                    "gallery" => SectionTypes.GALLERY,
                    "contact_form" => SectionTypes.CONTACT_FORM,
                    "newsletter" => SectionTypes.NEWSLETTER,
                    "featured_product" => SectionTypes.FEATURED_PRODUCT,
                    "featured_collection" => SectionTypes.FEATURED_COLLECTION,
                    "testimonials" => SectionTypes.TESTIMONIALS,
                    "faq" => SectionTypes.FAQ,
                    "videos" => SectionTypes.VIDEOS,
                    "slideshow" => SectionTypes.SLIDESHOW,
                    _ => normalized // Allow already-normalized values like ImageBanner
                };

                var configJson = incoming.Settings != null 
                    ? JsonSerializer.Serialize(incoming.Settings)
                    : "{}";

                var newSection = new PageSection
                {
                    PageId = pageId,
                    SectionType = sectionType,
                    Config = configJson,
                    SortOrder = sortOrderBase + incoming.SortOrder,
                    IsActive = incoming.Visible,
                    Title = incoming.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PageSections.Add(newSection);
            }

            await _context.SaveChangesAsync();

            // Invalidate page cache so preview pulls fresh data
            await _cacheService.InvalidatePageCacheAsync(pageId);
            
            // Generate snapshot automatically if enabled
            // TODO: restore Task.Run after debug - currently running inline to catch errors
            if (_autoGenerateSnapshots)
            {
                try
                {
                    _logger.LogInformation("[DEBUG] About to generate snapshot inline for page {PageId}", pageId);
                    await GenerateSnapshotAsync(pageId, page.CompanyId);
                    _logger.LogInformation("[DEBUG] Auto-generated snapshot inline for page {PageId}", pageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DEBUG] Failed to auto-generate snapshot inline for page {PageId}", pageId);
                    throw; // Re-throw to see error in response during debug
                }
            }

            return await GetPageByIdAsync(pageId);
        }

        public async Task<bool> DeletePageAsync(int pageId)
        {
            var page = await _context.WebsitePages
                .Include(p => p.Sections)
                    .ThenInclude(s => s.Children)
                .FirstOrDefaultAsync(p => p.Id == pageId);

            if (page == null) return false;

            // Don't allow deletion of system pages
            if (PageTypes.IsSystemPage(page.PageType))
            {
                throw new InvalidOperationException($"Cannot delete system page of type {page.PageType}");
            }

            _context.WebsitePages.Remove(page);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cacheService.InvalidatePageCacheAsync(pageId);

            _logger.LogInformation("Deleted page {PageId}", pageId);

            return true;
        }

        public async Task<WebsitePageDto?> DuplicatePageAsync(int pageId, string? newName = null)
        {
            var originalPage = await _context.WebsitePages
                .Include(p => p.Sections)
                    .ThenInclude(s => s.Children)
                .FirstOrDefaultAsync(p => p.Id == pageId);

            if (originalPage == null) return null;

            var duplicatedPage = new WebsitePage
            {
                CompanyId = originalPage.CompanyId,
                PageType = originalPage.PageType,
                Name = newName ?? $"{originalPage.Name} (Copy)",
                Slug = GenerateSlug(newName ?? $"{originalPage.Name}-copy"),
                MetaTitle = originalPage.MetaTitle,
                MetaDescription = originalPage.MetaDescription,
                IsActive = true,
                IsPublished = false,
                TemplateId = originalPage.TemplateId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WebsitePages.Add(duplicatedPage);
            await _context.SaveChangesAsync();

            // Duplicate sections
            foreach (var section in originalPage.Sections)
            {
                var duplicatedSection = new PageSection
                {
                    PageId = duplicatedPage.Id,
                    SectionType = section.SectionType,
                    Config = section.Config,
                    ThemeOverrides = section.ThemeOverrides,
                    SortOrder = section.SortOrder,
                    IsActive = section.IsActive,
                    CustomCssClass = section.CustomCssClass,
                    Title = section.Title,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PageSections.Add(duplicatedSection);
                await _context.SaveChangesAsync();

                // Duplicate children if any
                foreach (var child in section.Children)
                {
                    var duplicatedChild = new PageSectionChild
                    {
                        PageSectionId = duplicatedSection.Id,
                        BlockType = child.BlockType,
                        Settings = child.Settings,
                        SortOrder = child.SortOrder,
                        IsActive = child.IsActive,
                        CustomCssClass = child.CustomCssClass,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.PageSectionChildren.Add(duplicatedChild);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Duplicated page {OriginalPageId} to {NewPageId}", 
                pageId, duplicatedPage.Id);

            return await GetPageByIdAsync(duplicatedPage.Id);
        }

        public async Task<WebsitePageDto?> PublishPageAsync(int pageId, PublishWebsitePageDto dto)
        {
            var page = await _context.WebsitePages.FindAsync(pageId);
            if (page == null) return null;

            // Validate page before publishing
            if (!await ValidatePageAsync(pageId))
            {
                throw new InvalidOperationException("Page validation failed. Cannot publish.");
            }

            page.IsPublished = true;
            page.PublishedAt = DateTime.UtcNow;
            page.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear production cache
            await _cacheService.InvalidatePageCacheAsync(pageId);
            await _cacheService.InvalidateCompanyCacheAsync(page.CompanyId);
            
            // Generate initial snapshot for published page
            await GenerateSnapshotAsync(pageId, page.CompanyId);

            _logger.LogInformation("Published page {PageId}", pageId);

            return await GetPageByIdAsync(pageId);
        }

        public async Task<bool> ValidatePageAsync(int pageId)
        {
            var page = await _context.WebsitePages
                .Include(p => p.Sections)
                .FirstOrDefaultAsync(p => p.Id == pageId);

            if (page == null) return false;

            // Basic validation
            if (string.IsNullOrEmpty(page.Name)) return false;

            // Page-specific validation
            switch (page.PageType)
            {
                case PageTypes.PRODUCT:
                    // Must have PRODUCT_INFORMATION section
                    return page.Sections.Any(s => s.SectionType == SectionTypes.PRODUCT_INFORMATION);
                
                case PageTypes.COLLECTION:
                case PageTypes.ALL_PRODUCTS:
                    // Must have PRODUCTS section
                    return page.Sections.Any(s => s.SectionType == SectionTypes.PRODUCTS);
                
                case PageTypes.ALL_COLLECTIONS:
                    // Must have COLLECTIONS_LIST section
                    return page.Sections.Any(s => s.SectionType == SectionTypes.COLLECTIONS_LIST);
                
                case PageTypes.CART:
                    // Must have CART section
                    return page.Sections.Any(s => s.SectionType == SectionTypes.CART);
                
                default:
                    // HOME and CUSTOM pages can be empty
                    return true;
            }
        }

        #endregion

        #region Section Operations

        public async Task<List<PageSectionDto>> GetSectionsByPageIdAsync(int pageId)
        {
            var sections = await _context.PageSections
                .Where(s => s.PageId == pageId)
                .Include(s => s.Children)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return sections.Select(MapToSectionDto).ToList();
        }

        public async Task<PageSectionDto?> GetSectionByIdAsync(int sectionId)
        {
            var section = await _context.PageSections
                .Include(s => s.Children.OrderBy(c => c.SortOrder))
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            return section != null ? MapToSectionDto(section) : null;
        }

        public async Task<PageSectionDto> CreateSectionAsync(int pageId, CreatePageSectionDto dto)
        {
            // Validate section type
            if (!SectionTypes.IsValidSectionType(dto.SectionType))
            {
                throw new ArgumentException($"Invalid section type: {dto.SectionType}");
            }

            // Get max sort order
            var maxSortOrder = await _context.PageSections
                .Where(s => s.PageId == pageId)
                .MaxAsync(s => (int?)s.SortOrder) ?? 0;

            var section = new PageSection
            {
                PageId = pageId,
                SectionType = dto.SectionType,
                Config = dto.Config ?? GetDefaultSectionConfig(dto.SectionType),
                ThemeOverrides = dto.ThemeOverrides,
                SortOrder = dto.SortOrder ?? (maxSortOrder + 10),
                IsActive = dto.IsActive ?? true,
                CustomCssClass = dto.CustomCssClass,
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PageSections.Add(section);
            await _context.SaveChangesAsync();

            // Add default children for PRODUCT_INFORMATION
            if (dto.SectionType == SectionTypes.PRODUCT_INFORMATION)
            {
                await AddDefaultProductInfoBlocksAsync(section.Id);
            }

            // Reload with children
            section = await _context.PageSections
                .Include(s => s.Children)
                .FirstAsync(s => s.Id == section.Id);

            // Invalidate page cache
            await _cacheService.InvalidatePageCacheAsync(pageId);

            _logger.LogInformation("Created section {SectionId} of type {SectionType} for page {PageId}", 
                section.Id, section.SectionType, pageId);

            return MapToSectionDto(section);
        }

        public async Task<PageSectionDto?> UpdateSectionAsync(int sectionId, UpdatePageSectionDto dto)
        {
            var section = await _context.PageSections.FindAsync(sectionId);
            if (section == null) return null;

            section.Config = dto.Config ?? section.Config;
            section.ThemeOverrides = dto.ThemeOverrides ?? section.ThemeOverrides;
            section.SortOrder = dto.SortOrder ?? section.SortOrder;
            section.IsActive = dto.IsActive ?? section.IsActive;
            section.CustomCssClass = dto.CustomCssClass ?? section.CustomCssClass;
            section.Title = dto.Title ?? section.Title;
            section.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate page cache
            await _cacheService.InvalidatePageCacheAsync(section.PageId);

            _logger.LogInformation("Updated section {SectionId}", sectionId);

            return await GetSectionByIdAsync(sectionId);
        }

        public async Task<bool> DeleteSectionAsync(int sectionId)
        {
            var section = await _context.PageSections
                .Include(s => s.Children)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) return false;

            // Check if this is a required section
            var page = await _context.WebsitePages.FindAsync(section.PageId);
            if (page != null && IsRequiredSection(page.PageType, section.SectionType))
            {
                throw new InvalidOperationException($"Cannot delete required section {section.SectionType} from {page.PageType} page");
            }

            _context.PageSections.Remove(section);
            await _context.SaveChangesAsync();

            // Invalidate page cache
            await _cacheService.InvalidatePageCacheAsync(section.PageId);

            _logger.LogInformation("Deleted section {SectionId}", sectionId);

            return true;
        }

        public async Task<PageSectionDto?> DuplicateSectionAsync(int sectionId, DuplicateSectionDto dto)
        {
            var originalSection = await _context.PageSections
                .Include(s => s.Children)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (originalSection == null) return null;

            var targetPageId = dto.TargetPageId ?? originalSection.PageId;

            // Get max sort order in target page
            var maxSortOrder = await _context.PageSections
                .Where(s => s.PageId == targetPageId)
                .MaxAsync(s => (int?)s.SortOrder) ?? 0;

            var duplicatedSection = new PageSection
            {
                PageId = targetPageId,
                SectionType = originalSection.SectionType,
                Config = originalSection.Config,
                ThemeOverrides = originalSection.ThemeOverrides,
                SortOrder = dto.SortOrder ?? (maxSortOrder + 10),
                IsActive = originalSection.IsActive,
                CustomCssClass = originalSection.CustomCssClass,
                Title = dto.NewTitle ?? originalSection.Title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PageSections.Add(duplicatedSection);
            await _context.SaveChangesAsync();

            // Duplicate children
            foreach (var child in originalSection.Children)
            {
                var duplicatedChild = new PageSectionChild
                {
                    PageSectionId = duplicatedSection.Id,
                    BlockType = child.BlockType,
                    Settings = child.Settings,
                    SortOrder = child.SortOrder,
                    IsActive = child.IsActive,
                    CustomCssClass = child.CustomCssClass,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PageSectionChildren.Add(duplicatedChild);
            }

            await _context.SaveChangesAsync();

            // Invalidate page cache
            await _cacheService.InvalidatePageCacheAsync(targetPageId);

            _logger.LogInformation("Duplicated section {OriginalSectionId} to {NewSectionId}", 
                sectionId, duplicatedSection.Id);

            return await GetSectionByIdAsync(duplicatedSection.Id);
        }

        public async Task<bool> ReorderSectionsAsync(int pageId, ReorderSectionsDto dto)
        {
            var sections = await _context.PageSections
                .Where(s => s.PageId == pageId)
                .ToListAsync();

            foreach (var reorder in dto.SectionOrders)
            {
                var section = sections.FirstOrDefault(s => s.Id == reorder.SectionId);
                if (section != null)
                {
                    section.SortOrder = reorder.SortOrder;
                    section.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            // Invalidate page cache
            await _cacheService.InvalidatePageCacheAsync(pageId);

            _logger.LogInformation("Reordered sections for page {PageId}", pageId);

            return true;
        }

        #endregion

        #region Template Operations

        public async Task<List<WebsitePageDto>> GetPageTemplatesAsync(string? pageType = null)
        {
            var query = _context.WebsitePages
                .Where(p => p.TemplateId != null)
                .Include(p => p.Sections)
                    .ThenInclude(s => s.Children)
                .AsQueryable();

            if (!string.IsNullOrEmpty(pageType))
            {
                query = query.Where(p => p.PageType == pageType);
            }

            var templates = await query.ToListAsync();

            return templates.Select(MapToPageDto).ToList();
        }

        public async Task<WebsitePageDto> CreatePageFromTemplateAsync(int companyId, int templateId, string name)
        {
            var template = await _context.WebsitePages
                .Include(p => p.Sections)
                    .ThenInclude(s => s.Children)
                .FirstOrDefaultAsync(p => p.Id == templateId);

            if (template == null)
            {
                throw new ArgumentException($"Template {templateId} not found");
            }

            // Create page from template
            var dto = new CreateWebsitePageDto
            {
                PageType = template.PageType,
                Name = name,
                Slug = GenerateSlug(name),
                MetaTitle = template.MetaTitle,
                MetaDescription = template.MetaDescription,
                TemplateId = templateId
            };

            var newPage = await CreatePageAsync(companyId, dto);

            // Copy sections from template
            foreach (var templateSection in template.Sections)
            {
                var sectionDto = new CreatePageSectionDto
                {
                    SectionType = templateSection.SectionType,
                    Config = templateSection.Config,
                    ThemeOverrides = templateSection.ThemeOverrides,
                    SortOrder = templateSection.SortOrder,
                    IsActive = templateSection.IsActive,
                    CustomCssClass = templateSection.CustomCssClass,
                    Title = templateSection.Title
                };

                await CreateSectionAsync(newPage.Id, sectionDto);
            }

            return await GetPageByIdAsync(newPage.Id) ?? throw new InvalidOperationException($"Failed to retrieve created page with ID {newPage.Id}");
        }

        #endregion

        #region Validation

        public async Task<bool> ValidatePageTypeAsync(string pageType)
        {
            return await Task.FromResult(PageTypes.IsValidPageType(pageType));
        }

        public async Task<bool> ValidateSectionTypeAsync(string sectionType)
        {
            return await Task.FromResult(SectionTypes.IsValidSectionType(sectionType));
        }

        public async Task<bool> ValidateSectionConfigAsync(string sectionType, string config)
        {
            try
            {
                // Parse JSON to validate structure
                var jsonDoc = JsonDocument.Parse(config);
                
                // TODO: Add section-specific validation based on sectionType
                
                return await Task.FromResult(true);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public async Task<bool> CanCreatePageTypeAsync(int companyId, string pageType)
        {
            // System pages can only have one instance per company
            if (PageTypes.IsSystemPage(pageType))
            {
                var exists = await _context.WebsitePages
                    .AnyAsync(p => p.CompanyId == companyId && p.PageType == pageType);
                
                return !exists;
            }

            // Other page types can have multiple instances
            return true;
        }

        #endregion

        #region Initialization

        public async Task InitializeDefaultPagesAsync(int companyId)
        {
            _logger.LogInformation("Initializing default pages for company {CompanyId}", companyId);

            // Check if pages already exist for this company
            var existingPages = await _context.WebsitePages
                .Where(p => p.CompanyId == companyId)
                .AnyAsync();

            if (existingPages)
            {
                _logger.LogInformation("Pages already exist for company {CompanyId}, skipping initialization", companyId);
                return;
            }

            // Define default pages to create
            var defaultPages = new[]
            {
                new { PageType = PageTypes.HOME, Name = "Home", Slug = "home", MetaTitle = "Welcome" },
                new { PageType = PageTypes.PRODUCT, Name = "Product", Slug = "product", MetaTitle = "Product Details" },
                new { PageType = PageTypes.ALL_COLLECTIONS, Name = "All Collections", Slug = "collections", MetaTitle = "Collections" },
                new { PageType = PageTypes.CART, Name = "Cart", Slug = "cart", MetaTitle = "Shopping Cart" },
                new { PageType = PageTypes.COLLECTION, Name = "Collection", Slug = "collection", MetaTitle = "Collection" },
                new { PageType = PageTypes.ALL_PRODUCTS, Name = "All Products", Slug = "products", MetaTitle = "All Products" },
                new { PageType = PageTypes.CHECKOUT, Name = "Checkout", Slug = "checkout", MetaTitle = "Checkout" }
            };

            foreach (var pageData in defaultPages)
            {
                var page = new WebsitePage
                {
                    CompanyId = companyId,
                    PageType = pageData.PageType,
                    Name = pageData.Name,
                    Slug = pageData.Slug,
                    MetaTitle = pageData.MetaTitle,
                    MetaDescription = $"{pageData.MetaTitle} page for the website",
                    IsPublished = true,
                    PublishedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Sections = new List<PageSection>()
                };

                _context.WebsitePages.Add(page);
                await _context.SaveChangesAsync();

                // Add default sections based on page type
                await AddDefaultSectionsAsync(page.Id, pageData.PageType);

                _logger.LogInformation("Created default {PageType} page for company {CompanyId}", 
                    pageData.PageType, companyId);
            }

            _logger.LogInformation("Default pages initialized for company {CompanyId}", companyId);
        }

        #endregion

        #region Private Helper Methods

        private async Task AddDefaultSectionsAsync(int pageId, string pageType)
        {
            var sectionsToAdd = new List<(string type, string config)>();

            switch (pageType)
            {
                case PageTypes.PRODUCT:
                    sectionsToAdd.Add((SectionTypes.PRODUCT_INFORMATION, GetDefaultProductInfoConfig()));
                    break;
                
                case PageTypes.COLLECTION:
                case PageTypes.ALL_PRODUCTS:
                    sectionsToAdd.Add((SectionTypes.PRODUCTS, GetDefaultProductsConfig()));
                    break;
                
                case PageTypes.ALL_COLLECTIONS:
                    sectionsToAdd.Add((SectionTypes.COLLECTIONS_LIST, GetDefaultCollectionsListConfig()));
                    break;
                
                case PageTypes.CART:
                    sectionsToAdd.Add((SectionTypes.CART, GetDefaultCartConfig()));
                    break;
                
                // HOME and CUSTOM pages start empty
                default:
                    return;
            }

            int sortOrder = 10;
            foreach (var (type, config) in sectionsToAdd)
            {
                var section = new PageSection
                {
                    PageId = pageId,
                    SectionType = type,
                    Config = config,
                    SortOrder = sortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PageSections.Add(section);
                await _context.SaveChangesAsync();

                // Add default children for PRODUCT_INFORMATION
                if (type == SectionTypes.PRODUCT_INFORMATION)
                {
                    await AddDefaultProductInfoBlocksAsync(section.Id);
                }

                sortOrder += 10;
            }
        }

        private async Task AddDefaultProductInfoBlocksAsync(int sectionId)
        {
            foreach (var (blockType, order) in ProductInfoBlockTypes.DefaultBlocks)
            {
                var child = new PageSectionChild
                {
                    PageSectionId = sectionId,
                    BlockType = blockType,
                    Settings = GetDefaultBlockSettings(blockType),
                    SortOrder = order,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PageSectionChildren.Add(child);
            }

            await _context.SaveChangesAsync();
        }

        private string GetDefaultSectionConfig(string sectionType)
        {
            return sectionType switch
            {
                SectionTypes.PRODUCTS => GetDefaultProductsConfig(),
                SectionTypes.COLLECTIONS_LIST => GetDefaultCollectionsListConfig(),
                SectionTypes.CART => GetDefaultCartConfig(),
                SectionTypes.PRODUCT_INFORMATION => GetDefaultProductInfoConfig(),
                _ => "{}"
            };
        }

        private string GetDefaultProductsConfig()
        {
            return JsonSerializer.Serialize(new
            {
                colorScheme = "default",
                width = "page",
                imageRatio = "portrait",
                sorter = new
                {
                    enableSorting = true,
                    showProductCount = true,
                    showColumnSwitcher = true
                },
                filter = new
                {
                    enableFiltering = true,
                    layout = "left",
                    style = "dropdown"
                },
                cards = new
                {
                    desktopColumns = 4,
                    mobileColumns = 2,
                    spacing = 16
                },
                pagination = new
                {
                    type = "click-to-load",
                    itemsPerPage = 24
                }
            });
        }

        private string GetDefaultCollectionsListConfig()
        {
            return JsonSerializer.Serialize(new
            {
                colorScheme = "default",
                width = "page",
                heading = "Collections",
                headingSize = "h2",
                cards = new
                {
                    imageRatio = "square",
                    contentPosition = "bottom",
                    alignment = "center",
                    showProductCount = true,
                    overlayOpacity = 30
                }
            });
        }

        private string GetDefaultCartConfig()
        {
            return JsonSerializer.Serialize(new
            {
                colorScheme = "default",
                width = "page",
                showProgressBar = true,
                freeShippingGoal = 100,
                progressBarGradient = "linear-gradient(90deg, #ff0000, #00ff00)",
                imageRatio = "square",
                edgeRounding = "medium",
                relatedProducts = new
                {
                    enabled = true,
                    heading = "You may also like",
                    type = "app-based",
                    maxProducts = 4
                }
            });
        }

        private string GetDefaultProductInfoConfig()
        {
            return JsonSerializer.Serialize(new
            {
                colorScheme = "default",
                width = "page",
                mediaPosition = "left",
                stickyContent = true,
                galleryLayout = "thumbnails",
                enableZoom = true,
                enableLightbox = true
            });
        }

        private string GetDefaultBlockSettings(string blockType)
        {
            return blockType switch
            {
                ProductInfoBlockTypes.TITLE => JsonSerializer.Serialize(new { headingSize = "h1" }),
                ProductInfoBlockTypes.PRICE => JsonSerializer.Serialize(new 
                { 
                    showTaxes = false,
                    showSaleBadge = true,
                    highlightSalePrice = true
                }),
                ProductInfoBlockTypes.INVENTORY_STATUS => JsonSerializer.Serialize(new
                {
                    showStatus = "always",
                    lowThreshold = 10,
                    highLabel = "In stock",
                    lowLabel = "Low stock"
                }),
                ProductInfoBlockTypes.VARIANT_PICKER => JsonSerializer.Serialize(new
                {
                    displayStyle = "buttons",
                    showColorSwatches = true,
                    showSizeChart = true
                }),
                ProductInfoBlockTypes.QUANTITY_SELECTOR => JsonSerializer.Serialize(new
                {
                    minQuantity = 1,
                    maxQuantity = 99,
                    showPlusMinusButtons = true
                }),
                ProductInfoBlockTypes.BUY_BUTTONS => JsonSerializer.Serialize(new
                {
                    showAddToCart = true,
                    showBuyNow = true,
                    showPaymentIcons = true
                }),
                _ => "{}"
            };
        }

        private bool IsRequiredSection(string pageType, string sectionType)
        {
            return pageType switch
            {
                PageTypes.PRODUCT => sectionType == SectionTypes.PRODUCT_INFORMATION,
                PageTypes.COLLECTION or PageTypes.ALL_PRODUCTS => sectionType == SectionTypes.PRODUCTS,
                PageTypes.ALL_COLLECTIONS => sectionType == SectionTypes.COLLECTIONS_LIST,
                PageTypes.CART => sectionType == SectionTypes.CART,
                _ => false
            };
        }

        private string GenerateSlug(string name)
        {
            return name.ToLower()
                .Replace(" ", "-")
                .Replace("_", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("&", "and")
                .Replace("/", "-")
                .Replace("\\", "-");
        }

        private WebsitePageDto MapToPageDto(WebsitePage page)
        {
            return new WebsitePageDto
            {
                Id = page.Id,
                CompanyId = page.CompanyId,
                PageType = page.PageType,
                Name = page.Name,
                Slug = page.Slug,
                MetaTitle = page.MetaTitle,
                MetaDescription = page.MetaDescription,
                IsActive = page.IsActive,
                IsPublished = page.IsPublished,
                PublishedAt = page.PublishedAt,
                TemplateId = page.TemplateId,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt,
                Sections = page.Sections?.Select(MapToSectionDto).ToList() ?? new List<PageSectionDto>()
            };
        }

        private PageSectionDto MapToSectionDto(PageSection section)
        {
            return new PageSectionDto
            {
                Id = section.Id,
                PageId = section.PageId,
                SectionType = section.SectionType,
                Config = section.Config,
                ThemeOverrides = section.ThemeOverrides,
                SortOrder = section.SortOrder,
                IsActive = section.IsActive,
                CustomCssClass = section.CustomCssClass,
                Title = section.Title,
                CreatedAt = section.CreatedAt,
                UpdatedAt = section.UpdatedAt,
                Children = section.Children?.Select(MapToChildDto).ToList() ?? new List<PageSectionChildDto>()
            };
        }

        private PageSectionChildDto MapToChildDto(PageSectionChild child)
        {
            return new PageSectionChildDto
            {
                Id = child.Id,
                PageSectionId = child.PageSectionId,
                BlockType = child.BlockType,
                Settings = child.Settings,
                SortOrder = child.SortOrder,
                IsActive = child.IsActive,
                CustomCssClass = child.CustomCssClass,
                CreatedAt = child.CreatedAt,
                UpdatedAt = child.UpdatedAt
            };
        }
        
        /// <summary>
        /// Generates a snapshot of the page for stable production serving
        /// </summary>
        private async Task GenerateSnapshotAsync(int pageId, int companyId)
        {
            _logger.LogInformation("[SNAPSHOT] Entering GenerateSnapshotAsync - PageId: {PageId}, CompanyId: {CompanyId}", 
                pageId, companyId);
            
            try
            {
                // Load complete page data
                var page = await _context.WebsitePages
                    .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                        .ThenInclude(s => s.Children.OrderBy(c => c.SortOrder))
                    .FirstOrDefaultAsync(p => p.Id == pageId);
                    
                if (page == null)
                {
                    _logger.LogWarning("[SNAPSHOT] Page not found - PageId: {PageId}", pageId);
                    return;
                }
                
                _logger.LogInformation("[SNAPSHOT] Page loaded - Slug: {Slug}, PageType: {PageType}", 
                    page.Slug, page.PageType);
                
                // Check for existing snapshot
                var existingSnapshot = await _context.PublishedSnapshots
                    .Where(s => s.PageId == pageId && !s.IsStale)
                    .OrderByDescending(s => s.Version)
                    .FirstOrDefaultAsync();
                    
                // Mark old snapshots as stale
                if (existingSnapshot != null)
                {
                    existingSnapshot.IsStale = true;
                }
                
                // Create complete snapshot data
                _logger.LogInformation("[SNAPSHOT] Creating snapshot data for page {PageId}", pageId);
                var snapshotData = new
                {
                    pageId = page.Id,
                    companyId = page.CompanyId,
                    pageType = page.PageType,
                    name = page.Name,
                    slug = page.Slug,
                    metaTitle = page.MetaTitle,
                    metaDescription = page.MetaDescription,
                    sections = page.Sections.Select(s => new
                    {
                        id = s.Id,
                        type = s.SectionType,
                        config = s.Config,
                        themeOverrides = s.ThemeOverrides,
                        sortOrder = s.SortOrder,
                        isActive = s.IsActive,
                        title = s.Title,
                        customCssClass = s.CustomCssClass,
                        children = s.Children.Select(c => new
                        {
                            id = c.Id,
                            blockType = c.BlockType,
                            settings = c.Settings,
                            sortOrder = c.SortOrder,
                            isActive = c.IsActive,
                            customCssClass = c.CustomCssClass
                        })
                    }),
                    publishedAt = DateTime.UtcNow,
                    version = (existingSnapshot?.Version ?? 0) + 1
                };
                
                // Create new snapshot
                _logger.LogInformation("[SNAPSHOT] Serializing snapshot data...");
                var jsonData = JsonSerializer.Serialize(snapshotData);
                _logger.LogInformation("[SNAPSHOT] Serialized data size: {Size} bytes", jsonData.Length);
                
                var snapshot = new PublishedSnapshot
                {
                    CompanyId = companyId,
                    PageId = pageId,
                    PageSlug = page.Slug,
                    PageType = page.PageType,
                    SnapshotData = jsonData,
                    Version = (existingSnapshot?.Version ?? 0) + 1,
                    IsStale = false,
                    PublishedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                
                _logger.LogInformation("[SNAPSHOT] Adding snapshot to context...");
                _context.PublishedSnapshots.Add(snapshot);
                
                _logger.LogInformation("[SNAPSHOT] Saving to database...");
                var changes = await _context.SaveChangesAsync();
                
                _logger.LogInformation("[SNAPSHOT] SUCCESS - Generated snapshot v{Version} for page {PageId}, Changes saved: {Changes}", 
                    snapshot.Version, pageId, changes);
                _logger.LogInformation("[SNAPSHOT] Snapshot ID: {SnapshotId}, Slug: {Slug}", 
                    snapshot.Id, snapshot.PageSlug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SNAPSHOT] ERROR generating snapshot for page {PageId}. StackTrace: {StackTrace}", 
                    pageId, ex.StackTrace);
            }
            finally
            {
                _logger.LogInformation("[SNAPSHOT] Exiting GenerateSnapshotAsync - PageId: {PageId}", pageId);
            }
        }

        #endregion
    }
}