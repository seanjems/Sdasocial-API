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
using sdakccapi.Dtos.FollowerDto;
using sdakccapi.Dtos.LikesDto;
using sdakccapi.Dtos.PostsDto;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;
using sdakccapi.StaticDetails;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes =
    JwtBearerDefaults.AuthenticationScheme)]
    public class FollowerController : ControllerBase
    {
        private readonly AuthorizationController _authorizationController;
        private readonly sdakccapiDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private  Random rng = new Random();
        public FollowerController(sdakccapiDbContext context, AuthorizationController authorizationController, UserManager<AppUser> userManager)
        {
            _context = context;
            _authorizationController = authorizationController;
            _userManager = userManager;
        }
        

        // POST: api/Likes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CreatedPostOutDto>> CreateFollower(CreateFollowerDto createFollowerDto)
        {
          if (_context.followers == null)
          {
              return Problem("Entity set 'sdakccapiDbContext.folloers'  is null.");
          }
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var currentUser = _authorizationController.GetCurrentUser(HttpContext);
            var follower = new Follower(createFollowerDto);
            follower.UserId = currentUser.UserId;
            var objFromDb = await _context.followers.FirstOrDefaultAsync(x => x.UserId == follower.UserId && x.FollowingId == follower.FollowingId);
            //await _context.SaveChangesAsync();
            if (objFromDb == null)
            {
                var createFollow = await _context.followers.AddAsync(follower);
                await _context.SaveChangesAsync();
            }
            else
            {
                 _context.followers.Remove(objFromDb);
                await _context.SaveChangesAsync();
            }

            var numberOfFollowers = _context.followers.Count(x => x.FollowingId == follower.FollowingId);
            return Created("", new {userId = createFollowerDto.ToFollowId, numberOfFollowers =  numberOfFollowers});
        }

        // GET: api/follower/getfollowersuggest
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostLikes>>> GetfollowerSuggest()
        {
            if (_context.followers == null)
            {
                return NotFound();
            }
            int numberPerpage = 15;
            //if on timeline
            List<PostLikes> suggestedFollows = new List<PostLikes>();
            //ifon userprofile

            var currentUser = _authorizationController.GetCurrentUser(HttpContext);

            //suggest follows based on who is following you
            var query =  _context.followers
                 .Where(x => x.FollowingId == currentUser.UserId);
            int count = 0;
            foreach (var following in query)
            {
                bool noFollowBack = !_context.followers.Where(x => x.UserId != currentUser.UserId && x.FollowingId != following.UserId).Any();
                if (noFollowBack)
                {
                    var user = await _userManager.FindByIdAsync(following.UserId);
                    suggestedFollows.Add(new PostLikes(user));
                    count++;
                }
                if (count>=20)
                {
                    break;
                }
            }

            //extract follows from your friends' friends

            if (suggestedFollows.Count() < 10)
            {
                var myFriends = _context.followers
                 .Where(x => x.UserId == currentUser.UserId).Take(50); //TODO: sort by interaction score

             
                int countThierFriends = 0;
                foreach (var friend in myFriends)
                {
                    var queryThierFriends = _context.followers
                    .Where(x => x.FollowingId == friend.UserId).Take(100); //TODO: sort by interaction score

                    foreach (var friendTheyFollow in queryThierFriends)
                    {
                        
                        bool noFollowBack = !_context.followers.Where(x => x.UserId == currentUser.UserId && x.FollowingId == friendTheyFollow.UserId).Any();
                        if (noFollowBack)
                        {
                            var user = await _userManager.FindByIdAsync(friendTheyFollow.UserId);
                            suggestedFollows.Add(new PostLikes(user));
                            countThierFriends++;
                        }
                        
                        if (countThierFriends >= 5 ) //get their top 5 friends
                        {
                            break;
                        }
                        
                    }
                    if (suggestedFollows.Count() >= 20) break;
                }
            }

            if (suggestedFollows.Count()==0)
            {
                //default  get 20 most followed people from system
                var defaultFriends = _context.followers.GroupBy(x => x.FollowingId)
                                      .OrderByDescending(x => x.Count())
                                      .Select(x => x.Key)
                                      .Take(20)
                                      .ToList();
                foreach (var userSuggestId in defaultFriends)
                {
                    var user = await _userManager.FindByIdAsync(userSuggestId);
                    suggestedFollows.Add(new PostLikes(user));
                }
            }
            //default  get any 20 people from system

            if (suggestedFollows.Count() == 0)
            {
                

                var defaultUsers = _userManager.Users.Where(x=>string.IsNullOrEmpty(x.ProfilePicUrl)).Take(500).ToList();
                int n = defaultUsers.Count() + 1;
                var defaultUsersShuffled = defaultUsers.OrderBy(a => rng.Next(n)).Take(20).ToList();
                foreach (var user in defaultUsersShuffled)
                {                   
                    suggestedFollows.Add(new PostLikes(user));
                }
            }
            return suggestedFollows;
            
        }

        // GET: api/follower/getfollowersuggest
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostLikes>>> GetUsersFollowers(int page=1, string userId=null)
        {
            if (_context.followers == null)
            {
                return NotFound();
            }
            int numberPerpage = 15;
            //if on timeline
            var currentUser = userId?? _authorizationController.GetCurrentUser(HttpContext)?.UserId;
            if (currentUser == null) return BadRequest("Target user is null");
            List<PostLikes> friendsList = new List<PostLikes>();
                
            var myFollowersList =  _context.followers
                 .Where(x => x.FollowingId == currentUser)
                 .Skip(page*numberPerpage)
                 .Take(numberPerpage).ToList(); //TODO: sort by interaction score


            foreach (var follower in myFollowersList)
            {
                var user = await _userManager.FindByIdAsync(follower.UserId);
                friendsList.Add(new PostLikes(user));
            }
           
            return friendsList;
        }

        // GET: api/follower/getfollowersuggest
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostLikes>>> GetUsersFollowing(int page = 1, string userId = null)
        {
            if (_context.followers == null)
            {
                return NotFound();
            }
            int numberPerpage = 15;
            //if on timeline
            var currentUser = userId ?? _authorizationController.GetCurrentUser(HttpContext)?.UserId;
            if (currentUser == null) return BadRequest("Target user is null");
            List<PostLikes> friendsList = new List<PostLikes>();

            var myFollowingList = _context.followers
                 .Where(x => x.UserId == currentUser)
                 .Skip(page * numberPerpage)
                 .Take(numberPerpage).ToList(); //TODO: sort by interaction score


            foreach (var follower in myFollowingList)
            {
                var user = await _userManager.FindByIdAsync(follower.FollowingId);
                friendsList.Add(new PostLikes(user));
            }
            return friendsList;

        }

    }
}
