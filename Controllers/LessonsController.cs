using Microsoft.AspNetCore.Authorization;
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

            private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".webm", ".ogg", ".mov"
            };

            private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp"
            };

            private const long MaxVideoBytes = 100 * 1024 * 1024;
            private const long MaxImageBytes = 10 * 1024 * 1024;

            public LessonsController(ApplicationDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

        private List<SelectListItem> GetCourses()
        {
            return dbContext.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.Title
                }).ToList();
        }

        private static bool IsAllowedUpload(IFormFile file, HashSet<string> allowedExtensions, long maxBytes, params string[] allowedContentTypes)
        {
            var extension = Path.GetExtension(file.FileName);
            return file.Length <= maxBytes
                && allowedExtensions.Contains(extension)
                && allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);
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
            public async Task<IActionResult> Create(AddLessonViewModel model, IFormFile? videoFile, IFormFile? imageFile)
            {
            if (!ModelState.IsValid)
            {
                model.Courses = GetCourses();
                return View(model);
            }

            string? videoPath = null;
            string? imagePath = null;

                // رفع الفيديو
                if (videoFile != null && videoFile.Length > 0)
                {
                    if (!IsAllowedUpload(videoFile, AllowedVideoExtensions, MaxVideoBytes,
                        "video/mp4", "video/webm", "video/ogg", "video/quicktime"))
                    {
                        ModelState.AddModelError(nameof(videoFile), "Upload an MP4, WebM, OGG, or MOV video up to 100 MB.");
                    }
                    else
                    {
                        var videosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
                        Directory.CreateDirectory(videosDirectory);
                        var extension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
                        var videoFileName = $"{Guid.NewGuid():N}{extension}";
                        var videoSavePath = Path.Combine(videosDirectory, videoFileName);

                        using (var stream = new FileStream(videoSavePath, FileMode.CreateNew))
                        {
                            await videoFile.CopyToAsync(stream);
                        }

                        videoPath = "/uploads/videos/" + videoFileName;
                    }
                }

                // رفع الصورة
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!IsAllowedUpload(imageFile, AllowedImageExtensions, MaxImageBytes,
                        "image/jpeg", "image/png", "image/gif", "image/webp"))
                    {
                        ModelState.AddModelError(nameof(imageFile), "Upload a JPG, PNG, GIF, or WebP image up to 10 MB.");
                    }
                    else
                    {
                        var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images");
                        Directory.CreateDirectory(imagesDirectory);
                        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        var imageFileName = $"{Guid.NewGuid():N}{extension}";
                        var imageSavePath = Path.Combine(imagesDirectory, imageFileName);

                        using (var stream = new FileStream(imageSavePath, FileMode.CreateNew))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        imagePath = "/uploads/images/" + imageFileName;
                    }
                }

                if (!ModelState.IsValid)
                {
                    model.Courses = GetCourses();
                    return View(model);
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
            return RedirectToAction("CourseLessons", new { courseId = lesson.CourseId });
        }
        [HttpGet]
        public IActionResult CourseLessons(int courseId)
        {
            var course = dbContext.Courses
                .Include(c => c.Lessons.OrderBy(l => l.OrderNumber))
                .FirstOrDefault(c => c.CourseId == courseId);

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

        public IActionResult Details(int id)
        {
            var lesson = dbContext.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson == null)
                return NotFound();

            return View(lesson);
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
