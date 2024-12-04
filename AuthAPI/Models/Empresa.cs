using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        [MaxLength(255)]
        public string? FullName { get; set; }

        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Alias { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string? Category { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Location { get; set; }

        [Required]
        public bool Active { get; set; }

        public string? Features { get; set; }

        [Required]
        [MaxLength(255)]
        public string? ResponsiblePerson { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string? ResponsibleEmail { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StaffCount { get; set; } = 0;
    }
}
