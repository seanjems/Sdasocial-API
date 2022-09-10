using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Dtos.SignalRDto;
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

        public ConversationsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment, AuthorizationController authorizationController, ILogger<ConversationsController> logger, FollowerController followerController)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authorizationController = authorizationController;
            _logger = logger;
            _followerController = followerController;
        }


        [NonAction]
        public bool ConversationExists(string senderUserId, string receiverUserId)
        {
            var query = _context.conversations.Include("Members");
              query =      query.Where(x => x.Members.Where(b => b.UserId==senderUserId).Any() &&
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
            if(conversation==null)
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
               .Where(x => x.UserId==currentUserId).ToListAsync();

            return conversationList;
        }
    }
}
