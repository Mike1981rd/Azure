using System.Threading.Tasks;
using WebsiteBuilderAPI.DTOs;

namespace WebsiteBuilderAPI.Services
{
    /// <summary>
    /// Service interface for managing editor history (undo/redo functionality)
    /// </summary>
    public interface IEditorHistoryService
    {
        // Save history
        Task<EditorHistoryDto> SaveHistoryAsync(CreateHistoryDto dto);
        Task<EditorHistoryDto> SaveHistoryAsync(int companyId, CreateHistoryDto dto);
        
        // Get history
        Task<HistoryListDto> GetHistoryAsync(int companyId, string entityType, int entityId, int page = 1, int pageSize = 20);
        Task<EditorHistoryDto?> GetHistoryItemAsync(int historyId);
        Task<EditorHistoryDto?> GetLatestHistoryAsync(int companyId, string entityType, int entityId);
        
        // Undo/Redo operations
        Task<EditorHistoryDto?> UndoAsync(int companyId, string entityType, int entityId);
        Task<EditorHistoryDto?> RedoAsync(int companyId, string entityType, int entityId);
        Task<EditorHistoryDto?> GoToVersionAsync(int companyId, string entityType, int entityId, int version);
        
        // Checkpoint management
        Task<EditorHistoryDto> CreateCheckpointAsync(int companyId, string entityType, int entityId, string description);
        Task<EditorHistoryDto[]> GetCheckpointsAsync(int companyId, string entityType, int entityId);
        Task<EditorHistoryDto?> RestoreCheckpointAsync(int historyId);
        
        // Cleanup operations
        Task<int> CleanupOldHistoryAsync(int companyId, int daysToKeep = 30);
        Task<int> TrimHistoryAsync(int companyId, string entityType, int entityId, int maxItems = 50);
        Task<bool> DeleteHistoryAsync(int historyId);
        Task<bool> DeleteAllHistoryAsync(int companyId, string entityType, int entityId);
        
        // Session management
        Task<string> StartSessionAsync(int companyId, int userId);
        Task EndSessionAsync(string sessionId);
        Task<EditorHistoryDto[]> GetSessionHistoryAsync(string sessionId);
    }
}