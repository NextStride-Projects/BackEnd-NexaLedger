namespace RecorderAPI.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string Action { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string EmpresaId { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
