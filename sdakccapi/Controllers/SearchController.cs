using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using sdakccapi.Dtos.PostsDto;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes =
    JwtBearerDefaults.AuthenticationScheme)]
    public class SearchController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUser> _userManager;
        private readonly AuthorizationController _authorizationController;

        public SearchController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment, AuthorizationController authorizationController, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authorizationController = authorizationController;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<PostLikes>>> SearchUser(string keyword, int page = 1)
        {
            if (string.IsNullOrEmpty(keyword)) return BadRequest("Keywords cannot be empty");
            int numberPerPage = 20;
            var users = _context.Users.Where(x => x.FirstName.Contains(keyword)
            || x.Lastname.Contains(keyword) ||
            x.LocalChurch.Contains(keyword) ||
            x.Profession.Contains(keyword) ||
            x.Relationship.Contains(keyword) ||
            x.Id.Contains(keyword) ||
            x.UserName.Contains(keyword) ||
            x.PhoneNumber.Contains(keyword) ||
            x.Family.Contains(keyword) ).Skip((page - 1) * numberPerPage).Take(numberPerPage).ToList();

            var baseLink = Request != null ? $"{Request?.Scheme}://{Request?.Host.Value}/" : null;

            var result = new List<PostLikes>();
            foreach (var output in users)
            {
                var item = new PostLikes(output);
                item.ProfilePicUrl = !string.IsNullOrEmpty(item.ProfilePicUrl) ? baseLink + item.ProfilePicUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";

                result.Add(item);
            }
            return result;
        }
    }
}
