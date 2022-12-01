

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
    
    public class AppRole : IdentityRole
    {
               
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }
        

        [NotMapped]
        public virtual IEnumerable<AppUser>? UsersInRole { get;  set; }


        public AppRole()
        {
            DateModified = DateTime.Now;
        }


    }
}
