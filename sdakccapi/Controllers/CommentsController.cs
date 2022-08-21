using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos.Comments;
using sdakccapi.Dtos.Users;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly sdakccapiDbContext _context; 
        private readonly IWebHostEnvironment _webHostEnvironment;


        public CommentsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comments>> GetComments(long id)
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

            return comments;
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComments([FromForm]CreateCommentDto createCommentDto)
        {
            if (createCommentDto.Id ==null)
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
        public async Task<ActionResult<Comments>> PostComments([FromForm]CreateCommentDto commentsInput)
        {
          if (_context.comments == null)
          {
              return Problem("Entity set 'sdakccapiDbContext.comments'  is null.");
          }
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var files = HttpContext.Request.Form.Files;

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

           

            return CreatedAtAction("GetComments", new { id = comments.UserId }, comments);
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
