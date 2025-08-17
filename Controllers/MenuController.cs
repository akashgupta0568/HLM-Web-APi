using HLM_Web_APi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HLM_Web_APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly MenuRepository _repo;
        public MenuController(MenuRepository repo) => _repo = repo;

        [HttpGet("menus")]
        public async Task<IActionResult> GetMenus()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
            var menus = await _repo.GetMenusForUserAsync(userId);
            return Ok(menus);
        }

        [HttpGet("Main-roles")]
        public async Task<IActionResult> getMainRoles()
        {
            var Roles = await _repo.GetAllMainRolesAsync();
            return Ok(Roles);
        }

        [HttpGet("GetInternalRoles")]
        public async Task<IActionResult> GetInternalUserRoles()
        {
            var internalRoles = await _repo.GetInternalRolesAsync();
            if (internalRoles == null || internalRoles.Count == 0)
            {
                return NotFound(new
                {
                    Message = "No internal roles found.",
                    Success = false
                });
            }
            return Ok(new
            {
                Data = internalRoles,
                Success = true
            });
        }


    }
}
