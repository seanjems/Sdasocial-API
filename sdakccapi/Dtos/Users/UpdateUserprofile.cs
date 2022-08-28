using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Users
{
    public class UpdateUserprofile
    {
       
        public string UserId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string? Relationship { get; set; }
        public string? Address { get; set; }
        public string? Family { get; set; }
        public string? Profession { get; set; }
        public string? Aboutme { get; set; }
        public string? LocalChurch { get; set; }
        public string? Contacts { get; set; }
        public string? FavouriteVerse { get; set; }
       
    }
}
