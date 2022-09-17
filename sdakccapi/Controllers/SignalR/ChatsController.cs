using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos.SignalRDto;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers.SignalR
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuthorizationController _authorizationController;
        private readonly ILogger<ConversationsController> _logger;
        private readonly FollowerController _followerController;
        private readonly UserManager<AppUser> _userManager;
        private readonly ConversationsController _conversationController;
        public ChatsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment, AuthorizationController authorizationController, ILogger<ConversationsController> logger, FollowerController followerController, UserManager<AppUser> userManager, ConversationsController conversationController)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authorizationController = authorizationController;
            _logger = logger;
            _followerController = followerController;
            _userManager = userManager;
            _conversationController = conversationController;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes =
            JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<ChatOut>>> GetChatsPerUser(int page = 1)
        {
            if (_context.chats == null)
            {
                return NotFound();
            }
            int numberPerPage = 50;
            //var posts = await _context.posts.FindAsync(id);
            var currentUser = _authorizationController.GetCurrentUser(HttpContext);

            var chatsList = new List<ChatOut>();

            if (currentUser == null) return Unauthorized();
            var conversations = _context.conversations.Include("Members")
                .Where(x => x.Members.Where(b => b.UserId == currentUser.UserId).Any()).ToList();


            var chats =  await _context.chats
                .Where(x => conversations.Select(x=>x.Id).ToArray()
                .Any(f=> x.conversationId ==f ))
                .OrderByDescending(x=>x.CreatedDate)
                .Skip((page - 1) * numberPerPage).Take(numberPerPage).ToListAsync();



            foreach (var item in chats)
            {
                var chatOut = new ChatOut(item);
                chatOut.ReceiverId = _context.conversationMembers.FirstOrDefault(x => x.ConversationId == item.conversationId && x.UserId != item.UserId).UserId;
                chatsList.Add(chatOut);
            }
            return chatsList.OrderBy(x=>x.CreatedAt).ToList();

        }



        [NonAction]
        [Authorize(AuthenticationSchemes =
           JwtBearerDefaults.AuthenticationScheme)]
        public async Task PostChatsPerUser(Chats chatToSave)
        {
            if (_context.chats == null)
            {
                return;
            }


           await _context.chats.AddAsync(chatToSave);
            _context.SaveChanges();

        }



    }


}
