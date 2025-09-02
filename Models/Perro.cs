using System;

namespace WebsiteBuilderAPI.Models
{
    public class Perro
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public int Edad { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}