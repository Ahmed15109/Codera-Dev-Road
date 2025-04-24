using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using progect_DEPI.Models;
using progect_DEPI.ViewModels;
using Rotativa.AspNetCore;

namespace progect_DEPI.Controllers
{
    public class CertificatesController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public CertificatesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
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
        public async Task<IActionResult> Add(AddCertificateViewModel viewModel)
        {
            var certificate = new Certificate
            {
                Content = viewModel.Content,
                CreatedAt = viewModel.CreatedAt,
                UserId = viewModel.UserId
            };

            await dbContext.Certificates.AddAsync(certificate);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("List", "Certificates");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var certificates = await dbContext.Certificates
                .Include(c => c.User)
                .ToListAsync();

            return View(certificates);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var certificate = await dbContext.Certificates.FindAsync(id);
            if (certificate == null) return NotFound();

            ViewBag.Users = new SelectList(dbContext.Users.ToList(), "UserId", "FullName", certificate.UserId);
            return View(certificate);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Certificate viewModel)
        {
            var certificate = await dbContext.Certificates.FindAsync(viewModel.CertificateId);
            if (certificate != null)
            {
                certificate.Content = viewModel.Content;
                certificate.CreatedAt = viewModel.CreatedAt;
                certificate.UserId = viewModel.UserId;
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var certificate = await dbContext.Certificates.FindAsync(id);
            if (certificate != null)
            {
                dbContext.Certificates.Remove(certificate);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var certificate = await dbContext.Certificates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (certificate == null) return NotFound();

            return View(certificate);
        }

        [HttpGet]
        public IActionResult Print(int id)
        {
            var certificate = dbContext.Certificates
                .Include(c => c.User)
                .FirstOrDefault(c => c.CertificateId == id);

            if (certificate == null)
                return NotFound();

            return new ViewAsPdf("Print", certificate)
            {
                FileName = $"Certificate_{certificate.User?.FullName}.pdf"
            };
        }
    }
}
