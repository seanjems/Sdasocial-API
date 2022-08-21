
using Microsoft.AspNetCore.Http;
using sdakccapi.Dtos.Users;
using sdakccapi.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using System.Text;

namespace sdakccapi.Dtos.PostsDto
{

    public class CreatedPostOutDto
    {

        public long Id { get; set; }
        public string CreatorId { get; set; }
        public string Desc { get; set; }
        public int? PostType { get; set; }
        public string? Name { get; set; }
        public string? Img { get; set; }
        public int Likes { get; set; }
        public bool Liked { get; set; }
        [ForeignKey("CreatorId")] public UserClaimsDto? CreatorUSer { get; set; }
        public PostLikes? PostLikes { get; set; }


        public CreatedPostOutDto()
        {

        }

        public CreatedPostOutDto(Posts post)
        {
            Id = post.Id;
            CreatorId = post.UserId;
            Desc = post.Description;
            Img =  post.ImageUrl;
            PostType = post.PostType;
            Name = $"{post.User?.FirstName} {post.User?.Lastname}";
        }

    }
}
