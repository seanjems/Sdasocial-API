
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    [Authorize]
    public class CategoryController : Controller
    {
       
        private readonly sdakccapiDbContext context;
        public CategoryController(sdakccapiDbContext context)
        {
            this.context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            List<Category> categories = await context.categories.OrderBy(x => x.Sorting).ToListAsync();
            return Ok(categories);
        }

        

        //PosT/admin/Category/Create
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
                category.Slug = category.Name.ToLower().Replace(" ", "-");
                category.Sorting = 100;
                var slug = await context.categories.FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The Category already exists.");
                    return BadRequest(ModelState);
                }

                context.Add(category);
                await context.SaveChangesAsync();


            return Ok(category);
           
        }
        [HttpGet]
        public async Task<IActionResult> GetSingleCategory(int id)
        {
            Category category = await context.categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        //PosT/admin/Category/edit/
        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
           

                category.Slug =  category.Name.ToLower().Replace(" ", "-");

                var slug = await context.categories.Where(x => x.Id != category.Id).FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The category already exists.");
                    return BadRequest(ModelState);
                }

                context.Update(category);
                await context.SaveChangesAsync();
                                   
                return Ok(category);
        }
        //GET/admin/Category/Delete/5
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Category category = await context.categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return NotFound();

            }
            else
            {
                context.categories.Remove(category);
                await context.SaveChangesAsync();

                
            }
            return Ok();
        }


    }
}
