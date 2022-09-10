using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sdakccapi.Dtos.SignalRDto;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActiveUsersController : ControllerBase
    {
        private readonly sdakccapiDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuthorizationController _authorizationController;

        public ActiveUsersController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment, AuthorizationController authorizationController)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _authorizationController = authorizationController;
        }

        // POST: api/PosactiveUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [NonAction]
        public async Task<ActiveUsers> PostActiveUsers(ActiveUsers activeUsers)
        {
            //if (_context.activeUsers == null)
            //{
            //    return Problem("Entity set 'sdakccapiDbContext.activeusers'  is null.");
            //}
            //if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.activeUsers.Add(activeUsers);
            await _context.SaveChangesAsync();

            return activeUsers;
        }

        [NonAction]
        public bool ConnectionExists(string connectionId)
        {
            return _context.activeUsers.Any(u => u.ConnectionId == connectionId);
        }
        [NonAction]
        public List<OnlineUsersOutDto> GetAffectedConversationsWithNewLists(string UserIdOfNewConn)
        {
            //when new user gets connected, only active users are informed and the informed online users should
            //have an existing chat with thhis new connection
            //else no notifications are sent
            //...
            //...
            //if (_context.activeUsers == null)
            //{
            //    return Problem("Entity set 'sdakccapiDbContext.activeusers'  is null.");
            //}
            //get conversations that have this new user
            var conversationsAffected = _context.conversationMembers.Where(x => x.UserId == UserIdOfNewConn).ToList();
           
            var listOfUserIds = conversationsAffected.Select(u=>u.UserId).ToList();
            var activeUsersConversations = _context.activeUsers.Where(x => listOfUserIds.Contains(x.UserId)).ToList();
            //send them new list of active users
            List<OnlineUsersOutDto> listMaster = new List<OnlineUsersOutDto>();
            foreach (var conversation in conversationsAffected)
            {
                var chatList = _context.conversationMembers.Where(x => x.ConversationId == conversation.ConversationId).ToList();
                List<ActiveUsersOutDto> onlineMembers = new List<ActiveUsersOutDto>();

                foreach (var member in chatList)
                {
                    onlineMembers.Add(new ActiveUsersOutDto(member));
                }
                listMaster.Add(new OnlineUsersOutDto()
                {
                    conversationId = conversation.ConversationId,
                    ActiveMembersList = onlineMembers
                });
            }
            return listMaster;

            //var userIdsFromConv = chatConversationsForMemeber.Select(u => u.Members.UserId).ToList();
            //var onlineUsers = _context.activeUsers.Where(x => userIdsFromConv.Contains(x.UserId)).ToList();
            //var listOut = new List<ActiveUsersOutDto>();
            //foreach (var member in onlineUsers)
            //{
            //    listOut.Add(new ActiveUsersOutDto(member));
            //}
            //return Ok(listOut);
        }

        // DELETE: api/Posts/5
        [NonAction]
        public async Task<IActionResult> DeletePosts(string ConnectionId)
        {
            if (_context.activeUsers == null) return NotFound();

            var connection = await _context.activeUsers.FindAsync(ConnectionId);
            if (connection == null) return NotFound();


            _context.activeUsers.Remove(connection);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
