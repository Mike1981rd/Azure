using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.Models
{
    public class Gato
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = "";
        
        public int Edad { get; set; }
        
        public bool Domestico { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}