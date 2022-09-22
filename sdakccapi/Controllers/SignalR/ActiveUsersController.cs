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

            var conversationList = _context.conversations.Include("Members")
                .Where(x => x.Members.Where(b => b.UserId == UserIdOfNewConn).Any())
                .Select(x=>x.Members.Where(b=>b.UserId!=UserIdOfNewConn).Select(y=>y.UserId)).ToList()
                .SelectMany(p=>p);

            
            var activeUsersConversations = _context.activeUsers.Where(x => conversationList.Contains(x.UserId)).ToList();
            //send them new list of active users
            var onlineUsers = new List<OnlineUsersOutDto>();
            foreach (var activeUser in activeUsersConversations.OrderByDescending(x=>x.CreatedTime).GroupBy(x=>x.UserId).First())
            {
                var conversationHeads = _context.conversations.Include("Members")
                    .Where(x => x.Members.Where(b => b.UserId == activeUser.UserId).Any())
                    .SelectMany(x=>x.Members).Where(y=>y.UserId!=activeUser.UserId)
                    .Select(x=>x.UserId)
                    .ToList();

                //add to return object ready for dispatch

               
                onlineUsers.Add(new OnlineUsersOutDto { 
                    ConnectionId = activeUser.ConnectionId,
                    ActiveUserIds = conversationHeads
                });

                
            }
            return onlineUsers;

           
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
