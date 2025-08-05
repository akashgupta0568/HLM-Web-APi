using DocumentFormat.OpenXml.Spreadsheet;
using HLM_Web_APi.DTO;
using HLM_Web_APi.Services;

//using HLM_Web_APi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HLM_Web_APi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly MenuRepository _repo;
        public AdminController(MenuRepository repo) => _repo = repo;

        private async Task<bool> EnsureAdminAsync()
        {
            var userId = 74;
            //var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (!int.TryParse(userIdStr, out var userId)) return false;
            return await _repo.IsUserInRoleAsync(userId, "Admin");
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            //if (!await EnsureAdminAsync()) return Forbid();
            var roles = await _repo.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("menus")]
        public async Task<IActionResult> GetMenus()
        {
            //if (!await EnsureAdminAsync()) return Forbid();
            var menus = await _repo.GetAllMenusAsync();
            return Ok(menus);
        }

        [HttpGet("role-permissions/{roleId}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            //if (!await EnsureAdminAsync()) return Forbid();
            var perms = await _repo.GetRolePermissionsAsync(roleId);
            return Ok(perms);
        }

        [HttpPost("update-role-permission")]
        public async Task<IActionResult> UpdateRolePermission([FromBody] RolePermissionUpdate req)
        {
            //if (!await EnsureAdminAsync()) return Forbid();
            await _repo.UpdateRolePermissionAsync(req.RoleId, req.PermissionKey, req.IsGranted);
            return Ok();
        }

        [HttpPost("set-menu-active")]
        public async Task<IActionResult> SetMenuActive([FromBody] dynamic body)
        {
            //if (!await EnsureAdminAsync()) return Forbid();
            int menuId = (int)body.menuId;
            bool isActive = (bool)body.isActive;
            await _repo.SetMenuActiveAsync(menuId, isActive);
            return Ok();
        }
    }
}
