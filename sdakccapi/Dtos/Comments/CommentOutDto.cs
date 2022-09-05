using sdakccapi.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Comments
{
    public class CommentOutDto
    {
    
        public long? ComId { get; set; }      
        public long PostId { get; set; }
        public string UserId { get; set; }
        public string? Text { get; set; }
        public List<CommentOutDto>? Replies { get; set; } = new List<CommentOutDto>();
        public string? CommentImageUrl { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? avatarUrl { get; set; }
        

        public CommentOutDto()
        {

        }
        public CommentOutDto( Models.Entities.Comments comment)
        {
            ComId = comment.Id;
            PostId = comment.PostId;
            UserId = comment.UserId;
            Text = comment.CommentDesc;
            CommentImageUrl = comment.CommentImageUrl;           
            FullName = $"{comment.User.FirstName} {comment.User.Lastname}";
            UserName = comment.User.UserName;
            avatarUrl = comment.User.ProfilePicUrl;

        }
    }
}
