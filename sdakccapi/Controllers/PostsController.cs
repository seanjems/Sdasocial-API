using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;

        public PostsController(sdakccapiDbContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Posts>>> Getposts()
        {
          if (_context.posts == null)
          {
              return NotFound();
          }
            return await _context.posts.ToListAsync();
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Posts>> GetPosts(long id)
        {
          if (_context.posts == null)
          {
              return NotFound();
          }
            var posts = await _context.posts.FindAsync(id);

            if (posts == null)
            {
                return NotFound();
            }

            return posts;
        }

        // PUT: api/Posts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosts(long id, Posts posts)
        {
            if (id != posts.Id)
            {
                return BadRequest();
            }

            _context.Entry(posts).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Posts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Posts>> PostPosts(Posts posts)
        {
          if (_context.posts == null)
          {
              return Problem("Entity set 'sdakccapiDbContext.posts'  is null.");
          }
            _context.posts.Add(posts);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPosts", new { id = posts.Id }, posts);
        }

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosts(long id)
        {
            if (_context.posts == null)
            {
                return NotFound();
            }
            var posts = await _context.posts.FindAsync(id);
            if (posts == null)
            {
                return NotFound();
            }

            _context.posts.Remove(posts);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostsExists(long id)
        {
            return (_context.posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
