namespace WebsiteBuilderAPI.DTOs.NavigationMenu
{
    public class UpdateNavigationMenuDto
    {
        public string? Name { get; set; }
        public string? Identifier { get; set; }
        public string? MenuType { get; set; }
        public List<MenuItemDto>? Items { get; set; }
        public bool? IsActive { get; set; }
    }
}