namespace WebsiteBuilderAPI.DTOs.NavigationMenu
{
    public class NavigationMenuResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string? MenuType { get; set; }
        public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Información adicional para la UI
        public int ItemCount { get; set; }
        public string ItemsSummary { get; set; } = string.Empty; // Resumen de elementos para mostrar en la lista
    }
}