using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Create(int courseId)
        {
            var viewModel = new AddLessonViewModel
            {
                CourseId = courseId
            };
            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddLessonViewModel model, IFormFile videoFile, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

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

            dbContext.Lessons.Add(lesson);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { id = model.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var lessons = await dbContext.Lessons.ToListAsync();
            return View(lessons);
        }
    }
}
