    namespace InventoryAPI.Models;

    public class Movement
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
