using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;

namespace sdakccapi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    [Authorize]
    public class ProductsController : Controller
    {
        private readonly sdakccapiDbContext context;
        private int numberPerPage = 10;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductsController(sdakccapiDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }
        //GET/admin/product/index
        [HttpGet]
        public async Task<IActionResult> GetAllProducts(int p = 1)
        {

            var product = context.products.Skip((p - 1) * numberPerPage).Take(numberPerPage).Include(x => x.Category).OrderByDescending(x => x.Id);
            List<Product> products = await product.ToListAsync();

            return Ok(products);
        }

        //GET/admin/product/category/slug
        [HttpGet]
        public async Task<IActionResult> ProductsByCategory(string categorySlug, int p = 1)
        {
           
            var category = context.categories.FirstOrDefault(x=>x.Slug==categorySlug);
            if (category == null) return NotFound("Category not found");
            
            var product = context.products.Include(x=>x.Category)
                                            .Where(m=>m.categoryId == category.Id)
                                            .Skip((p - 1) * numberPerPage).Take(numberPerPage)
                                            .Include(x=>x.Category)
                                            .OrderByDescending(x => x.Id);

            List<Product> products = await product.ToListAsync();


            return Ok(products);
        }

        //GET/products/details/5
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {

            if (id != default(int) && id>0)
            {
                Product product = await context.products.FirstOrDefaultAsync(x => x.Id == id);
                if (product != null)
                {
                    return Ok(product);
                }
                return NotFound("Product Not found");
            }
            return NotFound("Invalid Product Id");
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
           

                #region Handling the image upload process
                //handle image upload
                string webRootPath = webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string uploadPath = Path.Combine(webRootPath, @"\images\products".TrimStart('\\')); // doesnt work if second path has a trailling slash
                    string extension = Path.GetExtension(files[0].FileName);
                    if (!(extension == ".jpg" || extension == ".png" || extension =="jpeg"))
                    {
                        ModelState.AddModelError("", "The image file type must be jpg or png");
                        return BadRequest(ModelState);
                    }

                    string fileNewName = Guid.NewGuid().ToString() + extension;

                    using (var fileStream = new FileStream(Path.Combine(uploadPath, fileNewName), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    product.productImage = @"\images\products\" + fileNewName;
                }
                else
                {
                    product.productImage = @"\images\products\" + "noImage.png";

                }
                product.Slug = product.productName.ToLower().Replace(" ", "-");
                #endregion

                product.createdDate = DateTime.UtcNow;
                var slug = await context.products.FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The product already exists.");
                    return BadRequest(ModelState);
                }

                context.Add(product);
                await context.SaveChangesAsync();


                return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            

                product.Slug = product.productName.ToLower().Replace(" ", "-");

                var slug = await context.products.Where(x => x.Id != product.Id).FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The product already exists.");
                    return BadRequest(ModelState);
                }
                #region Handling the image upload process
                //handle image upload
                string webRootPath = webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string uploadPath = Path.Combine(webRootPath, @"\images\products".TrimStart('\\')); // doesnt work if second path has a trailling slash
                    string extension = Path.GetExtension(files[0].FileName);
                    if (!(extension == ".jpg" || extension == ".png" || extension =="jpeg"))
                    {
                        ModelState.AddModelError("", "The image file type must be jpg or png");
                        return BadRequest(ModelState);
                    }
                    string fileNewName = Guid.NewGuid().ToString() + extension;

                    //delete old image first if it exists
                    Product prodFromDB = await context.products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == product.Id);
                    if (!string.IsNullOrEmpty(prodFromDB.productImage))
                    {
                        string oldPath = Path.Combine(webRootPath, prodFromDB.productImage.TrimStart('\\'));
                        if (System.IO.File.Exists(oldPath) && prodFromDB.productImage != @"\images\products\noImage.png")
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploadPath, fileNewName), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    product.productImage = @"\images\products\" + fileNewName;
                }
                else
                {
                    //retain old image
                    Product prodFromDB = await context.products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == product.Id);
                    if (!string.IsNullOrEmpty(prodFromDB.productImage))
                    {
                        product.productImage = prodFromDB.productImage;
                    }
                    else
                    {
                        product.productImage = @"\images\products\" + "noImage.png";
                    }

                }

                #endregion

                context.Update(product);
                await context.SaveChangesAsync();

               return Ok("The product has been Updated");
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            Product product = await context.products.FirstOrDefaultAsync(x => x.Id == id);


            if (product == null)
            {
                return NotFound();
            }
            else
            {
                //delete old image first if it exists

                if (!string.IsNullOrEmpty(product.productImage))
                {
                    string oldPath = Path.Combine(webHostEnvironment.WebRootPath, product.productImage.TrimStart('\\'));
                    if (System.IO.File.Exists(oldPath) && product.productImage != @"\images\products\noImage.png")
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
                context.products.Remove(product);
                await context.SaveChangesAsync();

                return Ok("The product has been deleted");
            }
        }
    }
}
