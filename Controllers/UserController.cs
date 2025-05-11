using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using progect_DEPI.Models;

namespace progect_DEPI.Controllers
{
    

    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
       
        public async Task<IActionResult> Profile()
        {
            string userIdStr = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid User ID in cookie.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfileImage(IFormFile profileImage, int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (profileImage != null && profileImage.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "img");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(user.Picture))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Picture.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var fileName = $"user_{userId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(profileImage.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    user.Picture = $"/uploads/img/{fileName}";
                    user.UpdateAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, newImagePath = user.Picture });
                }

                return Json(new { success = false, message = "No image provided" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: عرض نموذج التعديل
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            string userIdStr = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid User ID in cookie.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: حفظ التعديلات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            string userIdStr = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest("Invalid User ID in cookie.");
            }

            if (userId != user.UserId)
            {
                return BadRequest("Mismatched user ID.");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                // الحصول على بيانات المستخدم الحالية من قاعدة البيانات
                var existingUser = await _context.Users.FindAsync(user.UserId);
                if (existingUser == null)
                {
                    return NotFound();
                }

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
                if (!_context.Users.Any(u => u.UserId == user.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ التعديلات: " + ex.Message);
                return View(user);
            }
        }






    }

}
