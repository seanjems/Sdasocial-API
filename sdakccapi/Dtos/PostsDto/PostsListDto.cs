using System;
using System.Collections.Generic;

namespace sdakccapi.Dtos.PostsDto
{

    public class PostsListDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        
        public string Image { get; set; }
        public List<PostLikes> PostLikes { get; set; }

      

    }
}
