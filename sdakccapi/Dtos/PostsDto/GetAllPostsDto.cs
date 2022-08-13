using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace sdakccapi.Dtos.PostsDto
{

    public class GetAllPostsDto
    {
        public IEnumerable<PostsListDto> Posts { get; set; }
        public int Page { get; set; }


    }
}
