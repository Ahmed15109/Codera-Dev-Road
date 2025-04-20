using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;

namespace progect_DEPI.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

     
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

        
        public async Task<IActionResult> MyNotifications(int? userId)
        {
            if (userId == null) return NotFound();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

 
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("MyNotifications", new { userId = notification.UserId });
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyNotifications", new { userId = notification.UserId });
        }

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
