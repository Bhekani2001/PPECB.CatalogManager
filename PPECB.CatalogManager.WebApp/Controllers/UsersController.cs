using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserBusinessLogic _userBusinessLogic;

        public UsersController(IUserBusinessLogic userBusinessLogic)
        {
            _userBusinessLogic = userBusinessLogic;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userBusinessLogic.GetAllUsersAsync();
            return View(users);
        }

        // GET: Users/ShowCreateUserForm
        public IActionResult ShowCreateUserForm()
        {
            return View();
        }

        // POST: Users/ProcessCreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCreateUser(CreateUserDto createDto)
        {
            if (ModelState.IsValid)
            {
                var registerDto = new RegisterDto
                {
                    Email = createDto.Email,
                    Username = createDto.Username,
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    Password = createDto.Password,
                    ConfirmPassword = createDto.Password,
                    PhoneNumber = createDto.PhoneNumber,
                    MobileNumber = createDto.MobileNumber
                };

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _userBusinessLogic.RegisterAsync(registerDto, ipAddress);

                if (result.Success)
                {
                    TempData["Success"] = $"User {createDto.Username} created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Message);
            }
            return View("ShowCreateUserForm", createDto);
        }

        // GET: Users/ShowEditUserForm/5
        public async Task<IActionResult> ShowEditUserForm(int id)
        {
            var user = await _userBusinessLogic.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var updateDto = new UpdateUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                MobileNumber = user.MobileNumber,
                Department = user.Department,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive
            };
            return View(updateDto);
        }

        // POST: Users/ProcessEditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEditUser(int id, UpdateUserDto updateDto)
        {
            if (id != updateDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _userBusinessLogic.UpdateUserProfileAsync(id, new UpdateProfileDto
                {
                    FirstName = updateDto.FirstName,
                    LastName = updateDto.LastName,
                    PhoneNumber = updateDto.PhoneNumber,
                    MobileNumber = updateDto.MobileNumber,
                    Department = updateDto.Department,
                    JobTitle = updateDto.JobTitle
                });

                if (updateDto.IsActive)
                {
                    await _userBusinessLogic.ActivateUserAsync(id, User.Identity?.Name ?? "System");
                }
                else
                {
                    await _userBusinessLogic.DeactivateUserAsync(id, User.Identity?.Name ?? "System");
                }

                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View("ShowEditUserForm", updateDto);
        }

        // GET: Users/GetUserDetails/5
        public async Task<IActionResult> GetUserDetails(int id)
        {
            var user = await _userBusinessLogic.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Users/ToggleUserStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _userBusinessLogic.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (user.IsActive)
            {
                await _userBusinessLogic.DeactivateUserAsync(id, User.Identity?.Name ?? "System");
                TempData["Success"] = $"User {user.Username} deactivated.";
            }
            else
            {
                await _userBusinessLogic.ActivateUserAsync(id, User.Identity?.Name ?? "System");
                TempData["Success"] = $"User {user.Username} activated.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/LockUser/5
        [HttpPost]
        public async Task<IActionResult> LockUser(int id, int minutes = 30)
        {
            var user = await _userBusinessLogic.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            await _userBusinessLogic.LockUserAsync(id, minutes, User.Identity?.Name ?? "System");
            TempData["Success"] = $"User {user.Username} locked for {minutes} minutes.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/UnlockUser/5
        [HttpPost]
        public async Task<IActionResult> UnlockUser(int id)
        {
            var user = await _userBusinessLogic.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            await _userBusinessLogic.UnlockUserAsync(id, User.Identity?.Name ?? "System");
            TempData["Success"] = $"User {user.Username} unlocked.";
            return RedirectToAction(nameof(Index));
        }
    }
}