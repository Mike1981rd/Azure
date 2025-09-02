using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        // One review can be for a Product OR a Room (not both)
        public int? ProductId { get; set; }
        public int? RoomId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string AuthorEmail { get; set; } = string.Empty;

        [MaxLength(2)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        [MaxLength(100)]
        public string Source { get; set; } = "Customer submission";

        public bool IsPinned { get; set; } = false;

        public bool IsVerifiedPurchase { get; set; } = false;

        [MaxLength(5000)]
        public string? BusinessReply { get; set; }

        public DateTime? BusinessReplyDate { get; set; }

        public int? RepliedByUserId { get; set; }

        public int LikesCount { get; set; } = 0;

        public int DislikesCount { get; set; } = 0;

        public int HelpfulCount { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public int? ApprovedByUserId { get; set; }

        [MaxLength(5)]
        public string? OriginalLanguage { get; set; }

        [MaxLength(500)]
        public string? TranslatedTitle { get; set; }

        [MaxLength(5000)]
        public string? TranslatedContent { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public virtual User? ApprovedBy { get; set; }

        [ForeignKey("RepliedByUserId")]
        public virtual User? RepliedBy { get; set; }

        public virtual ICollection<ReviewMedia> Media { get; set; } = new List<ReviewMedia>();
        public virtual ICollection<ReviewInteraction> Interactions { get; set; } = new List<ReviewInteraction>();
    }

    public class ReviewMedia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MediaType { get; set; } = "image";

        [Required]
        [MaxLength(500)]
        public string MediaUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        [MaxLength(255)]
        public string? Caption { get; set; }

        public int SortOrder { get; set; } = 0;

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ReviewId")]
        public virtual Review? Review { get; set; }
    }

    public class ReviewInteraction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewId { get; set; }

        public int? CustomerId { get; set; }

        [MaxLength(255)]
        public string? SessionId { get; set; }

        [Required]
        public InteractionType Type { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ReviewId")]
        public virtual Review? Review { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
    }

    public class ReviewStatistics
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        public int? ProductId { get; set; }
        public int? RoomId { get; set; }

        public int TotalReviews { get; set; } = 0;
        public int FiveStarCount { get; set; } = 0;
        public int FourStarCount { get; set; } = 0;
        public int ThreeStarCount { get; set; } = 0;
        public int TwoStarCount { get; set; } = 0;
        public int OneStarCount { get; set; } = 0;

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0;

        public int PositiveReviewsCount { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal PositivePercentage { get; set; } = 0;

        public int NewReviewsThisWeek { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal WeeklyGrowthPercentage { get; set; } = 0;

        [Column(TypeName = "jsonb")]
        public string? WeeklyTrend { get; set; }

        [Required]
        public DateTime LastCalculatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
    }

    public enum ReviewStatus
    {
        Pending,
        Approved,
        Rejected,
        Hidden,
        Flagged,
        Archived
    }

    public enum InteractionType
    {
        Like,
        Dislike,
        Helpful,
        Report
    }

    public enum ReviewSource
    {
        CustomerSubmission,
        VerifiedPurchase,
        Import,
        Email,
        API,
        Migration
    }
}