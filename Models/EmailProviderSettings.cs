using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class EmailProviderSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string Provider { get; set; } = "Postmark"; // Postmark, SendGrid, SMTP

        // Encrypted API key/token
        [StringLength(1000)]
        public string? ApiKey { get; set; }

        // Masked for display (e.g., pm-****************abcd)
        [StringLength(120)]
        public string? ApiKeyMask { get; set; }

        [StringLength(255)]
        public string? FromEmail { get; set; }

        [StringLength(255)]
        public string? FromName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

