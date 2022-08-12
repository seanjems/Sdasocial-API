
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Like 
    {
        
        public long PostId { get; set; }
        public long PostType { get; set; }
        public string UserId { get; set; }

        public DateTime CreatedTime { get; set; }
               
        [ForeignKey("UserId")]
        public virtual AppUser Users { get; set; }
        
        public Like()
        {
            CreatedTime = DateTime.Now;
        }

       
    }
}
