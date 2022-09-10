
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
    public class Conversations
    {
        [Key]
        public long Id { get; set; }       
      
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }
        public List<ConversationMembers> Members { get; set; }
     


        public Conversations()
        {
            DateModified = DateTime.Now;
        }
        
    }
}
