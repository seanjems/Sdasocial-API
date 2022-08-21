using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos.FollowerDto;
using sdakccapi.Dtos.LikesDto;
using sdakccapi.Dtos.PostsDto;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowerController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;

        public FollowerController(sdakccapiDbContext context)
        {
            _context = context;
        }

        // POST: api/Likes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CreatedPostOutDto>> CreateFollower(CreateFollowerDto createFollowerDto)
        {
          if (_context.followers == null)
          {
              return Problem("Entity set 'sdakccapiDbContext.folloers'  is null.");
          }
            var follower = new Follower(createFollowerDto);
            var objFromDb = await _context.followers.FirstOrDefaultAsync(x => x.UserId == follower.UserId && x.FollowingId == follower.FollowingId);
            //await _context.SaveChangesAsync();
            if (objFromDb == null)
            {
                var createFollow = await _context.followers.AddAsync(follower);
                await _context.SaveChangesAsync();
            }
            else
            {
                 _context.followers.Remove(objFromDb);
                await _context.SaveChangesAsync();
            }

            var numberOfFollowers = _context.followers.Count(x => x.FollowingId == follower.FollowingId);
            return Created("", new {userId = createFollowerDto.ToFollowId, numberOfFollowers =  numberOfFollowers});
        }

        
    }
}
