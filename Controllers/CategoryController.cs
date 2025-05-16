
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using progect_DEPI.Models;
using progect_DEPI.ViewModels;

namespace progect_DEPI.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public CategoryController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(Category viewModel)
        {
            if (viewModel.formFile != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                viewModel.formFile.CopyTo(memoryStream);
                viewModel.Image = memoryStream.ToArray();
            }
            
            await dbContext.Categories.AddAsync(viewModel);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var categories = await dbContext.Categories.ToListAsync();
            return View(categories);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await dbContext.Categories.FindAsync(id);
            return View(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Category viewModel)
        {
            var category = await dbContext.Categories.FindAsync(viewModel.CategoryId);
            if (category != null)
            {
                category.CategoryName = viewModel.CategoryName;
                category.Description = viewModel.Description;
                category.LessonsCount = viewModel.LessonsCount;
                category.UpdateAt = viewModel.UpdateAt;
                category.Image = viewModel.Image;   
            }
                await dbContext.SaveChangesAsync();
            return RedirectToAction("List", "Category");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(Category viewModel)
        {
            var category = await dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == viewModel.CategoryId);

            if (category != null)
            {
                dbContext.Categories.Remove(viewModel);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Category");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await dbContext.Categories
                .Include(c => c.Courses)
                    .ThenInclude(course => course.Enrollments)
                        .ThenInclude(enr => enr.User)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }
            
            return View(category);
        }
    }
}
