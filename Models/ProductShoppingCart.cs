using System;

namespace WebsiteBuilderAPI.Models
{
    public class ProductShoppingCart
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? Items { get; set; } // JSON array de items del carrito
        public DateTime CreatedAt { get; set; }

        // Navegación
        public Company Company { get; set; } = null!;
    }
}