
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace sdakccapi.Dtos.PostsDto
{
    
    public class UpdatePostsDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        [Required]
        public string Description { get; set; }      
        public IFormFile? ImageName { get; set; }
      
    }
}
