using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos.PostsDto;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuthorizationController _authorizationController;
        private readonly ILogger<ConversationsController> _logger;
        private readonly FollowerController _followerController;
        private readonly UserManager<AppUser> _userManager;
        public ConversationsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment, AuthorizationController authorizationController, ILogger<ConversationsController> logger, FollowerController followerController, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authorizationController = authorizationController;
            _logger = logger;
            _followerController = followerController;
            _userManager = userManager;
        }


        [NonAction]
        public bool ConversationExists(string senderUserId, string receiverUserId)
        {
            var query = _context.conversations.Include("Members");
            query = query.Where(x => x.Members.Where(b => b.UserId == senderUserId).Any() &&
                   x.Members.Where(b => b.UserId == receiverUserId).Any());
            return query.Any();
        }
        [NonAction]

        public async Task<Conversations> GetConversationOrCreate(string senderUserId, string receiverUserId, string ConnectionId)
        {
            var conversation = _context.conversations.Include("Members")
                .Where(x => x.Members.Where(b => b.UserId == senderUserId).Any() &&
                            x.Members.Where(b => b.UserId == receiverUserId).Any()).FirstOrDefault();
            //bool conversationExists = ConversationExists(senderUserId, receiverUserId);
            if (conversation == null)
            {
                //create conversation
                //TODO; start transaction scope

                try
                {
                    using (var scope = _context.Database.BeginTransaction())
                    {
                        var newConversation = new Conversations()
                        {
                            CreatedTime = DateTime.Now,
                            DateModified = DateTime.Now
                        };
                        await _context.conversations.AddAsync(newConversation);
                        await _context.SaveChangesAsync();

                        //add connection members
                        var connMember = new ConversationMembers()
                        {
                            ConversationId = newConversation.Id,
                            //ConnectionId = ConnectionId,
                            CreatedDate = DateTime.Now,
                            UserId = senderUserId,
                            isDeleted = false,
                            DateModified = DateTime.Now
                        };
                        var connMember2 = new ConversationMembers()
                        {
                            ConversationId = newConversation.Id,
                            // ConnectionId = ConnectionId,
                            CreatedDate = DateTime.Now,
                            UserId = receiverUserId,
                            isDeleted = false,
                            DateModified = DateTime.Now
                        };

                        _context.conversationMembers.AddRange(connMember, connMember2);

                        await _context.SaveChangesAsync();
                        scope.Commit();

                        conversation = newConversation;
                    }


                }
                catch (Exception e)
                {
                    //log exception
                    _logger.LogError("Error in ChatHub while adding Conversation", e.InnerException.Message);
                }
            }
            return conversation;
        }

        [NonAction]
        public async Task<List<ConversationMembers>> GetAllConversationsForUser(string currentUserId)
        {
            var conversationList = await _context.conversationMembers
               .Where(x => x.UserId == currentUserId).ToListAsync();

            return conversationList;
        }

        [NonAction]
        public async Task<List<PostLikes>> GetAllChatHeadsPerUser(HubCallerContext? context,int page = 1  )
        {
            string currentUserId;
            if (context!=null){
                currentUserId = _authorizationController.GetCurrentUser(context).UserId;
            }
            else
            {
                currentUserId = _authorizationController.GetCurrentUser(HttpContext).UserId;

            }
            if (string.IsNullOrEmpty(currentUserId)) return null;

            int numberPage = 50;
            var conversation = _context.conversations.Include("Members")
                 .Where(x => x.Members.Where(b => b.UserId == currentUserId).Any())
                 .GroupBy(p => p.Members)
                 .Select(g => g.First()).ToList();
            var chatsOut = new List<PostLikes>();

            var baseLink = Request != null ? $"{Request?.Scheme}://{Request?.Host.Value}/" : null;

            foreach (var convers in conversation)
            {
                var chathead = convers.Members.Where(x => x.UserId != currentUserId).Skip((page - 1) * numberPage).Take(numberPage).FirstOrDefault();

                var user = await _userManager.FindByIdAsync(chathead.UserId);
                var item = new PostLikes(user);
                item.ProfilePicUrl = !string.IsNullOrEmpty(item.ProfilePicUrl) ? baseLink + item.ProfilePicUrl : "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";

                chatsOut.Add(item);
            }
            
            //TODO: add last message and order by last message
            return chatsOut;
        }
    }
}
