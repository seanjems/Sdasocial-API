using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Users
{
    public class UpdateUserprofile
    {
        [Required]
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public int Relationship { get; set; }
        public string? Address { get; set; }
        public int Family { get; set; }
        public int Profession { get; set; }
        public string? Aboutme { get; set; }
        public int LocalChurch { get; set; }
        public string? Contacts { get; set; }
        public string? FavouriteVerse { get; set; }
       
    }
}
