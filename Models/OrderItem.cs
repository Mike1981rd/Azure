using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        
        // Snapshot del producto al momento de la compra
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        [Column(TypeName = "text")]
        public string? ProductImage { get; set; }
        
        [MaxLength(100)]
        public string? ProductSku { get; set; }
        
        // Atributos del producto (Storage: 128gb, Size: 8UK, etc.) en JSON
        [Column(TypeName = "text")]
        public string? ProductAttributes { get; set; }
        
        // Cantidades y precios
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        
        // Para UI - checkbox de selección
        public bool IsSelected { get; set; } = false;
        
        // Notas adicionales
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        // Navegación
        public virtual Order Order { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariant? ProductVariant { get; set; }
        
        // Propiedades calculadas
        [NotMapped]
        public decimal SubTotal => UnitPrice * Quantity;
        
        [NotMapped]
        public decimal FinalPrice => SubTotal - DiscountAmount + TaxAmount;
    }
}