using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models
{
    public class Host
    {
        // Identificación
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } // FK al usuario del sistema de autenticación
        
        // Información Personal
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(500)]
        public string? ProfilePicture { get; set; } // URL de la imagen
        
        [StringLength(500)]
        public string? Bio { get; set; } // Descripción personal
        
        public DateTime JoinedDate { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        // Información adicional del anfitrión
        public int? YearStartedHosting { get; set; } // Año en que comenzó como anfitrión
        
        [StringLength(1000)]
        public string AboutMe { get; set; } // Bio extendida/Acerca de mí
        
        [StringLength(200)]
        public string? Location { get; set; } // Dónde vive (ej: "Madrid, España")
        
        [StringLength(100)]
        public string? Work { get; set; } // A qué se dedica
        
        [Column(TypeName = "jsonb")]
        public string Attributes { get; set; } // JSON array de virtudes/características (ej: ["Amigable", "Puntual", "Comunicativo"])
        
        [Column(TypeName = "jsonb")]
        public string Hobbies { get; set; } // JSON array de hobbies/intereses
        
        // Verificación
        public bool IsVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsIdentityVerified { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string Languages { get; set; } // JSON array de idiomas que habla
        
        // Métricas Calculadas (se actualizan automáticamente)
        [Column(TypeName = "decimal(3,2)")]
        public decimal OverallRating { get; set; } // Promedio de todas las reseñas
        
        public int TotalReviews { get; set; } // Contador total de reseñas
        
        public int ResponseTimeMinutes { get; set; } // Tiempo promedio de respuesta
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal AcceptanceRate { get; set; } // Porcentaje de reservas aceptadas
        
        public bool IsSuperhost { get; set; } // Badge especial
        
        // Configuración del Host
        public bool IsActive { get; set; } = true; // Si está activo como host
        
        public DateTime? LastActiveDate { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign key for Company
        public int CompanyId { get; set; }
        
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        
        // Navegación (Entity Framework)
        public virtual ICollection<Room> Rooms { get; set; }
        public virtual ICollection<HostReview> HostReviews { get; set; }
        
        // Computed property
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        // Constructor
        public Host()
        {
            Rooms = new HashSet<Room>();
            HostReviews = new HashSet<HostReview>();
            JoinedDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            Languages = "[]"; // Empty JSON array
            Attributes = "[]"; // Empty JSON array
            Hobbies = "[]"; // Empty JSON array
            OverallRating = 0;
            TotalReviews = 0;
            ResponseTimeMinutes = 0;
            AcceptanceRate = 0;
            IsActive = true;
            YearStartedHosting = DateTime.UtcNow.Year; // Default to current year
        }
    }
}
