using System.ComponentModel.DataAnnotations;

namespace sdakccapi.Dtos.FollowerDto
{
    public class CreateFollowerDto
    {
        
        public string? CurrentUser { get; set; }
        [Required]
        public string ToFollowId { get; set; }

    }
}
