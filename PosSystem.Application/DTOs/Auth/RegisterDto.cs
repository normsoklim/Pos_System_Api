using System.ComponentModel.DataAnnotations;

namespace PosSystem.Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
}