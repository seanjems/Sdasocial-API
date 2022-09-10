
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
    public class ActiveUsers
    {
        //primary key but without identity
        public string ConnectionId { get; set; }     
        public string UserId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime DateModified { get; set; }
        [ForeignKey("UserId")]public AppUser User { get; set; }
        public ActiveUsers()
        {
            DateModified = DateTime.Now;
        }
        
    }
}
