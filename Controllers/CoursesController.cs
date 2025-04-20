using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;
using progect_DEPI.ViewModels;

namespace progect_DEPI.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public CoursesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
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

            await dbContext.Courses.AddAsync(course);
            await dbContext.SaveChangesAsync();


            ViewBag.Message = "Course Added successfully!";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var courses = await dbContext.Courses.ToListAsync();
            return View(courses);
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
        public async Task<IActionResult> Delete(Course viewModel)
        {
            var course = await dbContext.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourseId == viewModel.CourseId);
            if (course is not null)
            {
                dbContext.Courses.Remove(viewModel);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List", "Courses");
        }
    }
}
