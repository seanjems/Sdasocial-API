using sdakccapi.Dtos.Cart;
using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Users
{
    public class CreateUserDto
    {
        [Required]
        [MinLength(4)]
        public string Password { get; set; }
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required, MinLength(3), MaxLength(15)]
        public string Name { get; set; }
        [Required, MinLength(3), MaxLength(15)]
        public string Surname { get; set; }
        public List<CartDetail>? items { get; set; }
        public string? orderCurrency { get; set; }
    }
}
