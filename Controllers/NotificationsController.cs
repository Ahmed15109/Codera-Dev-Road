using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;
using System.Security.Claims;

namespace progect_DEPI.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Send(int userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllNotifications");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> SendToAll(string message)
        {
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = user.UserId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("AllNotifications");
        }


        public async Task<IActionResult> MyNotifications()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == user.UserId);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyNotifications));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == user.UserId);
            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyNotifications));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllNotifications()
        {
            var all = await _context.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(all);
        }
    }

}
