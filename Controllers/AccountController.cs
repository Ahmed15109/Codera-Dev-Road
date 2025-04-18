using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using progect_DEPI.Models; // استبدل بـ namespace بتاعك
using progect_DEPI.ViewModels; // هنعمل ViewModels دلوقتي

namespace progect_DEPI.Controllers
{
	

public class AccountController : Controller
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;

		public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		// GET: /Account/Register
		[HttpGet]
		public IActionResult Register()
		{
			return View("Register");
		}

		// POST: /Account/Register
		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new IdentityUser { UserName = model.Email, Email = model.Email };
				var result = await _userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);
					return RedirectToAction("Index", "Home");
				}

				foreach (var error in result.Errors)
					ModelState.AddModelError("", error.Description);
			}

			return View(model);
		}

		// GET: /Account/Login
		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		// POST: /Account/Login
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model , string ReturnUrl= "~/Home/Index")
        {
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(
					model.Email,
					model.Password,
					model.RememberMe,
					lockoutOnFailure: false
				);

				if (result.Succeeded)
					return LocalRedirect(ReturnUrl);

				ModelState.AddModelError("", "Invalid login attempt.");
			}

			return View(model);
		}

		// POST: /Account/Logout
		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login", "Account");
		}
	}

}

