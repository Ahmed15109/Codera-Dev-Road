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

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var enrollment = await dbContext.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null) return NotFound();
            return View(enrollment);
        }
    }
}
