using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using UniManage.Models;
using UniManage.ViewModels.Account;
using UniManage.Helpers;

namespace UniManage.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToDashboard();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByNameAsync(model.Email);
                    AuditHelper.Log(user.Id, "Login", "User", user.Id, "User logged in successfully", Request);
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return RedirectToDashboard();
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email, 
                    FirstName = model.FirstName, 
                    LastName = model.LastName, 
                    DateOfBirth = model.DateOfBirth,
                    NIC = model.NIC,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Student");
                    AuditHelper.Log(user.Id, "Register", "User", user.Id, "New student registered via public form", Request);
                    
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToDashboard();
                }
                AddErrors(result);
            }
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            string userId = User.Identity.GetUserId();
            AuditHelper.Log(userId, "Logout", "User", userId, "User logged out", Request);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/Profile
        public async new Task<ActionResult> Profile()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var model = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                NIC = user.NIC,
                Address = user.Address,
                Bio = user.Bio,
                ProfilePicturePath = user.ProfilePicturePath,
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        // POST: /Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.NIC = model.NIC;
                user.Address = model.Address;
                user.Bio = model.Bio;
                user.PhoneNumber = model.PhoneNumber;

                if (model.ProfilePicture != null)
                {
                    user.ProfilePicturePath = FileHelper.SaveFile(model.ProfilePicture, "profiles");
                }

                var result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    AuditHelper.Log(user.Id, "UpdateProfile", "User", user.Id, "User updated their profile", Request);
                    return RedirectToAction("Profile");
                }
                AddErrors(result);
            }
            return View("Profile", model);
        }

        private ActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Administrator")) return RedirectToAction("Dashboard", "Admin");
            if (User.IsInRole("Lecturer")) return RedirectToAction("Dashboard", "Lecturer");
            return RedirectToAction("Dashboard", "Student");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToDashboard();
        }

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
    }
}
