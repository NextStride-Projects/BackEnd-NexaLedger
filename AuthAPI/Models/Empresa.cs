using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Nombre { get; set; }

        [MaxLength(255)]
        public string? Direccion { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }
    }
}
