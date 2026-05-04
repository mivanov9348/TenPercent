namespace TenPercent.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Player";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Agent? Agent { get; set; }
    }
}
