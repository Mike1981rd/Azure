using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Controller for managing website pages
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WebsitePagesController : ControllerBase
    {
        private readonly IWebsiteBuilderService _builderService;
        private readonly ILogger<WebsitePagesController> _logger;

        public WebsitePagesController(
            IWebsiteBuilderService builderService,
            ILogger<WebsitePagesController> logger)
        {
            _builderService = builderService;
            _logger = logger;
        }

        /// <summary>
        /// Get all pages for a company
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<List<WebsitePageDto>>> GetPages(int companyId)
        {
            try
            {
                var pages = await _builderService.GetPagesByCompanyIdAsync(companyId);
                return Ok(pages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pages for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while retrieving pages");
            }
        }

        /// <summary>
        /// Get a specific page by ID
        /// </summary>
        [HttpGet("{pageId}")]
        public async Task<ActionResult<WebsitePageDto>> GetPage(int pageId)
        {
            try
            {
                var page = await _builderService.GetPageByIdAsync(pageId);
                if (page == null)
                {
                    return NotFound($"Page {pageId} not found");
                }
                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page {PageId}", pageId);
                return StatusCode(500, "An error occurred while retrieving the page");
            }
        }

        /// <summary>
        /// Get a page by slug
        /// </summary>
        [HttpGet("company/{companyId}/slug/{slug}")]
        [AllowAnonymous] // Allow public access for live site
        public async Task<ActionResult<WebsitePageDto>> GetPageBySlug(int companyId, string slug)
        {
            try
            {
                var page = await _builderService.GetPageBySlugAsync(companyId, slug);
                if (page == null)
                {
                    return NotFound($"Page with slug '{slug}' not found");
                }
                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page by slug {Slug} for company {CompanyId}", 
                    slug, companyId);
                return StatusCode(500, "An error occurred while retrieving the page");
            }
        }

        /// <summary>
        /// Create a new page
        /// </summary>
        [HttpPost("company/{companyId}")]
        public async Task<ActionResult<WebsitePageDto>> CreatePage(
            int companyId, 
            [FromBody] CreateWebsitePageDto dto)
        {
            try
            {
                var page = await _builderService.CreatePageAsync(companyId, dto);
                return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, page);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while creating the page");
            }
        }

        /// <summary>
        /// Update a page
        /// </summary>
        [HttpPut("{pageId}")]
        public async Task<ActionResult<WebsitePageDto>> UpdatePage(
            int pageId, 
            [FromBody] UpdateWebsitePageDto dto)
        {
            try
            {
                var page = await _builderService.UpdatePageAsync(pageId, dto);
                if (page == null)
                {
                    return NotFound($"Page {pageId} not found");
                }
                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId}", pageId);
                return StatusCode(500, "An error occurred while updating the page");
            }
        }

        /// <summary>
        /// Ensure custom page exists for a company
        /// </summary>
        [HttpPost("company/{companyId}/ensure-custom")]
        public async Task<ActionResult<WebsitePageDto>> EnsureCustomPage(int companyId)
        {
            try
            {
                // Check if target slug already exists
                var existingPage = await _builderService.GetPageBySlugAsync(companyId, "habitaciones");
                if (existingPage != null)
                {
                    return Ok(existingPage);
                }

                // Migrate legacy slug 'custom' → 'habitaciones' if present
                var legacyPage = await _builderService.GetPageBySlugAsync(companyId, "custom");
                if (legacyPage != null)
                {
                    var updateDto = new UpdateWebsitePageDto
                    {
                        Slug = "habitaciones",
                        Name = legacyPage.Name ?? "Habitaciones",
                        MetaTitle = legacyPage.MetaTitle ?? "Habitaciones - Nuestro alojamiento",
                        MetaDescription = legacyPage.MetaDescription ?? "Descubre nuestras habitaciones y servicios de alojamiento",
                        IsActive = true
                    };
                    var updated = await _builderService.UpdatePageAsync(legacyPage.Id, updateDto);
                    return Ok(updated ?? legacyPage);
                }

                // Create the custom page if it doesn't exist
                var createDto = new CreateWebsitePageDto
                {
                    PageType = "CUSTOM", // Use uppercase value to pass validation
                    Slug = "habitaciones",
                    Name = "Habitaciones",
                    MetaTitle = "Habitaciones - Nuestro alojamiento",
                    MetaDescription = "Descubre nuestras habitaciones y servicios de alojamiento"
                };

                var page = await _builderService.CreatePageAsync(companyId, createDto);
                return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring custom page for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while ensuring the custom page exists");
            }
        }

        /// <summary>
        /// Replace all sections of a page (bulk update from editor)
        /// </summary>
        [HttpPut("{pageId}/sections")]
        public async Task<ActionResult<WebsitePageDto>> ReplaceSections(
            int pageId,
            [FromBody] UpdatePageSectionsDto dto)
        {
            try
            {
                var page = await _builderService.GetPageByIdAsync(pageId);
                if (page == null)
                {
                    return NotFound($"Page {pageId} not found");
                }

                // Delegate to service layer for atomic replace
                var updated = await _builderService.ReplacePageSectionsAsync(pageId, dto);
                if (updated == null)
                {
                    return StatusCode(500, "Failed to replace sections");
                }
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing sections for page {PageId}", pageId);
                return StatusCode(500, "An error occurred while replacing sections");
            }
        }

        /// <summary>
        /// Delete a page
        /// </summary>
        [HttpDelete("{pageId}")]
        public async Task<ActionResult> DeletePage(int pageId)
        {
            try
            {
                var result = await _builderService.DeletePageAsync(pageId);
                if (!result)
                {
                    return NotFound($"Page {pageId} not found");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page {PageId}", pageId);
                return StatusCode(500, "An error occurred while deleting the page");
            }
        }

        /// <summary>
        /// Duplicate a page
        /// </summary>
        [HttpPost("{pageId}/duplicate")]
        public async Task<ActionResult<WebsitePageDto>> DuplicatePage(
            int pageId, 
            [FromBody] DuplicatePageDto dto)
        {
            try
            {
                var page = await _builderService.DuplicatePageAsync(pageId, dto.NewName);
                if (page == null)
                {
                    return NotFound($"Page {pageId} not found");
                }
                return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating page {PageId}", pageId);
                return StatusCode(500, "An error occurred while duplicating the page");
            }
        }

        /// <summary>
        /// Publish a page
        /// </summary>
        [HttpPost("{pageId}/publish")]
        public async Task<ActionResult<WebsitePageDto>> PublishPage(
            int pageId,
            [FromBody] PublishWebsitePageDto dto)
        {
            try
            {
                var page = await _builderService.PublishPageAsync(pageId, dto);
                if (page == null)
                {
                    return NotFound($"Page {pageId} not found");
                }
                return Ok(page);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing page {PageId}", pageId);
                return StatusCode(500, "An error occurred while publishing the page");
            }
        }

        /// <summary>
        /// Validate a page
        /// </summary>
        [HttpPost("{pageId}/validate")]
        public async Task<ActionResult<bool>> ValidatePage(int pageId)
        {
            try
            {
                var isValid = await _builderService.ValidatePageAsync(pageId);
                return Ok(new { isValid, pageId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating page {PageId}", pageId);
                return StatusCode(500, "An error occurred while validating the page");
            }
        }

        /// <summary>
        /// Initialize default pages for a company
        /// </summary>
        [HttpPost("company/{companyId}/initialize")]
        [AllowAnonymous] // Allow public access for initialization
        public async Task<ActionResult> InitializeDefaultPages(int companyId)
        {
            try
            {
                await _builderService.InitializeDefaultPagesAsync(companyId);
                return Ok(new { message = $"Default pages initialized for company {companyId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default pages for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while initializing default pages");
            }
        }

        /// <summary>
        /// Get page templates
        /// </summary>
        [HttpGet("templates")]
        public async Task<ActionResult<List<WebsitePageDto>>> GetTemplates([FromQuery] string? pageType = null)
        {
            try
            {
                var templates = await _builderService.GetPageTemplatesAsync(pageType);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page templates");
                return StatusCode(500, "An error occurred while retrieving templates");
            }
        }

        /// <summary>
        /// Create page from template
        /// </summary>
        [HttpPost("company/{companyId}/from-template")]
        public async Task<ActionResult<WebsitePageDto>> CreateFromTemplate(
            int companyId,
            [FromBody] CreateFromTemplateDto dto)
        {
            try
            {
                var page = await _builderService.CreatePageFromTemplateAsync(
                    companyId, dto.TemplateId, dto.Name);
                return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, page);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page from template for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while creating page from template");
            }
        }

        /// <summary>
        /// Check if a page type can be created
        /// </summary>
        [HttpGet("company/{companyId}/can-create/{pageType}")]
        public async Task<ActionResult<bool>> CanCreatePageType(int companyId, string pageType)
        {
            try
            {
                var canCreate = await _builderService.CanCreatePageTypeAsync(companyId, pageType);
                return Ok(new { canCreate, pageType, companyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if page type {PageType} can be created", pageType);
                return StatusCode(500, "An error occurred while checking page type");
            }
        }

        /// <summary>
        /// Validate page type
        /// </summary>
        [HttpGet("validate-type/{pageType}")]
        public async Task<ActionResult<bool>> ValidatePageType(string pageType)
        {
            try
            {
                var isValid = await _builderService.ValidatePageTypeAsync(pageType);
                return Ok(new { isValid, pageType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating page type {PageType}", pageType);
                return StatusCode(500, "An error occurred while validating page type");
            }
        }
    }

    /// <summary>
    /// DTO for duplicating a page
    /// </summary>
    public class DuplicatePageDto
    {
        public string? NewName { get; set; }
    }

    /// <summary>
    /// DTO for creating page from template
    /// </summary>
    public class CreateFromTemplateDto
    {
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}