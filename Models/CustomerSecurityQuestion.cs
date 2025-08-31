using System;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class CustomerSecurityQuestion
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Question { get; set; }
        
        [Required]
        public string AnswerHash { get; set; } // Encrypted answer
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        public virtual Customer Customer { get; set; }
        
        public CustomerSecurityQuestion()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}