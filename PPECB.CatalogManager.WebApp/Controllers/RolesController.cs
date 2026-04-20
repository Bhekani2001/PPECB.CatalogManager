using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.WebApp.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        // GET: Roles/ShowCreateRoleForm
        public IActionResult ShowCreateRoleForm()
        {
            return View();
        }

        // POST: Roles/ProcessCreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCreateRole(CreateRoleDto createDto)
        {
            if (ModelState.IsValid)
            {
                var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == createDto.Name);
                if (existingRole != null)
                {
                    ModelState.AddModelError("", "Role name already exists.");
                    return View("ShowCreateRoleForm", createDto);
                }

                var role = new Role
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    IsSystemRole = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Role '{createDto.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View("ShowCreateRoleForm", createDto);
        }

        // GET: Roles/ShowEditRoleForm/5
        public async Task<IActionResult> ShowEditRoleForm(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            var updateDto = new UpdateRoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            };
            return View(updateDto);
        }

        // POST: Roles/ProcessEditRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEditRole(int id, UpdateRoleDto updateDto)
        {
            if (id != updateDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null) return NotFound();

                // Check if name already exists (excluding current role)
                var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == updateDto.Name && r.Id != id);
                if (existingRole != null)
                {
                    ModelState.AddModelError("", "Role name already exists.");
                    return View("ShowEditRoleForm", updateDto);
                }

                role.Name = updateDto.Name;
                role.Description = updateDto.Description;
                role.UpdatedAt = DateTime.UtcNow;
                role.UpdatedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Role '{updateDto.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View("ShowEditRoleForm", updateDto);
        }

        // GET: Roles/ConfirmDeleteRole/5
        public async Task<IActionResult> ConfirmDeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            // Check if role has users assigned
            var hasUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
            if (hasUsers)
            {
                TempData["Error"] = "Cannot delete role that has users assigned.";
                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }

        // POST: Roles/ProcessDeleteRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Role '{role.Name}' deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Roles/GetRoleDetails/5
        public async Task<IActionResult> GetRoleDetails(int id)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null) return NotFound();
            return View(role);
        }
    }
}