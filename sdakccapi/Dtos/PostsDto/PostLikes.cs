using sdakccapi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace sdakccapi.Dtos.PostsDto
{
    public class PostLikes
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }

        public PostLikes(AppUser user)
        {
            UserId = user.Id;
            FirstName = user.FirstName;
            LastName = user.Lastname;
            UserName = user.UserName;
        }
        public PostLikes()
        {

        }
    }
}
