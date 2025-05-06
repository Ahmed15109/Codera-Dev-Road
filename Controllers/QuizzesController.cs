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
            if (!ModelState.IsValid) return View(viewModel);

            var quiz = new Quizzes
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

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var quizzes = await dbContext.Quizzes.Include(q => q.Course).ToListAsync();
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
        public async Task<IActionResult> Edit(Quizzes viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

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
            var quiz = await dbContext.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.QuizId == id);
            if (quiz == null) return NotFound();
            return View(quiz);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Take(int quizId)
        {
            if (User.IsInRole("Admin")) return RedirectToAction("List");

            var quiz = await dbContext.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null || !quiz.Questions.Any())
                return Content("This quiz has no questions. Please contact the administrator.");

            var viewModel = new TakeQuizViewModel
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.Title,
                Questions = quiz.Questions.Select(q => new TakeQuizViewModel.QuestionWithAnswers
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,
                    Answers = q.Answers.Select(a => new TakeQuizViewModel.AnswerChoice
                    {
                        AnswerId = a.AnswerId,
                        Text = a.Text
                    }).ToList()
                }).ToList()
            };

            return View("Take", viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitAnswers(int quizId, List<TakeQuizViewModel.AnswerSubmission> submissions)
        {
            if (submissions == null || submissions.Any(s => s.SelectedAnswerId == 0))
            {
                ModelState.AddModelError(string.Empty, "Please answer all the questions.");
                return RedirectToAction("Take", new { quizId });
            }

            var identityId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
            if (user == null) return Unauthorized();

            var questions = await dbContext.Questions
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Answers)
                .ToListAsync();

            int score = 0;
            var questionsForView = new List<QuestionViewModel>();

            foreach (var question in questions)
            {
                var correctAnswerId = question.Answers.FirstOrDefault(a => a.IsCorrect)?.AnswerId;
                var userAnswer = submissions.FirstOrDefault(s => s.QuestionId == question.QuestionId);

                if (userAnswer != null && userAnswer.SelectedAnswerId == correctAnswerId)
                    score++;

                var answerViews = question.Answers.Select(a => new AnswerViewModel
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect,
                    IsSelectedByUser = userAnswer != null && userAnswer.SelectedAnswerId == a.AnswerId
                }).ToList();

                questionsForView.Add(new QuestionViewModel
                {
                    Text = question.Text,
                    Answers = answerViews
                });
            }

            var result = new QuizResult
            {
                QuizId = quizId,
                UserId = user.UserId,
                Score = score,
                TakenAt = DateTime.Now
            };

            dbContext.QuizResults.Add(result);
            await dbContext.SaveChangesAsync();

            var resultViewModel = new QuizResultViewModel
            {
                QuizTitle = await dbContext.Quizzes.Where(q => q.QuizId == quizId).Select(q => q.Title).FirstOrDefaultAsync(),
                TotalQuestions = questions.Count,
                Score = score,
                Questions = questionsForView
            };

            return View("Result", resultViewModel);
        }
    }
}
