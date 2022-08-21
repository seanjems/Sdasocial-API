using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos.LikesDto;
using sdakccapi.Dtos.PostsDto;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly PostsController _postsController;

        public LikesController(sdakccapiDbContext context, PostsController postsController)
        {
            _context = context;
            _postsController = postsController;
        }

        // POST: api/Likes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CreatedPostOutDto>> PostLike(CreateLikeDto likeInput)
        {
          if (_context.likes == null)
          {
              return Problem("Entity set 'sdakccapiDbContext.likes'  is null.");
          }
            var like = new Like(likeInput);
            var objFromDb = await _context.likes.FirstOrDefaultAsync(x => x.PostId == like.PostId && x.UserId == like.UserId);
            //await _context.SaveChangesAsync();
            if (objFromDb == null)
            {
                var createLike = await _context.likes.AddAsync(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                 _context.likes.Remove(objFromDb);
                await _context.SaveChangesAsync();
            }

            var result = await _postsController.GetPost(like.PostId);

            return result;
        }

        
    }
}
