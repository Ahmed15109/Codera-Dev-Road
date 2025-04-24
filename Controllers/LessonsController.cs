using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;
using progect_DEPI.ViewModels;

namespace progect_DEPI.Controllers
{
    public class LessonsController : Controller
    {
            private readonly ApplicationDbContext dbContext;

            public LessonsController(ApplicationDbContext dbContext)
            {
                this.dbContext = dbContext;
            }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create(int? courseId = null)
        {
            var viewModel = new AddLessonViewModel
            {
                CourseId = courseId ?? 0,
                Courses = dbContext.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseId.ToString(),
                        Text = c.Title
                    }).ToList()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(AddLessonViewModel model, IFormFile videoFile, IFormFile imageFile)
            {


            string videoPath = null;
                string imagePath = null;

                // رفع الفيديو
                if (videoFile != null && videoFile.Length > 0)
                {
                    var videoFileName = Path.GetFileName(videoFile.FileName);
                    var videoSavePath = Path.Combine("wwwroot/uploads/videos", videoFileName);

                    using (var stream = new FileStream(videoSavePath, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(stream);
                    }

                    videoPath = "/uploads/videos/" + videoFileName;
                }

                // رفع الصورة
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageFileName = Path.GetFileName(imageFile.FileName);
                    var imageSavePath = Path.Combine("wwwroot/uploads/images", imageFileName);

                    using (var stream = new FileStream(imageSavePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    imagePath = "/uploads/images/" + imageFileName;
                }

                // إنشاء Lesson جديد
                var lesson = new Lesson
                {
                    Title = model.Title,
                    Content = model.Content,
                    OrderNumber = model.OrderNumber,
                    Level = model.Level,
                    CourseId = model.CourseId,
                    VideoUrl = videoPath,
                    ImageUrl = imagePath
                };
            if (ModelState.IsValid)
            {
                dbContext.Lessons.Add(lesson);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("CourseLessons", new { courseId = lesson.CourseId });
            }

            dbContext.Lessons.Add(lesson);
                await dbContext.SaveChangesAsync();


            return RedirectToAction("CourseLessons", "Lessons", new { courseId = model.CourseId });
        }
        [HttpGet]
        public async Task<IActionResult> CourseLessons(int courseId)
        {
            var course = await dbContext.Courses
    .Include(c => c.Lessons)
    .FirstOrDefaultAsync(c => c.CourseId == courseId);


            if (course == null)
                return NotFound();

            return View(course);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await dbContext.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            var viewModel = new AddLessonViewModel
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Content = lesson.Content,
                OrderNumber = lesson.OrderNumber,
                Level = lesson.Level,
                CourseId = lesson.CourseId
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddLessonViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var lesson = await dbContext.Lessons.FindAsync(model.LessonId);
            if (lesson == null) return NotFound();

            lesson.Title = model.Title;
            lesson.Content = model.Content;
            lesson.OrderNumber = model.OrderNumber;
            lesson.Level = model.Level;

            await dbContext.SaveChangesAsync();

            return RedirectToAction("CourseLessons", new { courseId = lesson.CourseId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await dbContext.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            dbContext.Lessons.Remove(lesson);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("CourseLessons", new { courseId = lesson.CourseId });
        }

    }
}
