using sdakccapi.Dtos.PostsDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Posts 
    {
        [Key]
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public int? PostType { get; set; }
        public string? ImageUrl { get; set; }
        [Required]
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        public List<Like> PostLikes { get; set; }
        public Posts()
        {
            DateModified = DateTime.Now;
        }
        public Posts(CreatePostDto createPostDto)
        {
            DateModified = DateTime.Now;
            Description = createPostDto.Description;
            Id = createPostDto.Id;
            UserId = createPostDto.UserId;
        }
        public Posts(CreatedPostOutDto createPostDto)
        {
        
            Id = createPostDto.Id;
            DateModified = DateTime.Now;
            Description = createPostDto.Desc;
            Id = createPostDto.Id;
            UserId = createPostDto.CreatorId;
        }
    }
}
