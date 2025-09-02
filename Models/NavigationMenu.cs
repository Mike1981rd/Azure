namespace WebsiteBuilderAPI.Models
{
    public class NavigationMenu
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;        // Nombre del menú (ej: "Ventas", "Main menu")
        public string Identifier { get; set; } = string.Empty;  // Identificador único (slug)
        public string? MenuType { get; set; }                   // header, footer, sidebar
        public string? Items { get; set; }                      // JSON array de elementos del menú con estructura MenuItem
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public Company Company { get; set; } = null!;
    }
    
    // Clase para deserializar los items del menú (no es una entidad de BD)
    public class MenuItem
    {
        public string Label { get; set; } = string.Empty;       // Etiqueta visible (ej: "support@purrteam.com")
        public string Link { get; set; } = string.Empty;        // Enlace o acción (ej: "#", "/productos", "mailto:...")
        public string? Type { get; set; }                       // Tipo de enlace: page, collection, product, external, email, phone
        public int Order { get; set; }                          // Orden de aparición
        public List<MenuItem>? SubItems { get; set; }           // Sub-elementos para menús anidados
    }
}