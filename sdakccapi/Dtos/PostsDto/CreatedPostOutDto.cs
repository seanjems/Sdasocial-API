
using Microsoft.AspNetCore.Http;
using sdakccapi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace sdakccapi.Dtos.PostsDto
{
    
    public class CreatedPostOutDto
    {
        public long Id { get; set; }
        public string CreatorId { get; set; }
        public string Description { get; set; }
        public int? PostType { get; set; }
        public string Image { get; set; }

        public CreatedPostOutDto()
        {

        }

        public CreatedPostOutDto(Posts post)
        {
            Id = post.Id;
            CreatorId = post.UserId;
            Description = post.Description;
            Image = post.ImageUrl;
            PostType = post.PostType;
        }

    }
}
