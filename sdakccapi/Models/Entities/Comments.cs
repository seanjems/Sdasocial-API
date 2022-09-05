
using sdakccapi.Dtos.Comments;
using sdakccapi.Dtos.LikesDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Comments
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long PostId { get; set; }
        [Required]
        public string UserId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }
        [Required]
        public string? CommentDesc { get; set; }
        public long? ParentCommentId { get; set; }
        public string? CommentImageUrl { get; set; }
        [ForeignKey("UserId")]public AppUser User { get; set; }


        public Comments()
        {
            DateModified = DateTime.Now;
        }
        public Comments(CreateCommentDto createCommentDto)
        {
            DateModified = DateTime.Now;
            CommentDesc = createCommentDto.CommentDesc;
            ParentCommentId = createCommentDto.ParentCommentId;
            CommentImageUrl = createCommentDto.CommentImageUrl;
            PostId = createCommentDto.PostId;
            Id = (long)(createCommentDto?.Id??0);
            UserId = createCommentDto.UserId;
        }
    }
}
