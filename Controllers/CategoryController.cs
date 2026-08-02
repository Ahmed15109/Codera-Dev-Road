
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

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private const long MaxCategoryImageBytes = 10 * 1024 * 1024;

        public CategoryController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private static bool IsAllowedImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            return file.Length <= MaxCategoryImageBytes
                && AllowedImageExtensions.Contains(extension)
                && allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(AddCategoryViewModel viewModel)
        {
            var category = new Category
            {
                CategoryName = viewModel.CategoryName,
                Description = viewModel.Description,
                LessonsCount = viewModel.LessonsCount,
                UpdateAt = viewModel.UpdateAt
            };

            if (viewModel.formFile != null && viewModel.formFile.Length > 0)
            {
                if (!IsAllowedImage(viewModel.formFile))
                {
                    ModelState.AddModelError(nameof(viewModel.formFile), "Upload a JPG, PNG, GIF, or WebP image up to 10 MB.");
                }
                else
                {
                    using var memoryStream = new MemoryStream();
                    await viewModel.formFile.CopyToAsync(memoryStream);
                    category.Image = memoryStream.ToArray();
                }
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            await dbContext.Categories.AddAsync(category);
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
                if (viewModel.formFile != null && viewModel.formFile.Length > 0)
                {
                    if (!IsAllowedImage(viewModel.formFile))
                    {
                        ModelState.AddModelError(nameof(viewModel.formFile), "Upload a JPG, PNG, GIF, or WebP image up to 10 MB.");
                        return View(viewModel);
                    }

                    using var memoryStream = new MemoryStream();
                    await viewModel.formFile.CopyToAsync(memoryStream);
                    category.Image = memoryStream.ToArray();
                }
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
