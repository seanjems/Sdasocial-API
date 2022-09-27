using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos;
using sdakccapi.Dtos.Comments;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes =
    JwtBearerDefaults.AuthenticationScheme)]
    public class CommentsController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuthorizationController _authorizationController;

        public CommentsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager, AuthorizationController authorizationController)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _authorizationController = authorizationController;
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentOutDto>> GetComments(long id)
        {
            return Ok(singleCommentOutPut(id));
        }

        private async Task<CommentOutDto> singleCommentOutPut(long id)
        {
            //this method is currently deperecated

            var baseLink = $"{Request.Scheme}://{Request.Host.Value}/";
            int numberPerpage = 15;
            var allCommentsOnpost = _context.comments.Include(x => x.User).Where(x => x.Id == id)
                .OrderByDescending(x => x.CreatedTime)
                .ToList();
            var commentsList = new List<CommentOutDto>();
            foreach (var comment in allCommentsOnpost)
            {

                var commentOut = new CommentOutDto(comment);
                commentOut.CommentImageUrl = !string.IsNullOrEmpty(commentOut.CommentImageUrl) ? baseLink + commentOut.CommentImageUrl : null;
                commentOut.avatarUrl = !string.IsNullOrEmpty(commentOut.avatarUrl) ? baseLink + commentOut.avatarUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";

                //commbine comment image  and text to make dangerously set html
                if (!string.IsNullOrEmpty(commentOut.CommentImageUrl))
                {

                    commentOut.Text = $"<img src=\"{commentOut.CommentImageUrl}\" alt=\"\" style=\"height: auto;width: auto\"/>" +
                        $"<p></p>" +
                        $"<p>{commentOut.Text}</p>";
                }

                foreach (var comment2 in allCommentsOnpost.Where(x => x.ParentCommentId == commentOut.ComId))
                {
                    var commentOut2 = new CommentOutDto(comment2);
                    commentOut2.CommentImageUrl = !string.IsNullOrEmpty(commentOut2.CommentImageUrl) ? commentOut2.CommentImageUrl : null;


                    if (!string.IsNullOrEmpty(commentOut2.CommentImageUrl))
                    {

                        commentOut.Text = $"<img src=\"{commentOut2.CommentImageUrl}\" alt=\"\" style=\"height: auto;width: auto\"/>" +
                            $"<p></p>" +
                            $"<p>{commentOut2.Text}</p>";
                    }
                    commentOut.Replies.Add(commentOut2);
                }
                commentsList.Add(commentOut);
            }
            return commentsList[0];
        }
        // GET: api/Comments
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CommentOutDto>> GetCommentsPerPost(long PostId, int page = 1)
        {
            var baseLink = $"{Request.Scheme}://{Request.Host.Value}/";
            int numberPerpage = 15;
            var allCommentsOnpost = _context.comments.Include(x => x.User).Where(x => x.PostId == PostId)
                .OrderByDescending(x => x.CreatedTime)
                .Skip((page - 1) * numberPerpage)
                .Take(numberPerpage)
                .ToList();
            var commentsList = new List<CommentOutDto>();
            foreach (var comment in allCommentsOnpost.Where(x => x.ParentCommentId == null || x.ParentCommentId == 0))
            {

                var commentOut = new CommentOutDto(comment);
                commentOut.CommentImageUrl = !string.IsNullOrEmpty(commentOut.CommentImageUrl) ? baseLink + commentOut.CommentImageUrl : null;
                commentOut.avatarUrl = !string.IsNullOrEmpty(commentOut.avatarUrl) ? baseLink + commentOut.avatarUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";
                // commentOut.Text = $"<span style=\"word-wrap: break-word;\"/>{commentOut.Text} </span>";
                //commbine comment image  and text to make dangerously set html
                if (!string.IsNullOrEmpty(commentOut.CommentImageUrl))
                {

                    commentOut.Text = $"<img src=\"{commentOut.CommentImageUrl}\" alt=\"\" style=\"height: auto;width: auto\"/>" +
                        $"<p></p>" +
                        $"<p>{commentOut.Text}</p>";
                }

                foreach (var comment2 in allCommentsOnpost.Where(x => x.ParentCommentId == commentOut.ComId))
                {

                    var commentOut2 = new CommentOutDto(comment2);
                    commentOut2.CommentImageUrl = !string.IsNullOrEmpty(commentOut2.CommentImageUrl) ? baseLink + commentOut2.CommentImageUrl : null;
                    commentOut2.avatarUrl = !string.IsNullOrEmpty(commentOut2.avatarUrl) ? baseLink + commentOut2.avatarUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";

                    if (!string.IsNullOrEmpty(commentOut2.CommentImageUrl))
                    {

                        commentOut.Text = $"<img src=\"{commentOut2.CommentImageUrl}\" alt=\"\" style=\"height: auto;width: auto\"/>" +
                            $"<p></p>" +
                            $"<p>{commentOut2.Text}</p>";
                    }
                    commentOut.Replies.Add(commentOut2);
                }
                commentsList.Add(commentOut);
            }

            return commentsList;
        }
        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutComments([FromForm] UpdateCommentDto createCommentDto)
        {
            if (createCommentDto.Id == null)
            {
                return BadRequest("No comment Id specified");
            }
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var files = HttpContext.Request.Form.Files;

            if (createCommentDto.CommentDesc == null && files.Count() == 0)
            {
                ModelState.AddModelError("", "Comment description and Image can't both be empty");
                return BadRequest(ModelState);
            }
            var commentFromDb = _context.comments.Find(createCommentDto.Id);

            if (commentFromDb == null) return NotFound();


            //TODO; DELETE OLD IMAGE AND UPLOAD NEW IMAGE
            commentFromDb.CommentDesc = commentFromDb.CommentDesc;

            _context.Entry(commentFromDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {

                return BadRequest();

            }

            return NoContent();


        }

        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<List<CommentOutDto>>> PostComments([FromForm] CreateCommentDto commentsInput)
        {
            if (_context.comments == null)
            {
                return Problem("Entity set 'sdakccapiDbContext.comments'  is null.");
            }
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var files = HttpContext.Request.Form.Files;
            var currentUser = _authorizationController.GetCurrentUser(HttpContext);
            if (currentUser == null) return Unauthorized();

            if ((string.IsNullOrEmpty(commentsInput.CommentDesc) || string.IsNullOrWhiteSpace(commentsInput.CommentDesc)) && files.Count() == 0)
            {
                ModelState.AddModelError("", "Comment description and Image can't both be empty");
                return BadRequest(ModelState);
            }

            //save image if exists
            var comments = new Comments(commentsInput);


            if (files.Count() > 0)
            {
                var saveResult = await SaveFile(files[0]);
                if (saveResult.ReturnCode == "200")
                {
                    comments.CommentImageUrl = saveResult.Link;
                }
                else
                {
                    return BadRequest(saveResult.Message);
                }

            }
            comments.CreatedTime = DateTime.Now;
            comments.UserId = currentUser.UserId;
            comments.Id = 0;
            var create = _context.comments.Add(comments);
            await _context.SaveChangesAsync();


            return Ok(GetCommentsPerPost(comments.PostId));
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComments(long id)
        {
            if (_context.comments == null)
            {
                return NotFound();
            }
            var comments = await _context.comments.FindAsync(id);
            if (comments == null)
            {
                return NotFound();
            }

            _context.comments.Remove(comments);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentsExists(string id)
        {
            return (_context.comments?.Any(e => e.UserId == id)).GetValueOrDefault();
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
