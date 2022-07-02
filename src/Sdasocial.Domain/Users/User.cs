using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Sdasocial.Users
{
    public  class User:  IdentityUser
    {        
        
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int Relationship { get; set; }
        public string Address { get; set; }
        public int Family { get; set; }
        public int Profession { get; set; }
        public string Aboutme { get; set; }
        public int LocalChurch { get; set; }
        public string  Contacts  { get; set; }
        public string FavouriteVerse { get; set; }

        public List<Follower.Follower> Followers { get; set; }

       
    }
}
