namespace WebsiteBuilderAPI.DTOs.NavigationMenu
{
    public class CreateNavigationMenuDto
    {
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string? MenuType { get; set; } = "header";
        public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
        public bool IsActive { get; set; } = true;
    }

    public class MenuItemDto
    {
        public string Label { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string? Type { get; set; } = "external";
        public int Order { get; set; }
        public List<MenuItemDto>? SubItems { get; set; }
    }
}