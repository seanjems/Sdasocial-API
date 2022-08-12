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
        public DateTime CreatedTime { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        public Posts()
        {
            CreatedTime = DateTime.Now;
        }
    }
}
