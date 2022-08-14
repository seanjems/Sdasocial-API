
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace sdakccapi.Dtos.PostsDto
{
    
    public class CreatePostDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
      
    }
}
