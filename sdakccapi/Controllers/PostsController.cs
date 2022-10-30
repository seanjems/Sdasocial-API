using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    [Authorize(AuthenticationSchemes =
    JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuthorizationController _authorizationController;
        private readonly UserManager<AppUser> _userManager;

        public PostsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment, AuthorizationController authorizationController, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authorizationController = authorizationController;
            _userManager = userManager;
        }

        // GET: api/Posts
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CreatedPostOutDto>>> Getposts(int page =1, string? userProfileId=null, string? userName=null)
        {
            if (_context.posts == null)
            {
                return NotFound();
            }
         
            //if(string.IsNullOrEmpty(userProfileId) && string.IsNullOrEmpty(userName))
            //{
            //    ModelState.AddModelError("", "Both Username and UserId cannot be Empty");
            //    return BadRequest(ModelState);
            //}

            int numberPerpage = 15;
            //if on timeline
            List<Posts> posts;
            //ifon userprofile
            if (!string.IsNullOrEmpty(userName) && userName != "undefined" && userName != "null")
            {
                var userId = await _userManager.FindByNameAsync(userName);

                if (userId==null) return NotFound();

                posts = await _context.posts
                //.Include("PostLikesList")
                //.Include(nameof(User))                
                .Where(x=>x.UserId==userId.Id)
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * numberPerpage)
                .Take(numberPerpage)
                .ToListAsync();
            }
            else if (!string.IsNullOrEmpty(userProfileId) && userProfileId != "undefined" && userProfileId != "null")
            {
                posts = await _context.posts
                //.Include("PostLikesList")
                //.Include(nameof(User))                
                .Where(x => x.UserId == userProfileId)
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * numberPerpage)
                .Take(numberPerpage)
                .ToListAsync();


               
            }
            else
            {
                //TODO; Get Id from http context and personalize

                posts = await _context.posts
               //.Include("PostLikesList")
               //.Include(nameof(User))
               .OrderByDescending(x => x.CreatedTime)
               .Skip((page - 1) * numberPerpage)
               .Take(numberPerpage)
               .ToListAsync();
            }
            

            var baseLink = $"{Request.Scheme}://{Request.Host.Value}/"; 
            var list = new List<CreatedPostOutDto>();
            foreach (var post in posts)
            {
                var postOut = PostWithDetails(post);
                postOut.CreatorUSer.ProfilePicUrl = !string.IsNullOrEmpty(postOut.CreatorUSer.ProfilePicUrl) ? baseLink + postOut.CreatorUSer.ProfilePicUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";
                postOut.Img = !string.IsNullOrEmpty(postOut.Img) ? baseLink + post.ImageUrl:null;
                list.Add(postOut);
            }

            return list;
        }
        

   
        // GET: api/Posts/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CreatedPostOutDto>> GetPost(long id)
        {
            if (_context.posts == null)
            {
                return NotFound();
            }
            //var posts = await _context.posts.FindAsync(id);

            var post = await _context.posts
             .Include("PostLikesList")
             .Include("User")             
             .FirstOrDefaultAsync(x=>x.Id==id);

            if (post == null)
            {
                return NotFound();
            }
            
            var baseLink = Request != null ? $"{Request?.Scheme}://{Request?.Host.Value}/" : null ;
            var list = new List<CreatedPostOutDto>();


            var postOut = PostWithDetails(post, Request);
            //var postsLikes = _context.likes.Where(x => x.PostId == post.Id).ToList();
            //foreach (var like in postsLikes)
            //{
            //    var postLiker = _context.Users.Find(like.UserId);
            //    var postLikerName = new PostLikes()
            //    {
            //        UserId = like.UserId,
            //        FirstName = postLiker.FirstName,
            //        LastName = postLiker.Lastname
            //    };
            //    postOut.PostLikes.Add(postLikerName);
            //}
            //var currentUser = _authorizationController.GetCurrentUser(HttpContext);
            //postOut.Likes = postsLikes.Count();
            //postOut.Liked = postsLikes.Where(x => x.UserId == currentUser?.UserId).Count() > 0;
            postOut.CreatorUSer.ProfilePicUrl = !string.IsNullOrEmpty(postOut.CreatorUSer.ProfilePicUrl) ? baseLink + postOut.CreatorUSer.ProfilePicUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";
            postOut.Img = !string.IsNullOrEmpty(post.ImageUrl)?baseLink + post.ImageUrl: null;

            return postOut;
        }

        [NonAction]
        public  CreatedPostOutDto PostWithDetails(Posts post, HttpRequest? request=null)
        {
            if (request == null) request = Request;
            var baseLink = $"{request?.Scheme}://{request?.Host.Value}/";
            var postOut = new CreatedPostOutDto(post);
            var postsLikes = _context.likes.Where(x => x.PostId == post.Id).ToList();
            foreach (var like in postsLikes)
            {
                var postLiker = _context.Users.Find(like.UserId);
                var postLikerName = new PostLikes()
                {
                    UserId = like.UserId,
                    FirstName = postLiker.FirstName,
                    LastName = postLiker.Lastname
                };
                postOut.PostLikes.Add(postLikerName);
            }
            var currentUser = _authorizationController.GetCurrentUser(HttpContext);
            postOut.Likes = postsLikes.Count();
            postOut.Liked = postsLikes.Where(x => x.UserId == currentUser?.UserId).Count() > 0;
            postOut.CreatorUSer = new PostLikes(_context.Users.Find(post.UserId));
            postOut.Img = !string.IsNullOrEmpty(post.ImageUrl)?baseLink + post.ImageUrl:null;
            postOut.Comments = _context.comments.Count(x=>x.PostId == post.Id);
            postOut.Shares = 0;//_context.comments.Count(x=>x.PostId == post.Id);

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
            var currentUser = _authorizationController.GetCurrentUser(HttpContext);
            var postsEntity = new Posts(posts);
            postsEntity.UserId = currentUser?.UserId;
            postsEntity.CreatedTime = DateTime.UtcNow;
            //save image if exists

            //TODO: create a transactional scope for this

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

            // var postsOut = new CreatedPostOutDto(postsEntity);
            var postsOut = PostWithDetails(postsEntity);
            var baseLink = $"{Request.Scheme}://{Request.Host.Value}/";

            //edit image path
            postsOut.Img = !string.IsNullOrEmpty(postsOut.Img)?baseLink + postsOut.Img:null;

            //TODO: GET user details from claims


            postsOut.Name = $"{currentUser.FirstName} {currentUser.LastName}";
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
                    if (!(extension.ToLower() == ".jpg" || extension.ToLower() == ".png" || extension.ToLower() == ".jpeg"))
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
