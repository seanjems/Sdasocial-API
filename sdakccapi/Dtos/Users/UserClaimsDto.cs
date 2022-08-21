using sdakccapi.Models.Entities;

namespace sdakccapi.Dtos.Users
{
    public class UserClaimsDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public long? TenantId { get; set; }

        public UserClaimsDto()
        {

        }
        public UserClaimsDto(AppUser user)
        {
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.Lastname;
            UserId = user.Id;
            FullName = $"{user.FirstName} {user.Lastname}";
        }

    }

}
