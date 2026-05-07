namespace TenPercent.Data.Models
{
    using Microsoft.AspNetCore.Identity;
    using System;

        public class User : IdentityUser<int>
    {

        public string Role { get; set; } = "Player";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Agent? Agent { get; set; }
    }
}