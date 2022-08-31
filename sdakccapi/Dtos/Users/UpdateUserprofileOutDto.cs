using sdakccapi.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Users
{
    public class UpdateUserprofileOutDto
    {
       
        public string UserId { get; set; }       
        public string FirstName { get; set; }        
        public string Lastname { get; set; }
        public string? Relationship { get; set; }
        public string? Address { get; set; }
        public string? Family { get; set; }
        public string? Profession { get; set; }
        public string? Aboutme { get; set; }
        public string? LocalChurch { get; set; }
        public string? Contacts { get; set; }
        public string? FavouriteVerse { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public string ProfilePicUrl { get; set; }
        public string CoverPicUrl { get; set; }
        public string UserName { get; set; }

        public UpdateUserprofileOutDto()
        {

        }
        public UpdateUserprofileOutDto(AppUser user)
        {
            Aboutme = user.Aboutme;
            LocalChurch = user.LocalChurch;
            Contacts = user.Contacts;  
            FavouriteVerse = user.FavouriteVerse;
            Contacts = user.Contacts;
            Aboutme = user.Aboutme;
            Address = user.Address;
            Family = user.Family;
            Profession = user.Profession;
            FirstName = user.FirstName;
            Lastname = user.Lastname;
            UserName = user.UserName;
        }
    }
}
