using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.IBusinessLogic;

namespace PPECB.CatalogManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserBusinessLogic _userBusinessLogic;

        public UsersController(IUserBusinessLogic userBusinessLogic)
        {
            _userBusinessLogic = userBusinessLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userBusinessLogic.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userBusinessLogic.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });
            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userBusinessLogic.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = $"User with email {email} not found." });
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "User created successfully.", email = result.Email, username = result.Username });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                await _userBusinessLogic.ActivateUserAsync(id, "API");
            else
                await _userBusinessLogic.DeactivateUserAsync(id, "API");

            return Ok(new { message = "User updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userBusinessLogic.DeactivateUserAsync(id, "API");
            return Ok(new { message = "User deactivated successfully." });
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockUser(int id, [FromQuery] int minutes = 30)
        {
            await _userBusinessLogic.LockUserAsync(id, minutes, "API");
            return Ok(new { message = $"User locked for {minutes} minutes." });
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> UnlockUser(int id)
        {
            await _userBusinessLogic.UnlockUserAsync(id, "API");
            return Ok(new { message = "User unlocked successfully." });
        }
    }
}