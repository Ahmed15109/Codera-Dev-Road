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
        public async Task<IActionResult> Add(Course viewModel)
        {
            if (viewModel.formFile != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                viewModel.formFile.CopyTo(memoryStream);
                viewModel.Image = memoryStream.ToArray();
            }
           

            await dbContext.Courses.AddAsync(viewModel);
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
