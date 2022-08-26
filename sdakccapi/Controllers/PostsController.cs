using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos;
using sdakccapi.Dtos.PostsDto;
using sdakccapi.Dtos.Users;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreatedPostOutDto>>> Getposts(int page =1, string? userProfileId=null)
        {
            if (_context.posts == null)
            {
                return NotFound();
            }
            int numberPerpage = 15;
            //if on timeline
            List<Posts> posts;
            //ifon userprofile
            if (userProfileId ==null)
            {
              posts = await _context.posts
               .Include(nameof(PostLikes))
               .Include(nameof(User))
               .OrderByDescending(x => x.CreatedTime)
               .Skip((page - 1) * numberPerpage)
               .Take(numberPerpage)
               .ToListAsync();
            }
            else
            {
               posts = await _context.posts
                .Include(nameof(PostLikes))
                .Include(nameof(User))                
                .Where(x=>x.UserId==userProfileId)
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * numberPerpage)
                .Take(numberPerpage)
                .ToListAsync();
            }
            

            var baseLink = $"{Request.Scheme}://{Request.Host.Value}/"; 
            var list = new List<CreatedPostOutDto>();
            foreach (var post in posts)
            {
                var postOut = new CreatedPostOutDto(post);
                postOut.CreatorUSer = new UserClaimsDto(post.User);
                postOut.Img = baseLink + post.ImageUrl;
                list.Add(postOut);
            }

            return list;
        }
        

   
        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreatedPostOutDto>> GetPost(long id)
        {
            if (_context.posts == null)
            {
                return NotFound();
            }
            //var posts = await _context.posts.FindAsync(id);

            var post = await _context.posts
             .Include(nameof(Like))
             .Include(nameof(User))             
             .FirstOrDefaultAsync(x=>x.Id==id);

            if (post == null)
            {
                return NotFound();
            }

            var baseLink = $"{Request.Scheme}://{Request.Host.Value}/";
            var list = new List<CreatedPostOutDto>();

           
            var postOut = new CreatedPostOutDto(post);
            postOut.CreatorUSer = new UserClaimsDto(post.User);
            postOut.Img = baseLink + post.ImageUrl;

            return postOut;
        }

        // PUT: api/Posts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosts([FromForm] CreatedPostOutDto postDto)
        {
            if (postDto.Id != postDto.Id)
            {
                return BadRequest();
            }

            var files = HttpContext.Request.Form.Files;

            if (postDto.Desc == null && files.Count() == 0)
            {
                ModelState.AddModelError("", "Post description and Image can't both be empty");
                return BadRequest(ModelState);
            }
            var postFromDb = _context.posts.Find(postDto.Id);

            if (postFromDb == null) return NotFound();

            postFromDb.Description = postDto.Desc;

            _context.Entry(postFromDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostsExists(postDto.Id))
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
        public async Task<ActionResult<CreatedPostOutDto>> PostPosts([FromForm] CreatePostDto posts)
        {
            if (_context.posts == null)
            {
                return Problem("Entity set 'sdakccapiDbContext.posts'  is null.");
            }
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var files = HttpContext.Request.Form.Files;

            if (posts.Description == null && files.Count() == 0)
            {
                ModelState.AddModelError("", "Post description and Image can't both be empty");
                return BadRequest(ModelState);
            }
            var postsEntity = new Posts(posts);

            //save image if exists

            if (files.Count() > 0)
            {
                var saveResult = await SaveFile(files[0]);
                if (saveResult.ReturnCode == "200")
                {
                    postsEntity.ImageUrl = saveResult.Link;
                }
                else
                {
                    return BadRequest(saveResult.Message);
                }

            }

            _context.posts.Add(postsEntity);
            await _context.SaveChangesAsync();

            var postsOut = new CreatedPostOutDto(postsEntity);
            var baseLink = $"{Request.Scheme}://{Request.Host.Value}/";

            //edit image path
            postsOut.Img = baseLink + postsOut.Img;

            //TODO: GET user details from claims
            
                var currentUser = _context.Users.Find(postsOut.CreatorId);
            postsOut.Name = $"{currentUser.FirstName} {currentUser.Lastname}";
            return CreatedAtAction("GetPosts", postsOut);
        }

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosts(long id)
        {
            if (_context.posts == null) return NotFound();

            var posts = await _context.posts.FindAsync(id);
            if (posts == null) return NotFound();


            _context.posts.Remove(posts);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostsExists(long id)
        {
            return (_context.posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        //Save imageupload file
        private async Task<ResultObject> SaveFile(IFormFile dto)
        {
            //handle image upload
            string webRootPath = _webHostEnvironment.WebRootPath;
            string link = null;
            var obj = new ResultObject();

            var files = dto;
            try
            {
                if (files.Length > 0)
                {
                    string uploadPath = Path.Combine(webRootPath, @"\media\images".TrimStart('\\')); // doesnt work if second path has a trailling slash
                    string extension = Path.GetExtension(files.FileName);
                    if (!(extension.ToLower() == ".jpg" || extension.ToLower() == ".png"))
                    {
                        throw new ApplicationException("The image file type must be jpg or png");

                    }

                    string fileNewName = Guid.NewGuid().ToString() + extension;

                    using (var fileStream = new FileStream(Path.Combine(uploadPath, fileNewName), FileMode.Create))
                    {
                        await files.CopyToAsync(fileStream);
                    }
                    link = @"media/images/" + fileNewName;
                }

                string msg = "Upload of Image Successful";
                obj.ReturnCode = "200";
                obj.ReturnDescription = msg;
                obj.Response = "Success";
                obj.Message = msg;
                obj.Link = link;

            }
            catch (Exception ex)
            {
                string msg = "An Error has occurred while attempting to Upload Image: Inner Exception: " + ex.Message;
                obj.ReturnCode = "501";
                obj.ReturnDescription = msg;
                obj.Response = "Failed";
                obj.Message = msg;
            }
            return obj;
        }

    }
}
