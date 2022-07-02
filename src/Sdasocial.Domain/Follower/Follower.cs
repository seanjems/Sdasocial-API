using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Sdasocial.Follower
{
    public class Follower : Entity
    {
        public Guid UserId { get; set; }
        public Guid FollowerId { get; set; }
        public DateTime CreatedTime { get; set; }

        public Follower()
        {
            CreatedTime = DateTime.Now;
        }

        public override object[] GetKeys()
        {
            return new object[] { UserId, FollowerId };
        }
    }
}
