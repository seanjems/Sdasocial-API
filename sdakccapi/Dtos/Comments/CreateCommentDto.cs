using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Comments
{
    public class CreateCommentDto
    {
    
        public long? Id { get; set; }
        [Required]
        public long PostId { get; set; }        
        public string? UserId { get; set; }
        public string? CommentDesc { get; set; }
        public long? ParentCommentId { get; set; }
        public string? CommentImageUrl { get; set; }
    }
}
