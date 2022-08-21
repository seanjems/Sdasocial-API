
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
    public class Like 
    {
        [Required]
        public long PostId { get; set; }
        public long PostType { get; set; }
        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }



        public Like()
        {
            DateModified = DateTime.Now;
        }

        public Like(CreateLikeDto createLikeDto)
        {
            PostId = createLikeDto.PostId;
            UserId = createLikeDto.UserId;
            DateModified = DateTime.Now;
        }
       
    }
}
