using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;
using System.Security.Claims;

namespace progect_DEPI.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // تسجيل الطالب في كورس باستخدام UserId من الكوكيز
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            // 👇 نبدأ باستخدام الكوكيز هنا
            string userIdStr = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid User ID in cookie.");
            }

            // 👇 التحقق من وجود المستخدم في جدول Users
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 👇 التحقق من وجود الكورس
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // 👇 التحقق من عدم التسجيل المسبق
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existingEnrollment != null)
            {
                TempData["Message"] = "أنت مسجل بالفعل في هذا الكورس";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            // 👇 إنشاء تسجيل جديد
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.Now,
                Status = "Active",
                PaymentStatus = "Pending"
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم التسجيل في الكورس بنجاح";
            return RedirectToAction("CourseContent", new { courseId });
        }

        // عرض محتوى الكورس بعد التسجيل
        [HttpGet]
        public async Task<IActionResult> CourseContent(int courseId)
        {
            string userIdStr = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 👇 التحقق من تسجيل الطالب في الكورس
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.Status == "Active");

            if (!isEnrolled)
            {
                TempData["Error"] = "يجب التسجيل في الكورس أولاً";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            // 👇 جلب محتوى الكورس مع الدروس مرتبة
            var courseContent = await _context.Courses
                .Include(c => c.Lessons.OrderBy(l => l.OrderNumber))
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            ViewBag.CourseId = courseId;
            return View(courseContent);
        }

        // عرض جميع الكورسات المسجل فيها الطالب
        public async Task<IActionResult> MyCourses()
        {
            string userIdStr = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var enrolledCourses = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Select(e => e.Course)
                .ToListAsync();

            return View(enrolledCourses);
        }

        // عرض محتوى درس معين
        public async Task<IActionResult> LessonContent(int lessonId)
        {
            string userIdStr = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            // 👇 التحقق من تسجيل الطالب في الكورس التابع لهذا الدرس
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == lesson.CourseId && e.Status == "Active");

            if (!isEnrolled)
            {
                TempData["Error"] = "غير مصرح لك بالوصول لهذا الدرس";
                return RedirectToAction("MyCourses");
            }

            ViewBag.PreviousLesson = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId && l.OrderNumber < lesson.OrderNumber)
                .OrderByDescending(l => l.OrderNumber)
                .FirstOrDefaultAsync();

            ViewBag.NextLesson = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId && l.OrderNumber > lesson.OrderNumber)
                .OrderBy(l => l.OrderNumber)
                .FirstOrDefaultAsync();

            return View(lesson);
        }
    }
}