using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using progect_DEPI.Models;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.ViewModels;

namespace progect_DEPI.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private const long MaxCourseImageBytes = 10 * 1024 * 1024;

        public CoursesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private static bool IsAllowedImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            return file.Length <= MaxCourseImageBytes
                && AllowedImageExtensions.Contains(extension)
                && allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            var categories = dbContext.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(AddCourseViewModel viewModel)
        {
            var course = new Course
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                Price = viewModel.Price,
                CreatedAt = viewModel.CreatedAt,
                UpdateAt = viewModel.UpdateAt,
                CategoryId = viewModel.CategoryId
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
                    course.Image = memoryStream.ToArray();
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(dbContext.Categories.ToList(), "CategoryId", "CategoryName");
                return View(viewModel);
            }

            await dbContext.Courses.AddAsync(course);
            await dbContext.SaveChangesAsync();

            ViewBag.Message = "Course Added successfully!";
            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var courses = await dbContext.Courses.ToListAsync();
            return View(courses);
        }

        public IActionResult Details(int id)
        {
            var course = dbContext.Courses
                .Include(c => c.Lessons)
                .FirstOrDefault(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await dbContext.Courses.FindAsync(id);
            return View(course);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Course viewModel)
        {
            var course = await dbContext.Courses.FindAsync(viewModel.CourseId);
            if (course is not null)
            {
                course.Title = viewModel.Title;
                course.Description = viewModel.Description;
                course.Price = viewModel.Price;
                course.CreatedAt = viewModel.CreatedAt;
                course.UpdateAt = viewModel.UpdateAt;
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("List", "Courses");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await dbContext.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course is not null)
            {
                dbContext.Courses.Remove(course);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Courses");
        }

        public IActionResult CourseLessons(int courseId)
        {
            var course = dbContext.Courses
                .Include(c => c.Lessons.OrderBy(l => l.OrderNumber))
                .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            return View(course);
        }
    }
}
