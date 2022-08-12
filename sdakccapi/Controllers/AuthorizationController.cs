using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using sdakccapi.Dtos;
using sdakccapi.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private IConfiguration _config;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthorizationController(IConfiguration config, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            var user = await Authenticate(userLogin);
            if (user != null)
            {
                var token = GenerateToken(user);
                return Ok(new { UserToken = token });
            }
            return NotFound("Invalid username or password");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            AppUser newUser = new AppUser()
            {
                FirstName = createUserDto.Name,
                Lastname = createUserDto.Surname,
                Email = createUserDto.EmailAddress,
                UserName = createUserDto.UserName,
                
            };
            var result = await _userManager.CreateAsync(newUser,createUserDto.Password);
            if (result.Succeeded)
            {
                return Created("", new { Id = newUser.Id, Email = newUser.Email, FirstName= newUser.FirstName, LastName = newUser.Lastname });
            }
            return BadRequest(result.Errors);
        }

        private string GenerateToken(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("user", JsonConvert.SerializeObject(new UserClaimsDto(user)))
,
            };

           var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(3),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        } 

        private async Task<AppUser> Authenticate(UserLogin userLogin)
        {
            var user = await _userManager.FindByEmailAsync(userLogin.Email);

            if (user!=null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, false);

                if (result.Succeeded)
                {
                    return user;
                }
               
            }
            return null;
        }
    }
}
