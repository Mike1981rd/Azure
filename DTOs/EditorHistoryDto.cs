using System;

namespace WebsiteBuilderAPI.DTOs
{
    /// <summary>
    /// DTO for returning editor history
    /// </summary>
    public class EditorHistoryDto
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Version { get; set; }
        public bool IsCheckpoint { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserName { get; set; }
        public string StateData { get; set; } = "{}";
    }

    /// <summary>
    /// DTO for creating history entry
    /// </summary>
    public class CreateHistoryDto
    {
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string StateData { get; set; } = "{}";
        public string? Description { get; set; }
        public bool IsCheckpoint { get; set; } = false;
    }

    /// <summary>
    /// DTO for undo/redo operations
    /// </summary>
    public class UndoRedoDto
    {
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Direction { get; set; } = "undo"; // undo or redo
        public int? TargetVersion { get; set; } // Optional: go to specific version
    }

    /// <summary>
    /// DTO for history list with pagination
    /// </summary>
    public class HistoryListDto
    {
        public int TotalCount { get; set; }
        public int CurrentVersion { get; set; }
        public bool CanUndo { get; set; }
        public bool CanRedo { get; set; }
        public EditorHistoryDto[] Items { get; set; } = Array.Empty<EditorHistoryDto>();
    }
}