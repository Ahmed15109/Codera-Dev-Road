using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;

namespace progect_DEPI.Controllers
{
    [Authorize]
    public class QuizResultsController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public QuizResultsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // ✅ عرض نتائج الطالب الحالي
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var identityId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await dbContext.Users
                                      .FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (user == null)
                return Unauthorized();

            var results = await dbContext.QuizResults
                                         .Include(q => q.Quiz)
                                         .Where(r => r.UserId == user.UserId)
                                         .AsNoTracking()  // تحسين الأداء
                                         .ToListAsync();

            return View(results);
        }

        // ✅ عرض كل النتائج (Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var results = await dbContext.QuizResults
                                         .Include(r => r.Quiz)
                                         .Include(r => r.User)
                                         .AsNoTracking()  // تحسين الأداء
                                         .ToListAsync();

            return View(results);
        }

        // ✅ عرض فورم تسجيل نتيجة كويز
        [HttpGet]
        public IActionResult SubmitResult()
        {
            ViewBag.Quizzes = dbContext.Quizzes
                .Select(q => new SelectListItem
                {
                    Value = q.QuizId.ToString(),
                    Text = q.Title
                }).ToList();

            return View();
        }

        // ✅ تسجيل النتيجة فعليًا
        [HttpPost]
        public async Task<IActionResult> SubmitResult(int quizId, int score)
        {
            if (score < 0 || score > 100)  // التأكد من صحة النتيجة
            {
                ModelState.AddModelError("score", "Score must be between 0 and 100.");
                ViewBag.Quizzes = dbContext.Quizzes
                    .Select(q => new SelectListItem
                    {
                        Value = q.QuizId.ToString(),
                        Text = q.Title
                    }).ToList();
                return View();
            }

            var identityId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await dbContext.Users
                                      .FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (user == null)
                return Unauthorized();

            var result = new QuizResult
            {
                QuizId = quizId,
                UserId = user.UserId,
                Score = score,
                TakenAt = DateTime.Now
            };

            dbContext.QuizResults.Add(result);
            await dbContext.SaveChangesAsync();

            TempData["Success"] = "Quiz result submitted successfully!";
            return RedirectToAction("MyResults");
        }

        // ✅ تفاصيل نتيجة معينة
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var result = await dbContext.QuizResults
                                         .Include(r => r.Quiz)
                                         .Include(r => r.User)
                                         .FirstOrDefaultAsync(r => r.QuizResultId == id);

            if (result == null) return NotFound();

            return View(result);
        }

        // ✅ Admin فقط يقدر يحذف نتيجة
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await dbContext.QuizResults.FindAsync(id);
            if (result != null)
            {
                dbContext.QuizResults.Remove(result);
                await dbContext.SaveChangesAsync();
            }

            TempData["Success"] = "Result deleted successfully!";
            return RedirectToAction("List");
        }
    }
}
