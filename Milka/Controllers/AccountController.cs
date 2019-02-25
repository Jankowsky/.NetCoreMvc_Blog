using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Milka.Models;

namespace Milka.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userMgr, SignInManager<AppUser> signMgr)
        {
            _userManager = userMgr;
            _signInManager = signMgr;
        }


        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        
        [Authorize]
        public async Task<IActionResult> Logout (string returnUrl)
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl ?? "/");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login ( LoginModel details, string returnUrl)
        {
            if(ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(details.Email);
                if(user != null)
                {
                    await _signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, details.Password, false, false);
                    if(result.Succeeded)
                    {
                        return Redirect(returnUrl ?? "/");
                    }
                }

                ModelState.AddModelError(nameof(LoginModel.Email), "Bad Login or password.");
            }
            return View(details);
        }
    }
}