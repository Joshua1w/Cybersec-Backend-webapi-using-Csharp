using BlogBackend.Data;
using BlogBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BlogBackend.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly BlogDbContext _context;
        public CategoryController(BlogDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllCategories()
        {
            var categories = _context.Categories.Select(c => new { c.Id, c.Name }).ToList();
            return Ok(categories);
        }

        // Admin: Add category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory([FromBody] BlogBackend.Dto.CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { Message = "Category name is required." });

            var category = new BlogBackend.Models.Category { Name = request.Name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(new { category.Id, category.Name });
        }

        // Admin: Delete category
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { Message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Category deleted successfully." });
        }
    }
}
