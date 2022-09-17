
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
    public class Chats
    {
        public long Id { get; set; }
        public long conversationId { get; set; }
        public string UserId { get; set; }
        public string? TextMessage { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DateModified { get; set; }
        public bool isDeleted { get; set; } = false;

        public Chats()
        {
            DateModified = DateTime.Now;
        }

    }
}
