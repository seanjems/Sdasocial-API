using Sdasocial.BookType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Sdasocial.Posts
{
    public class Posts :  AuditedEntity<Guid> 
    {
       
        public Guid UserId { get; set; } 
        public string Description { get; set; }
        public PostType PostType { get; set; }
        public string Image { get; set; }
        public List<Like.Like> Likes { get; set; }
    }
}
