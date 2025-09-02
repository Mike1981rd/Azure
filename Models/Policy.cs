using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class Policy
    {
        public int Id { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "returns", "privacy", "terms", "shipping", "contact"
        
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        
        public string? Content { get; set; } // HTML content from rich text editor
        
        public bool IsRequired { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual Company Company { get; set; } = null!;
    }
    
    public static class PolicyTypes
    {
        public const string Returns = "returns";
        public const string Privacy = "privacy";
        public const string Terms = "terms";
        public const string Shipping = "shipping";
        public const string Contact = "contact";
        
        public static readonly Dictionary<string, string> DefaultTitles = new Dictionary<string, string>
        {
            { Returns, "Política de devoluciones y reembolsos" },
            { Privacy, "Política de privacidad" },
            { Terms, "Términos del servicio" },
            { Shipping, "Política de envío" },
            { Contact, "Información de contacto" }
        };
        
        public static readonly Dictionary<string, string> DefaultTitlesEn = new Dictionary<string, string>
        {
            { Returns, "Return and Refund Policy" },
            { Privacy, "Privacy Policy" },
            { Terms, "Terms of Service" },
            { Shipping, "Shipping Policy" },
            { Contact, "Contact Information" }
        };
        
        public static readonly List<string> AllTypes = new List<string>
        {
            Returns, Privacy, Terms, Shipping, Contact
        };
    }
}