using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    /// <summary>
    /// Controller for managing structural components (Header, Footer, Announcement Bar, Cart Drawer)
    /// </summary>
    [ApiController]
    [Route("api/structural-components")]
    [Authorize]
    public class StructuralComponentsController : ControllerBase
    {
        private readonly IStructuralComponentsService _componentsService;
        private readonly IEditorHistoryService _historyService;
        private readonly ILogger<StructuralComponentsController> _logger;

        public StructuralComponentsController(
            IStructuralComponentsService componentsService,
            IEditorHistoryService historyService,
            ILogger<StructuralComponentsController> logger)
        {
            _componentsService = componentsService;
            _historyService = historyService;
            _logger = logger;
        }

        #region Get Operations

        /// <summary>
        /// Get structural components for a company
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<StructuralComponentsDto>> GetByCompanyId(int companyId)
        {
            try
            {
                _logger.LogInformation("Getting structural components for company {CompanyId}", companyId);
                var components = await _componentsService.GetByCompanyIdAsync(companyId);
                
                if (components == null)
                {
                    _logger.LogWarning("No components found for company {CompanyId}, creating defaults", companyId);
                    
                    // Create default components with proper DTO
                    var createDto = new CreateStructuralComponentsDto
                    {
                        HeaderConfig = @"{
                            ""colorScheme"":""1"",
                            ""width"":""page"",
                            ""layout"":""drawer"",
                            ""showSeparator"":false,
                            ""sticky"":{
                                ""enabled"":false,
                                ""alwaysShow"":false,
                                ""mobileEnabled"":false,
                                ""mobileAlwaysShow"":false,
                                ""initialOpacity"":100
                            },
                            ""menuOpenOn"":""hover"",
                            ""menuId"":""main-menu"",
                            ""logo"":{
                                ""desktopUrl"":"""",
                                ""height"":190,
                                ""mobileUrl"":"""",
                                ""mobileHeight"":120,
                                ""altText"":""Company Logo"",
                                ""link"":""/""
                            },
                            ""iconStyle"":""style2-outline"",
                            ""cart"":{
                                ""style"":""bag"",
                                ""showCount"":true,
                                ""countPosition"":""top-right"",
                                ""countBackground"":""#000000"",
                                ""countText"":""#FFFFFF""
                            },
                            ""stickyCart"":false,
                            ""edgeRounding"":""size0"",
                            ""showAs1"":""drawer-and-page"",
                            ""showAs2"":""drawer-and-page"",
                            ""customCss"":""""
                        }",
                        AnnouncementBarConfig = @"{""enabled"":true,""announcements"":[{""id"":""1"",""content"":""Free shipping on orders over $50!""}]}",
                        FooterConfig = @"{""enabled"":true,""sections"":[]}",
                        CartDrawerConfig = @"{""displayType"":""drawer"",""drawerPosition"":""right""}",
                        Notes = "Default configuration created automatically"
                    };
                    
                    components = await _componentsService.CreateOrUpdateAsync(companyId, createDto);
                    
                    if (components == null)
                    {
                        _logger.LogError("Failed to create default components for company {CompanyId}", companyId);
                        return StatusCode(500, "Failed to create default components");
                    }
                    
                    _logger.LogInformation("Successfully created default components for company {CompanyId}", companyId);
                }
                
                _logger.LogInformation("Returning structural components for company {CompanyId}", companyId);
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting components for company {CompanyId}", companyId);
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get published structural components for a company
        /// </summary>
        [HttpGet("company/{companyId}/published")]
        [AllowAnonymous]
        public async Task<ActionResult<StructuralComponentsDto>> GetPublishedByCompanyId(int companyId)
        {
            try
            {
                var components = await _componentsService.GetPublishedByCompanyIdAsync(companyId);
                if (components == null)
                {
                    return NotFound($"No published components found for company {companyId}");
                }
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published components for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while retrieving published components");
            }
        }

        /// <summary>
        /// Get specific component configuration
        /// </summary>
        [HttpGet("company/{companyId}/{componentType}")]
        public async Task<ActionResult<string>> GetComponentConfig(int companyId, string componentType)
        {
            try
            {
                var config = await _componentsService.GetComponentConfigAsync(companyId, componentType);
                if (config == null)
                {
                    return NotFound($"Component {componentType} not found for company {companyId}");
                }
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {ComponentType} for company {CompanyId}", 
                    componentType, companyId);
                return StatusCode(500, "An error occurred while retrieving component configuration");
            }
        }

        /// <summary>
        /// Get published component configuration
        /// </summary>
        [HttpGet("company/{companyId}/{componentType}/published")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetPublishedComponentConfig(int companyId, string componentType)
        {
            try
            {
                var config = await _componentsService.GetPublishedComponentConfigAsync(companyId, componentType);
                if (config == null)
                {
                    return NotFound($"Published component {componentType} not found for company {companyId}");
                }
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published {ComponentType} for company {CompanyId}", 
                    componentType, companyId);
                return StatusCode(500, "An error occurred while retrieving published component");
            }
        }

        #endregion

        #region Create/Update Operations

        /// <summary>
        /// Create or update all structural components
        /// </summary>
        [HttpPost("company/{companyId}")]
        public async Task<ActionResult<StructuralComponentsDto>> CreateOrUpdate(
            int companyId, 
            [FromBody] CreateStructuralComponentsDto dto)
        {
            try
            {
                var components = await _componentsService.CreateOrUpdateAsync(companyId, dto);
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating components for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while saving components");
            }
        }

        /// <summary>
        /// Update specific component
        /// </summary>
        [HttpPut("company/{companyId}/component")]
        public async Task<ActionResult<StructuralComponentsDto>> UpdateComponent(
            int companyId,
            [FromBody] UpdateComponentDto dto)
        {
            try
            {
                var components = await _componentsService.UpdateComponentAsync(companyId, dto);
                if (components == null)
                {
                    return NotFound($"Components not found for company {companyId}");
                }
                return Ok(components);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {ComponentType} for company {CompanyId}", 
                    dto.ComponentType, companyId);
                return StatusCode(500, "An error occurred while updating component");
            }
        }

        /// <summary>
        /// Update all components at once
        /// </summary>
        [HttpPut("company/{companyId}")]
        public async Task<ActionResult<StructuralComponentsDto>> UpdateAll(
            int companyId,
            [FromBody] StructuralComponentsDto dto)
        {
            try
            {
                var components = await _componentsService.UpdateAllComponentsAsync(companyId, dto);
                if (components == null)
                {
                    return NotFound($"Components not found for company {companyId}");
                }
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all components for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while updating components");
            }
        }

        #endregion

        #region Publishing Operations

        /// <summary>
        /// Publish components
        /// </summary>
        [HttpPost("company/{companyId}/publish")]
        public async Task<ActionResult<StructuralComponentsDto>> Publish(
            int companyId,
            [FromBody] PublishComponentsDto dto)
        {
            try
            {
                var components = await _componentsService.PublishAsync(companyId, dto);
                if (components == null)
                {
                    return NotFound($"Components not found for company {companyId}");
                }
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing components for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while publishing components");
            }
        }

        /// <summary>
        /// Unpublish components
        /// </summary>
        [HttpPost("company/{companyId}/unpublish")]
        public async Task<ActionResult<StructuralComponentsDto>> Unpublish(int companyId)
        {
            try
            {
                var components = await _componentsService.UnpublishAsync(companyId);
                if (components == null)
                {
                    return NotFound($"Published components not found for company {companyId}");
                }
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing components for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while unpublishing components");
            }
        }

        /// <summary>
        /// Create draft from published version
        /// </summary>
        [HttpPost("company/{companyId}/create-draft")]
        public async Task<ActionResult<StructuralComponentsDto>> CreateDraftFromPublished(int companyId)
        {
            try
            {
                var components = await _componentsService.CreateDraftFromPublishedAsync(companyId);
                if (components == null)
                {
                    return NotFound($"Published components not found for company {companyId}");
                }
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating draft for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while creating draft");
            }
        }

        #endregion

        #region Reset Operations

        /// <summary>
        /// Reset specific component to default
        /// </summary>
        [HttpPost("company/{companyId}/reset/{componentType}")]
        public async Task<ActionResult<StructuralComponentsDto>> ResetComponent(
            int companyId, 
            string componentType)
        {
            try
            {
                var components = await _componentsService.ResetComponentToDefaultAsync(companyId, componentType);
                if (components == null)
                {
                    return NotFound($"Components not found for company {companyId}");
                }
                return Ok(components);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting {ComponentType} for company {CompanyId}", 
                    componentType, companyId);
                return StatusCode(500, "An error occurred while resetting component");
            }
        }

        /// <summary>
        /// Reset all components to default
        /// </summary>
        [HttpPost("company/{companyId}/reset-all")]
        public async Task<ActionResult<StructuralComponentsDto>> ResetAll(int companyId)
        {
            try
            {
                var components = await _componentsService.ResetAllToDefaultAsync(companyId);
                if (components == null)
                {
                    return NotFound($"Components not found for company {companyId}");
                }
                return Ok(components);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting all components for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while resetting components");
            }
        }

        #endregion

        #region Preview Operations

        /// <summary>
        /// Generate preview HTML for a component
        /// </summary>
        [HttpPost("company/{companyId}/preview")]
        public async Task<ActionResult<string>> GeneratePreview(
            int companyId,
            [FromBody] ComponentPreviewDto dto)
        {
            try
            {
                var html = await _componentsService.GeneratePreviewHtmlAsync(companyId, dto);
                return Ok(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating preview for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while generating preview");
            }
        }

        /// <summary>
        /// Generate CSS for a component
        /// </summary>
        [HttpGet("company/{companyId}/{componentType}/css")]
        public async Task<ActionResult<string>> GenerateComponentCss(int companyId, string componentType)
        {
            try
            {
                var css = await _componentsService.GenerateComponentCssAsync(companyId, componentType);
                return Ok(css);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSS for {ComponentType}", componentType);
                return StatusCode(500, "An error occurred while generating CSS");
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate component configuration
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<bool>> ValidateConfig(
            [FromBody] ValidateConfigDto dto)
        {
            try
            {
                var isValid = await _componentsService.ValidateComponentConfigAsync(dto.ComponentType, dto.Config);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration");
                return StatusCode(500, "An error occurred while validating configuration");
            }
        }

        /// <summary>
        /// Validate all components for a company
        /// </summary>
        [HttpGet("company/{companyId}/validate")]
        public async Task<ActionResult<ValidationResult>> ValidateAll(int companyId)
        {
            try
            {
                var result = await _componentsService.ValidateAllComponentsAsync(companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating components for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while validating components");
            }
        }

        #endregion

        #region History Operations

        /// <summary>
        /// Get component history
        /// </summary>
        [HttpGet("company/{companyId}/history")]
        public async Task<ActionResult<HistoryListDto>> GetHistory(
            int companyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var history = await _historyService.GetHistoryAsync(
                    companyId, "components", companyId, page, pageSize);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while retrieving history");
            }
        }

        /// <summary>
        /// Undo last change
        /// </summary>
        [HttpPost("company/{companyId}/undo")]
        public async Task<ActionResult<EditorHistoryDto>> Undo(int companyId)
        {
            try
            {
                var history = await _historyService.UndoAsync(companyId, "components", companyId);
                if (history == null)
                {
                    return BadRequest("Cannot undo - no previous state available");
                }
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error undoing changes for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while undoing changes");
            }
        }

        /// <summary>
        /// Redo last undone change
        /// </summary>
        [HttpPost("company/{companyId}/redo")]
        public async Task<ActionResult<EditorHistoryDto>> Redo(int companyId)
        {
            try
            {
                var history = await _historyService.RedoAsync(companyId, "components", companyId);
                if (history == null)
                {
                    return BadRequest("Cannot redo - no next state available");
                }
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redoing changes for company {CompanyId}", companyId);
                return StatusCode(500, "An error occurred while redoing changes");
            }
        }

        #endregion
    }

    /// <summary>
    /// DTO for validating configuration
    /// </summary>
    public class ValidateConfigDto
    {
        public string ComponentType { get; set; } = string.Empty;
        public string Config { get; set; } = "{}";
    }
}