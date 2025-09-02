namespace WebsiteBuilderAPI.DTOs.Permissions
{
    public class GroupedPermissionsDto
    {
        public string Resource { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
}