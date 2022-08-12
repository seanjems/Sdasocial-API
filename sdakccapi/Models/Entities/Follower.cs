using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Follower 
    {
      
        public Guid UserId { get; set; }
        public Guid FollowerId { get; set; }
        public DateTime CreatedTime { get; set; }

        public Follower()
        {
            CreatedTime = DateTime.Now;
        }

        
    }
}
