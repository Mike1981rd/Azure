using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class HostReview
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int HostId { get; set; }
        
        [ForeignKey("HostId")]
        public virtual Host Host { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
        
        [Required]
        public int ReservationId { get; set; }
        
        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Comment { get; set; }
        
        // Rating categories
        [Range(1, 5)]
        public int CommunicationRating { get; set; }
        
        [Range(1, 5)]
        public int CheckInRating { get; set; }
        
        [Range(1, 5)]
        public int AccuracyRating { get; set; }
        
        [Range(1, 5)]
        public int ValueRating { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign key for Company
        public int CompanyId { get; set; }
        
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        
        // Constructor
        public HostReview()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}