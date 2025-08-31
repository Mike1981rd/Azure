using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service implementation for managing editor history
    /// </summary>
    public class EditorHistoryService : IEditorHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditorHistoryService> _logger;

        // Current position tracker for undo/redo
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> _currentPositions = new();

        public EditorHistoryService(
            ApplicationDbContext context,
            ILogger<EditorHistoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Save History

        public async Task<EditorHistoryDto> SaveHistoryAsync(CreateHistoryDto dto)
        {
            // Extract company ID from the entity based on type
            int companyId = await GetCompanyIdFromEntityAsync(dto.EntityType, dto.EntityId);
            return await SaveHistoryAsync(companyId, dto);
        }

        public async Task<EditorHistoryDto> SaveHistoryAsync(int companyId, CreateHistoryDto dto)
        {
            // Get current version
            var currentVersion = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == dto.EntityType && 
                           h.EntityId == dto.EntityId)
                .MaxAsync(h => (int?)h.Version) ?? 0;

            var history = new EditorHistory
            {
                CompanyId = companyId,
                EntityType = dto.EntityType,
                EntityId = dto.EntityId,
                ChangeType = dto.ChangeType,
                StateData = dto.StateData,
                Description = dto.Description,
                IsCheckpoint = dto.IsCheckpoint,
                Version = currentVersion + 1,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = dto.IsCheckpoint ? null : DateTime.UtcNow.AddDays(EditorHistoryConstants.HISTORY_EXPIRATION_DAYS)
            };

            // Set page ID if entity is a section
            if (dto.EntityType == EditorHistoryConstants.ENTITY_SECTION)
            {
                var section = await _context.PageSections.FindAsync(dto.EntityId);
                if (section != null)
                {
                    history.PageId = section.PageId;
                }
            }

            _context.EditorHistories.Add(history);
            await _context.SaveChangesAsync();

            // Trim history if needed
            await TrimHistoryAsync(companyId, dto.EntityType, dto.EntityId);

            _logger.LogInformation("Saved history for {EntityType} {EntityId}, version {Version}", 
                dto.EntityType, dto.EntityId, history.Version);

            return MapToDto(history);
        }

        #endregion

        #region Get History

        public async Task<HistoryListDto> GetHistoryAsync(int companyId, string entityType, int entityId, int page = 1, int pageSize = 20)
        {
            var query = _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == entityType && 
                           h.EntityId == entityId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(h => h.Version)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => MapToDto(h))
                .ToListAsync();

            var currentPosition = GetCurrentPosition(companyId, entityType, entityId);
            var maxVersion = items.Any() ? items.Max(i => i.Version) : 0;

            return new HistoryListDto
            {
                TotalCount = totalCount,
                CurrentVersion = currentPosition,
                CanUndo = currentPosition > 1,
                CanRedo = currentPosition < maxVersion,
                Items = items.ToArray()
            };
        }

        public async Task<EditorHistoryDto?> GetHistoryItemAsync(int historyId)
        {
            var history = await _context.EditorHistories.FindAsync(historyId);
            return history != null ? MapToDto(history) : null;
        }

        public async Task<EditorHistoryDto?> GetLatestHistoryAsync(int companyId, string entityType, int entityId)
        {
            var history = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == entityType && 
                           h.EntityId == entityId)
                .OrderByDescending(h => h.Version)
                .FirstOrDefaultAsync();

            return history != null ? MapToDto(history) : null;
        }

        #endregion

        #region Undo/Redo Operations

        public async Task<EditorHistoryDto?> UndoAsync(int companyId, string entityType, int entityId)
        {
            var currentPosition = GetCurrentPosition(companyId, entityType, entityId);
            if (currentPosition <= 1) return null;

            var targetVersion = currentPosition - 1;
            return await GoToVersionAsync(companyId, entityType, entityId, targetVersion);
        }

        public async Task<EditorHistoryDto?> RedoAsync(int companyId, string entityType, int entityId)
        {
            var currentPosition = GetCurrentPosition(companyId, entityType, entityId);
            
            var maxVersion = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == entityType && 
                           h.EntityId == entityId)
                .MaxAsync(h => (int?)h.Version) ?? 0;

            if (currentPosition >= maxVersion) return null;

            var targetVersion = currentPosition + 1;
            return await GoToVersionAsync(companyId, entityType, entityId, targetVersion);
        }

        public async Task<EditorHistoryDto?> GoToVersionAsync(int companyId, string entityType, int entityId, int version)
        {
            var history = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == entityType && 
                           h.EntityId == entityId &&
                           h.Version == version)
                .FirstOrDefaultAsync();

            if (history == null) return null;

            // Update current position
            SetCurrentPosition(companyId, entityType, entityId, version);

            // Apply the state (this would be entity-specific)
            await ApplyHistoryStateAsync(history);

            _logger.LogInformation("Restored {EntityType} {EntityId} to version {Version}", 
                entityType, entityId, version);

            return MapToDto(history);
        }

        #endregion

        #region Checkpoint Management

        public async Task<EditorHistoryDto> CreateCheckpointAsync(int companyId, string entityType, int entityId, string description)
        {
            // Get current state
            var currentState = await GetCurrentStateAsync(entityType, entityId);

            var dto = new CreateHistoryDto
            {
                EntityType = entityType,
                EntityId = entityId,
                ChangeType = "checkpoint",
                StateData = currentState,
                Description = description,
                IsCheckpoint = true
            };

            return await SaveHistoryAsync(companyId, dto);
        }

        public async Task<EditorHistoryDto[]> GetCheckpointsAsync(int companyId, string entityType, int entityId)
        {
            var checkpoints = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == entityType && 
                           h.EntityId == entityId &&
                           h.IsCheckpoint)
                .OrderByDescending(h => h.Version)
                .Select(h => MapToDto(h))
                .ToListAsync();

            return checkpoints.ToArray();
        }

        public async Task<EditorHistoryDto?> RestoreCheckpointAsync(int historyId)
        {
            var checkpoint = await _context.EditorHistories.FindAsync(historyId);
            if (checkpoint == null || !checkpoint.IsCheckpoint) return null;

            await ApplyHistoryStateAsync(checkpoint);

            // Create new history entry for the restore
            var dto = new CreateHistoryDto
            {
                EntityType = checkpoint.EntityType,
                EntityId = checkpoint.EntityId,
                ChangeType = "restore",
                StateData = checkpoint.StateData,
                Description = $"Restored from checkpoint: {checkpoint.Description}"
            };

            return await SaveHistoryAsync(checkpoint.CompanyId, dto);
        }

        #endregion

        #region Cleanup Operations

        public async Task<int> CleanupOldHistoryAsync(int companyId, int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var toDelete = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.CreatedAt < cutoffDate &&
                           !h.IsCheckpoint)
                .ToListAsync();

            _context.EditorHistories.RemoveRange(toDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} old history items for company {CompanyId}", 
                toDelete.Count, companyId);

            return toDelete.Count;
        }

        public async Task<int> TrimHistoryAsync(int companyId, string entityType, int entityId, int maxItems = 50)
        {
            var items = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == entityType && 
                           h.EntityId == entityId &&
                           !h.IsCheckpoint)
                .OrderByDescending(h => h.Version)
                .Skip(maxItems)
                .ToListAsync();

            if (items.Any())
            {
                _context.EditorHistories.RemoveRange(items);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Trimmed {Count} history items for {EntityType} {EntityId}", 
                    items.Count, entityType, entityId);
            }

            return items.Count;
        }

        public async Task<bool> DeleteHistoryAsync(int historyId)
        {
            var history = await _context.EditorHistories.FindAsync(historyId);
            if (history == null) return false;

            _context.EditorHistories.Remove(history);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted history item {HistoryId}", historyId);
            return true;
        }

        public async Task<bool> DeleteAllHistoryAsync(int companyId, string entityType, int entityId)
        {
            var items = await _context.EditorHistories
                .Where(h => h.CompanyId == companyId && 
                           h.EntityType == entityType && 
                           h.EntityId == entityId)
                .ToListAsync();

            if (items.Any())
            {
                _context.EditorHistories.RemoveRange(items);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted all history for {EntityType} {EntityId}", 
                    entityType, entityId);
            }

            return items.Any();
        }

        #endregion

        #region Session Management

        public async Task<string> StartSessionAsync(int companyId, int userId)
        {
            var sessionId = Guid.NewGuid().ToString();
            
            // Store session info (could be in cache or database)
            // For now, just return the session ID
            
            _logger.LogInformation("Started editing session {SessionId} for user {UserId}", 
                sessionId, userId);

            return await Task.FromResult(sessionId);
        }

        public async Task EndSessionAsync(string sessionId)
        {
            // Clean up session
            // Could create a checkpoint here
            
            _logger.LogInformation("Ended editing session {SessionId}", sessionId);
            
            await Task.CompletedTask;
        }

        public async Task<EditorHistoryDto[]> GetSessionHistoryAsync(string sessionId)
        {
            var items = await _context.EditorHistories
                .Where(h => h.SessionId == sessionId)
                .OrderBy(h => h.CreatedAt)
                .Select(h => MapToDto(h))
                .ToListAsync();

            return items.ToArray();
        }

        #endregion

        #region Helper Methods

        private async Task<int> GetCompanyIdFromEntityAsync(string entityType, int entityId)
        {
            return entityType switch
            {
                EditorHistoryConstants.ENTITY_PAGE => await _context.WebsitePages
                    .Where(p => p.Id == entityId)
                    .Select(p => p.CompanyId)
                    .FirstOrDefaultAsync(),
                    
                EditorHistoryConstants.ENTITY_SECTION => await _context.PageSections
                    .Include(s => s.Page)
                    .Where(s => s.Id == entityId)
                    .Select(s => s.Page.CompanyId)
                    .FirstOrDefaultAsync(),
                    
                EditorHistoryConstants.ENTITY_COMPONENT => await _context.StructuralComponentsSettings
                    .Where(c => c.Id == entityId)
                    .Select(c => c.CompanyId)
                    .FirstOrDefaultAsync(),
                    
                _ => 0
            };
        }

        private async Task<string> GetCurrentStateAsync(string entityType, int entityId)
        {
            object? entity = entityType switch
            {
                EditorHistoryConstants.ENTITY_PAGE => await _context.WebsitePages
                    .Include(p => p.Sections)
                    .FirstOrDefaultAsync(p => p.Id == entityId),
                    
                EditorHistoryConstants.ENTITY_SECTION => await _context.PageSections
                    .FindAsync(entityId),
                    
                EditorHistoryConstants.ENTITY_COMPONENT => await _context.StructuralComponentsSettings
                    .FindAsync(entityId),
                    
                _ => null
            };

            return entity != null ? JsonSerializer.Serialize(entity) : "{}";
        }

        private async Task ApplyHistoryStateAsync(EditorHistory history)
        {
            // This would apply the state back to the entity
            // Implementation would be entity-specific
            
            switch (history.EntityType)
            {
                case EditorHistoryConstants.ENTITY_PAGE:
                    // Restore page state
                    break;
                    
                case EditorHistoryConstants.ENTITY_SECTION:
                    // Restore section state
                    break;
                    
                case EditorHistoryConstants.ENTITY_COMPONENT:
                    // Restore component state
                    break;
            }

            await _context.SaveChangesAsync();
        }

        private EditorHistoryDto MapToDto(EditorHistory history)
        {
            return new EditorHistoryDto
            {
                Id = history.Id,
                EntityType = history.EntityType,
                EntityId = history.EntityId,
                ChangeType = history.ChangeType,
                Description = history.Description,
                Version = history.Version,
                IsCheckpoint = history.IsCheckpoint,
                CreatedAt = history.CreatedAt,
                UserName = history.User?.FullName,
                StateData = history.StateData
            };
        }

        private int GetCurrentPosition(int companyId, string entityType, int entityId)
        {
            var key = $"{companyId}:{entityType}:{entityId}";
            return _currentPositions.GetOrAdd(key, 1);
        }

        private void SetCurrentPosition(int companyId, string entityType, int entityId, int position)
        {
            var key = $"{companyId}:{entityType}:{entityId}";
            _currentPositions.AddOrUpdate(key, position, (k, v) => position);
        }

        #endregion
    }
}