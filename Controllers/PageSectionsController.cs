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
    /// Controller for managing page sections
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PageSectionsController : ControllerBase
    {
        private readonly IWebsiteBuilderService _builderService;
        private readonly ILogger<PageSectionsController> _logger;

        public PageSectionsController(
            IWebsiteBuilderService builderService,
            ILogger<PageSectionsController> logger)
        {
            _builderService = builderService;
            _logger = logger;
        }

        /// <summary>
        /// Get all sections for a page
        /// </summary>
        [HttpGet("page/{pageId}")]
        public async Task<ActionResult<List<PageSectionDto>>> GetSections(int pageId)
        {
            try
            {
                var sections = await _builderService.GetSectionsByPageIdAsync(pageId);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sections for page {PageId}", pageId);
                return StatusCode(500, "An error occurred while retrieving sections");
            }
        }

        /// <summary>
        /// Get a specific section by ID
        /// </summary>
        [HttpGet("{sectionId}")]
        public async Task<ActionResult<PageSectionDto>> GetSection(int sectionId)
        {
            try
            {
                var section = await _builderService.GetSectionByIdAsync(sectionId);
                if (section == null)
                {
                    return NotFound($"Section {sectionId} not found");
                }
                return Ok(section);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section {SectionId}", sectionId);
                return StatusCode(500, "An error occurred while retrieving the section");
            }
        }

        /// <summary>
        /// Add a new section to a page
        /// </summary>
        [HttpPost("page/{pageId}")]
        public async Task<ActionResult<PageSectionDto>> AddSection(
            int pageId,
            [FromBody] CreatePageSectionDto dto)
        {
            try
            {
                var section = await _builderService.CreateSectionAsync(pageId, dto);
                return CreatedAtAction(nameof(GetSection), new { sectionId = section.Id }, section);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating section for page {PageId}", pageId);
                return StatusCode(500, "An error occurred while creating the section");
            }
        }

        /// <summary>
        /// Update a section
        /// </summary>
        [HttpPut("{sectionId}")]
        public async Task<ActionResult<PageSectionDto>> UpdateSection(
            int sectionId,
            [FromBody] UpdatePageSectionDto dto)
        {
            try
            {
                var section = await _builderService.UpdateSectionAsync(sectionId, dto);
                if (section == null)
                {
                    return NotFound($"Section {sectionId} not found");
                }
                return Ok(section);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section {SectionId}", sectionId);
                return StatusCode(500, "An error occurred while updating the section");
            }
        }

        /// <summary>
        /// Delete a section
        /// </summary>
        [HttpDelete("{sectionId}")]
        public async Task<ActionResult> DeleteSection(int sectionId)
        {
            try
            {
                var result = await _builderService.DeleteSectionAsync(sectionId);
                if (!result)
                {
                    return NotFound($"Section {sectionId} not found");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting section {SectionId}", sectionId);
                return StatusCode(500, "An error occurred while deleting the section");
            }
        }

        /// <summary>
        /// Duplicate a section
        /// </summary>
        [HttpPost("{sectionId}/duplicate")]
        public async Task<ActionResult<PageSectionDto>> DuplicateSection(
            int sectionId,
            [FromBody] DuplicateSectionDto dto)
        {
            try
            {
                var section = await _builderService.DuplicateSectionAsync(sectionId, dto);
                if (section == null)
                {
                    return NotFound($"Section {sectionId} not found");
                }
                return CreatedAtAction(nameof(GetSection), new { sectionId = section.Id }, section);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating section {SectionId}", sectionId);
                return StatusCode(500, "An error occurred while duplicating the section");
            }
        }

        /// <summary>
        /// Reorder sections within a page
        /// </summary>
        [HttpPost("page/{pageId}/reorder")]
        public async Task<ActionResult> ReorderSections(
            int pageId,
            [FromBody] ReorderSectionsDto dto)
        {
            try
            {
                var result = await _builderService.ReorderSectionsAsync(pageId, dto);
                if (!result)
                {
                    return BadRequest("Failed to reorder sections");
                }
                return Ok(new { message = "Sections reordered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering sections for page {PageId}", pageId);
                return StatusCode(500, "An error occurred while reordering sections");
            }
        }

        /// <summary>
        /// Validate section type
        /// </summary>
        [HttpGet("validate-type/{sectionType}")]
        public async Task<ActionResult<bool>> ValidateSectionType(string sectionType)
        {
            try
            {
                var isValid = await _builderService.ValidateSectionTypeAsync(sectionType);
                return Ok(new { isValid, sectionType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating section type {SectionType}", sectionType);
                return StatusCode(500, "An error occurred while validating section type");
            }
        }

        /// <summary>
        /// Validate section configuration
        /// </summary>
        [HttpPost("validate-config")]
        public async Task<ActionResult<bool>> ValidateSectionConfig([FromBody] ValidateSectionConfigDto dto)
        {
            try
            {
                var isValid = await _builderService.ValidateSectionConfigAsync(dto.SectionType, dto.Config);
                return Ok(new { isValid, sectionType = dto.SectionType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating section config");
                return StatusCode(500, "An error occurred while validating section configuration");
            }
        }
    }

    /// <summary>
    /// DTO for validating section configuration
    /// </summary>
    public class ValidateSectionConfigDto
    {
        public string SectionType { get; set; } = string.Empty;
        public string Config { get; set; } = "{}";
    }
}