using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;

namespace progect_DEPI.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public EnrollmentsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var enrollments = await dbContext.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .ToListAsync();

            return View(enrollments);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Users = new SelectList(dbContext.Users.ToList(), "UserId", "FullName");
            ViewBag.Courses = new SelectList(dbContext.Courses.ToList(), "CourseId", "Title");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(Enrollment enrollment)
        {
            enrollment.EnrolledAt = DateTime.Now;
            await dbContext.Enrollments.AddAsync(enrollment);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("List");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var enrollment = await dbContext.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            ViewBag.Users = new SelectList(dbContext.Users.ToList(), "UserId", "FullName", enrollment.UserId);
            ViewBag.Courses = new SelectList(dbContext.Courses.ToList(), "CourseId", "Title", enrollment.CourseId);
            return View(enrollment);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Enrollment model)
        {
            var enrollment = await dbContext.Enrollments.FindAsync(model.EnrollmentId);
            if (enrollment != null)
            {
                enrollment.Status = model.Status;
                enrollment.PaymentStatus = model.PaymentStatus;
                enrollment.CourseId = model.CourseId;
                enrollment.UserId = model.UserId;
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("List");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await dbContext.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                dbContext.Enrollments.Remove(enrollment);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var enrollment = await dbContext.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null) return NotFound();

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("Admin") || enrollment.User.IdentityId == currentUserId)
            {
                return View(enrollment);
            }

            return Forbid();
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var identityId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
            if (user == null) return Unauthorized();

            bool alreadyEnrolled = await dbContext.Enrollments
                .AnyAsync(e => e.UserId == user.UserId && e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Message"] = "You are already enrolled in this course."; 
                return RedirectToAction("MyEnrollments");
            }

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                UserId = user.UserId,
                EnrolledAt = DateTime.Now,
                Status = "Active",
                PaymentStatus = "Pending"
            };

            dbContext.Enrollments.Add(enrollment);
            await dbContext.SaveChangesAsync();

            TempData["Message"] = "You have been successfully enrolled in the course!";
            return RedirectToAction("MyEnrollments");
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> MyEnrollments()
        {
            var identityId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
            if (user == null) return Unauthorized();

            var enrollments = await dbContext.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == user.UserId)
                .ToListAsync();

            return View(enrollments);
        }
    }
}