using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;
using progect_DEPI.ViewModels;

namespace progect_DEPI.Controllers
{
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public QuizzesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Courses = new SelectList(dbContext.Courses.ToList(), "CourseId", "Title");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(AddQuizViewModel viewModel)
        {
            var quiz = new Quiz
            {
                Title = viewModel.Title,
                Content = viewModel.Content,
                Time = viewModel.Time,
                Status = viewModel.Status,
                CreatedAt = viewModel.CreatedAt,
                CourseId = viewModel.CourseId
            };

            await dbContext.Quizzes.AddAsync(quiz);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("List", "Quizzes");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var quizzes = await dbContext.Quizzes
                .Include(q => q.Course)
                .ToListAsync();

            return View(quizzes);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await dbContext.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            ViewBag.Courses = new SelectList(dbContext.Courses.ToList(), "CourseId", "Title", quiz.CourseId);
            return View(quiz);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Quiz viewModel)
        {
            var quiz = await dbContext.Quizzes.FindAsync(viewModel.QuizId);
            if (quiz != null)
            {
                quiz.Title = viewModel.Title;
                quiz.Content = viewModel.Content;
                quiz.Time = viewModel.Time;
                quiz.Status = viewModel.Status;
                quiz.CreatedAt = viewModel.CreatedAt;
                quiz.CourseId = viewModel.CourseId;

                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await dbContext.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                dbContext.Quizzes.Remove(quiz);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var quiz = await dbContext.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            return View(quiz);
        }
    }
}
