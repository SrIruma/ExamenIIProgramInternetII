using System.ComponentModel.DataAnnotations;

namespace ExamenII.Models
{
    public class Evidencia
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public string ImagenPath { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public string UserId { get; set; }
    }
}