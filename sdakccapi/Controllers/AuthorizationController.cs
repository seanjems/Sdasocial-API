using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using sdakccapi.Dtos.Users;
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


        public AuthorizationController(IConfiguration config, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _config = config;
            _signInManager = signInManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
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
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserprofile updateUserprofile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var files = HttpContext.Request.Form.Files;


            var objFromDb = await _userManager.FindByIdAsync(updateUserprofile.UserId); //TODO: get id from httpcontext

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
                        objFromDb.ProfilePicUrl = saveResult.Link;
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
            objFromDb.LocalChurch = updateUserprofile.LocalChurch;
            objFromDb.Contacts = updateUserprofile.Contacts;
            objFromDb.FavouriteVerse = updateUserprofile.FavouriteVerse;

            var result = await _userManager.UpdateAsync(objFromDb);
            if (result.Succeeded)
            {
                return Ok("User details updated");
            }
            return BadRequest(result.Errors);

        }

        private async Task<string> GenerateUserName(string oldUserName)
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
