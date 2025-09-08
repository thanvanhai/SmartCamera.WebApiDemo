using System.ComponentModel.DataAnnotations;

namespace SmartCamera.WebApiDemo.Models
{
    public enum UserRole
    {
        SuperAdmin = 1,
        Admin = 2,
        Operator = 3,
        Viewer = 4
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public UserRole Role { get; set; } = UserRole.Viewer;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        [StringLength(500)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}