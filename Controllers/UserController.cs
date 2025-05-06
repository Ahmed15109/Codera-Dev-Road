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

        
        public async Task<IActionResult> Profile(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            return View("Profile", user);
        }

        
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();


            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    user.UpdateAt = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Profile), new { id = user.UserId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(u => u.UserId == id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(user);
        }

        
        

        
        
    }

}
