﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sdakccapi.Controllers.SmtpClient;
using sdakccapi.Dtos;
using sdakccapi.Dtos.EmailDto;
using sdakccapi.Dtos.Users;
using sdakccapi.Infrastructure;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly sdakccapiDbContext _context;
        private readonly SmtpSenderController _emailSmtpController;


        public AuthorizationController(IConfiguration config,sdakccapiDbContext context, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment, SmtpSenderController emailSmtpController)
        {
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _emailSmtpController = emailSmtpController;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            var user = await Authenticate(userLogin);
            if (user != null)
            {
                

                var token = await GenerateToken(user);

                var emailDto = new EmailDto(true);
                emailDto.Subject = "A new Login has been detected";
                emailDto.ToEmail = user.Email;
                emailDto.Body = $"A new login into your account with SDA Kampala central has been detetected at {DateTime.UtcNow}UTC. If this wasn't you, You can take some steps to secure your account such as changing your account password with us or contacting us for help. <br/><br/>If this was you, then you can Ignore this message.<br/><br/>https://social.kampalacentraladventist.org";

                _emailSmtpController.SendMail(emailDto);
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
                UserName = createUserDto.UserName.ToLower(),


            };

            var result = await _userManager.CreateAsync(newUser, createUserDto.Password);
            if (result.Succeeded)
            {
                return Created("", new { Id = newUser.Id, Email = newUser.Email, FirstName = newUser.FirstName, LastName = newUser.Lastname });
            }
            else if (result.Errors.FirstOrDefault()?.Code == "DuplicateUserName")
            {
                // username exists auto generate new              
                newUser.UserName = await GenerateUserName(newUser.UserName);

                //try again creating once
                result = await _userManager.CreateAsync(newUser, createUserDto.Password);
                if (result.Succeeded)
                {
                    return Created("", new { Id = newUser.Id, Email = newUser.Email, FirstName = newUser.FirstName, LastName = newUser.Lastname });
                }

            }
            return BadRequest(result.Errors);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrEmpty(resetPasswordDto.OldPassword) && string.IsNullOrEmpty(resetPasswordDto.ResetToken))
            {
                ModelState.AddModelError("", "TokenContext and old password cannot all be null");
                return BadRequest(ModelState);  
            }
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.EmailAddress);
            if(user == null) return NotFound("User doesnot exist");
            if (resetPasswordDto?.OldPassword is not null)
            {
                
                var result = await _userManager.ChangePasswordAsync(user, resetPasswordDto.OldPassword, resetPasswordDto.Password);

                if (result.Succeeded)
                {
                    return Ok("Password Changed Successfully");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);                        
                    }
                    return BadRequest(ModelState);
                }
            }
            else
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.ResetToken));

                
                var result = await _userManager.ResetPasswordAsync(user,code,resetPasswordDto.Password);

                if (result.Succeeded)
                {
                    return Ok("Password Changed Successfully");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
           
        }
        // [Authorize]
        [NonAction]
        public UserClaimsDto GetCurrentUser(HttpContext httpContext)
        {
            var identity = httpContext?.User?.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;
                var userDetails = userClaims.FirstOrDefault(x => x.Type == "user")?.Value;
                if (userDetails == null) return null;
                var userClaimsDto = JsonConvert.DeserializeObject<UserClaimsDto>(userDetails);
                return userClaimsDto;
                
            }
            return null;
        }
        [Authorize]
        [NonAction]
        public UserClaimsDto GetCurrentUser(HubCallerContext hubContext)
        {
            var identity = hubContext?.User?.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;
                var userDetails = userClaims.FirstOrDefault(x => x.Type == "user")?.Value;
                if (userDetails == null) return null;
                var userClaimsDto = JsonConvert.DeserializeObject<UserClaimsDto>(userDetails);
                return userClaimsDto;

            }
            return null;
        }
        [Authorize(AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserprofile updateUserprofile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var files = HttpContext.Request.Form.Files;

            //read user from user context
            var currentUserId = GetCurrentUser(HttpContext)?.UserId;
            var objFromDb = await _userManager.FindByIdAsync(currentUserId); //TODO: get id from httpcontext

            if (objFromDb == null) return NotFound();


            //save image if exists

            if (files.Count() > 0)
            {
                //save dp if exists
                var dp = files.Where(x => x.Name.ToLower() == "profilepic").ToArray();
                if (dp.Any())
                {
                    var saveResult = await SaveFile(dp.FirstOrDefault());
                    if (saveResult.ReturnCode == "200")
                    {
                        objFromDb.ProfilePicUrl = saveResult.Link;
                    }
                    else
                    {
                        return BadRequest(saveResult.Message);
                    }
                }
                //save cover pic if exists
                var cover = files.Where(x => x.Name.ToLower() == "coverpic").ToArray();
                if (cover.Any())
                {
                    var saveResult = await SaveFile(cover.FirstOrDefault());
                    if (saveResult.ReturnCode == "200")
                    {
                        objFromDb.CoverPhotoUrl = saveResult.Link;
                    }
                    else
                    {
                        return BadRequest(saveResult.Message);
                    }
                }

            }

            if (!string.IsNullOrEmpty(updateUserprofile.FirstName)) objFromDb.FirstName = updateUserprofile.FirstName;
            if (!string.IsNullOrEmpty(updateUserprofile.Lastname)) objFromDb.Lastname = updateUserprofile.Lastname;

            objFromDb.Relationship = updateUserprofile.Relationship;
            objFromDb.Address = updateUserprofile.Address;
            objFromDb.Family = updateUserprofile.Family;
            objFromDb.Profession = updateUserprofile.Profession;
            objFromDb.Aboutme = updateUserprofile.Aboutme;
            objFromDb.LocalChurch = "SDA Kampala Central";//updateUserprofile.LocalChurch;
            objFromDb.Contacts = updateUserprofile.Contacts;
            objFromDb.FavouriteVerse = updateUserprofile.FavouriteVerse;

            var result = await _userManager.UpdateAsync(objFromDb);
            if (result.Succeeded)
            {
                return Ok("User details updated");
            }
            return BadRequest(result.Errors);

        }
        // GET: api/Posts/5
        [HttpGet]
        [Authorize(AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserProfile(string id=null, string userName=null)
        {
            if (id == null && userName == null)
            {
                ModelState.AddModelError("", "Both Username and Id cannot be null for this request");
                return BadRequest(ModelState);
            }
            AppUser user;
            if (!string.IsNullOrEmpty(userName) && userName != "null" && userName != "undefined")
            {
                user = await _userManager.FindByNameAsync(userName);
            }
            else
            {
                user = await _userManager.FindByIdAsync(id);
            }

            if (user == null) return NotFound("User not found");
            

            var baseLink = Request != null ? $"{Request?.Scheme}://{Request?.Host.Value}/" : null;
            //var list = new List<CreatedPostOutDto>();

            var userProfile = new UpdateUserprofileOutDto(user);
            
            var followers = _context.followers.Where(x => x.FollowingId == id).ToList();
            var following = _context.followers.Where(x => x.UserId == id).ToList();
            userProfile.TotalPosts = _context.posts.Where(x=>x.UserId == id).Count();

            userProfile.Followers = followers.Count();
            userProfile.Following = following.Count();

            userProfile.ProfilePicUrl = !string.IsNullOrEmpty(user.ProfilePicUrl) ? baseLink + user.ProfilePicUrl: "https://www.seekpng.com/png/detail/143-1435868_headshot-silhouette-person-placeholder.png";
            userProfile.CoverPicUrl = !string.IsNullOrEmpty(user.CoverPhotoUrl)? baseLink + user.CoverPhotoUrl: "https://via.placeholder.com/728x500.png?text=No+Cover+Image";
            
            return Ok(userProfile);
        }

        [NonAction]
        public async Task<string> GenerateUserName(string oldUserName)
        {
            int random = 1;
            string newUsername = oldUserName + random.ToString();

            while (await _userManager.FindByNameAsync(newUsername) != null)
            {
                random++;
                //prevent  infinte loop
                if (random > 100)
                {
                    break;
                }

            }
            return newUsername;
        }

        private async Task<string> GenerateToken(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var package = await _userManager.GetRolesAsync(user);

            //change here
            string packageValue = package?.FirstOrDefault(x => x == "lugogo")??"";
            var claims = new[]
            {
                new Claim("user", JsonConvert.SerializeObject(new UserClaimsDto(user))),
                new Claim("Package", packageValue)
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

            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, false);

                if (result.Succeeded)
                {
                    return user;
                }

            }
            return null;
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
