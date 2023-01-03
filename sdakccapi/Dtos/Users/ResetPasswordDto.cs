using sdakccapi.Dtos.Cart;
using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Users
{
    public class ResetPasswordDto
    {
        [Required]
        [MinLength(4)]
        public string Password { get; set; }
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        
        public string? OldPassword { get; set; }       
        public string? ResetToken { get; set; }
    }
}
