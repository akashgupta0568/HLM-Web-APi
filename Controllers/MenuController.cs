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

        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            //var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var menus = await _repo.GetMenusForUserAsync(69);
            return Ok(menus);
        }


    }
}
