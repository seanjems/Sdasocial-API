

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public string? Relationship { get;  set; }
        public string? Address { get;  set; }
        public string? Family { get;  set; }
        public string? Profession { get;  set; }
        public string? Aboutme { get;  set; }
        public string? LocalChurch { get;  set; }
        public string? Contacts { get;  set; }
        public string? FavouriteVerse { get;  set; }
        public string? ProfilePicUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        [Required]
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }

        [NotMapped]
        public virtual IEnumerable<AppUser> Followers { get;  set; }
        [NotMapped]
        public virtual IEnumerable<AppUser> Following { get;  set; }


        public AppUser()
        {
            DateModified = DateTime.Now;
        }


    }
}
