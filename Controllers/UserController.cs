using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using progect_DEPI.Models;
using System.Security.Claims;

namespace progect_DEPI.Controllers
{
    

    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private const long MaxProfileImageBytes = 5 * 1024 * 1024;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(identityId))
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileImage(IFormFile profileImage, int userId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                if (currentUser.UserId != userId)
                {
                    return Forbid();
                }

                if (profileImage == null || profileImage.Length == 0)
                {
                    return Json(new { success = false, message = "No image provided" });
                }

                if (profileImage.Length > MaxProfileImageBytes)
                {
                    return Json(new { success = false, message = "Profile images must be 5 MB or smaller." });
                }

                var extension = Path.GetExtension(profileImage.FileName);
                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!AllowedImageExtensions.Contains(extension) || !allowedContentTypes.Contains(profileImage.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Unsupported image type." });
                }

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "img");
                Directory.CreateDirectory(uploadsDir);

                if (!string.IsNullOrEmpty(currentUser.Picture))
                {
                    var oldImagePath = Path.Combine(uploadsDir, Path.GetFileName(currentUser.Picture));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var fileName = $"user_{userId}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.CreateNew))
                {
                    await profileImage.CopyToAsync(stream);
                }

                currentUser.Picture = $"/uploads/img/{fileName}";
                currentUser.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, newImagePath = currentUser.Picture });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Unable to update the profile image." });
            }
        }

        // GET: عرض نموذج التعديل
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            return View(user);
        }

        // POST: حفظ التعديلات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.UserId != user.UserId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                // الحصول على بيانات المستخدم الحالية من قاعدة البيانات
                var existingUser = currentUser;

                // تحديث الخصائص النصية فقط
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                // يمكنك إضافة المزيد من الخصائص هنا حسب الحاجة

                // الاحتفاظ بقيمة الصورة الحالية
                user.Picture = existingUser.Picture;

                // تحديث تاريخ التعديل
                existingUser.UpdateAt = DateTime.Now;

                // حفظ التغييرات
                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Profile));
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save the profile changes.");
                return View(user);
            }
        }






    }

}
