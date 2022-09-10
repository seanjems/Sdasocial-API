
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
    public class ConversationMembers
    {
        public long ConversationId { get; set; }
        //public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DateModified { get; set; }
        public bool isDeleted { get; set; }
       [ForeignKey("UserId")]public AppUser Users { get; set; }

        public ConversationMembers()
        {
            DateModified = DateTime.Now;
        }

    }
}
