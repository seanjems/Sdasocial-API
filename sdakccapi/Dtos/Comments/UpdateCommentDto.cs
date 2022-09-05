using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.Comments
{
    public class UpdateCommentDto
    {
        [Required]
        public long? Id { get; set; }
         
        public string? CommentDesc { get; set; }
    }
}
