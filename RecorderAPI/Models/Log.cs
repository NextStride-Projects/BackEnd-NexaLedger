namespace RecorderAPI.Models
{
    public class Log
    {
        public int Id { get; set; } // Primary Key
        public string Action { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public int EmpresaId { get; set; }
        public int? AccessedEmpresaId { get; set; }
        public int? AccessedUsuarioId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
