using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using sdakccapi.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme)]
   
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<AppUser> userManager;
        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }
        //GET /admin/roles
        [NonAction]
        public async Task<IList<string>> GetRoleForUser(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var user = await userManager.FindByIdAsync(userId);
            var roles = await userManager.GetRolesAsync(user);
            return roles;
        }
        
        //POST /admin/roles/Create
        [HttpPost]
        public async Task<IActionResult> Create([Required(ErrorMessage = "You must eneter a role name"), MinLength(2, ErrorMessage = "Minimum Length is 2")] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityRole newRole = new IdentityRole(name);
                
                IdentityResult result = await roleManager.CreateAsync(newRole);
                if (result.Succeeded)
                {
                    
                    return Ok("The role has been created successfully");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    return BadRequest(ModelState);
                }

            }
            ModelState.AddModelError("", "Minimum length is 2");
            return BadRequest(ModelState);
        }
        //GET /admin/roles/delete
        [HttpDelete]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                IdentityRole roleToDelete = roleManager.Roles.FirstOrDefault(x => x.Id == id);
                if (roleToDelete != null)
                {
                    IdentityResult result = await roleManager.DeleteAsync(roleToDelete);
                    if (result.Succeeded)
                    {                      
                        return Ok("The role has been deleted successfully");
                    }
                }
            }
            
            return NotFound("Error Occured. Role not found");
        }

        
        //POST /admin/roles/Edit/5     
        [NonAction]
        public async Task<bool> AddUserToRole(string userId, string roleName)
        {

            IdentityResult result = new IdentityResult();
            if (string.IsNullOrEmpty(userId)&& string.IsNullOrEmpty(roleName))
            {

                AppUser user = await userManager.FindByIdAsync(userId);
                IdentityRole role = await roleManager.FindByNameAsync(roleName);
                if (user != null && role !=null)
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }

              
            }

            return result.Succeeded;
        }
        //POST /admin/roles/Edit/5     
        [NonAction]
        public async Task<bool> RemoveUserFromRole(string userId, string roleName)
        {

            IdentityResult result = new IdentityResult();
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(roleName))
            {

                AppUser user = await userManager.FindByIdAsync(userId);
                IdentityRole role = await roleManager.FindByNameAsync(roleName);
                if (user != null && role != null)
                {
                  
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }

            }

            return result.Succeeded;
        }
    }
}
