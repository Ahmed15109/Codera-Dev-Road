using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;
using progect_DEPI.ViewModels;

namespace progect_DEPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public QuestionsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Add(int? quizId)
        {
            var quizzes = dbContext.Quizzes
                .Select(q => new SelectListItem
                {
                    Value = q.QuizId.ToString(),
                    Text = q.Title
                }).ToList();

            var model = new AddQuestionViewModel
            {
                QuizId = quizId ?? 0,
                Quizzes = quizzes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddQuestionViewModel model)
        {
            model.Quizzes = dbContext.Quizzes
                .Select(q => new SelectListItem
                {
                    Value = q.QuizId.ToString(),
                    Text = q.Title
                }).ToList();

            if (!ModelState.IsValid)
                return View(model);

            var question = new Question
            {
                Text = model.Text,
                QuizId = model.QuizId,
                Answers = new List<Answer>()
            };

            for (int i = 0; i < model.Answers.Count; i++)
            {
                question.Answers.Add(new Answer
                {
                    Text = model.Answers[i],
                    IsCorrect = (i == model.CorrectAnswerIndex)
                });
            }

            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();

            TempData["Success"] = "Question added successfully!";
            return RedirectToAction("Add", new { quizId = model.QuizId });
        }

        [HttpGet]
        public async Task<IActionResult> QuizQuestions(int quizId)
        {
            var quiz = await dbContext.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null)
                return NotFound();

            return View(quiz);
        }
    }
}
