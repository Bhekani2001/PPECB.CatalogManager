using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserBusinessLogic _userBusinessLogic;

        public AccountController(IUserBusinessLogic userBusinessLogic)
        {
            _userBusinessLogic = userBusinessLogic;
        }

        public IActionResult ShowLoginForm(string returnUrl = "/")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessLogin(LoginDto loginDto, string returnUrl = "/")
        {
            if (ModelState.IsValid)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                var result = await _userBusinessLogic.LoginAsync(loginDto, ipAddress, userAgent);

                if (result.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.Email),
                        new Claim(ClaimTypes.Email, result.Email),
                        new Claim(ClaimTypes.Name, result.Username),
                        new Claim("FullName", result.FullName),
                        new Claim(ClaimTypes.Role, result.Role)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    TempData["Success"] = $"Welcome back, {result.FullName}!";
                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError("", result.Message);
            }
            return View("ShowLoginForm", loginDto);
        }

        public IActionResult ShowRegisterForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessRegister(RegisterDto registerDto)
        {
            if (ModelState.IsValid)
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _userBusinessLogic.RegisterAsync(registerDto, ipAddress);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(ShowLoginForm));
                }

                ModelState.AddModelError("", result.Message);
            }
            return View("ShowRegisterForm", registerDto);
        }

        public async Task<IActionResult> ProcessLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction(nameof(ShowLoginForm));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> ShowUserProfile()
        {
            var email = User.Identity?.Name;
            if (email == null) return RedirectToAction(nameof(ShowLoginForm));

            var user = await _userBusinessLogic.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessUpdateProfile(UpdateProfileDto updateDto)
        {
            var email = User.Identity?.Name;
            if (email == null) return RedirectToAction(nameof(ShowLoginForm));

            var user = await _userBusinessLogic.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            await _userBusinessLogic.UpdateUserProfileAsync(user.Id, updateDto);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(ShowUserProfile));
        }

        public IActionResult ShowChangePasswordForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessChangePassword(ChangePasswordDto changePasswordDto)
        {
            var email = User.Identity?.Name;
            if (email == null) return RedirectToAction(nameof(ShowLoginForm));

            var user = await _userBusinessLogic.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            var result = await _userBusinessLogic.ChangePasswordAsync(user.Id, changePasswordDto);

            if (result)
            {
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction(nameof(ShowUserProfile));
            }

            ModelState.AddModelError("", "Current password is incorrect.");
            return View("ShowChangePasswordForm", changePasswordDto);
        }
    }
}