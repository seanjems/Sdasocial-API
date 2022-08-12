

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
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
        public int Relationship { get;  set; }
        public string? Address { get;  set; }
        public int Family { get;  set; }
        public int Profession { get;  set; }
        public string? Aboutme { get;  set; }
        public int LocalChurch { get;  set; }
        public string? Contacts { get;  set; }
        public string? FavouriteVerse { get;  set; }
        public DateTime CreatedTime { get; set; }
        public List<Follower>? Followers { get;  set; }


        public AppUser()
        {
            CreatedTime = DateTime.Now;
        }


    }
}
