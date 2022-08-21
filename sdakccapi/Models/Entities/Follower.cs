using sdakccapi.Dtos.FollowerDto;
using sdakccapi.StaticDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Follower 
    {
        public string UserId { get; set; }
        public string FollowingId { get; set; }
        [Required]
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }

        public Follower()
        {
            DateModified = DateTime.Now;
        }
        public Follower(CreateFollowerDto newFollower)
        {
            UserId = newFollower.CurrentUser;
            FollowingId = newFollower.ToFollowId;
            DateModified = DateTime.Now;
        }
        
    }
}
