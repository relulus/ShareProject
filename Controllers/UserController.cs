using ApiPhantom.Service.Interfaces;
using ApiPhantom.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ApiPhantom.Web.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            var result = await _userService.AddUserAsync(user);
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new { Message = "User added successfully." });
        }

        [HttpGet("{id}/interceptions")]
        public async Task<IActionResult> GetUserInterceptions(Guid id)
        {
            var interceptions = await _userService.GetUserInterceptionsAsync(id);
            if (interceptions == null || interceptions.Services.Count == 0)
                return NotFound(new { Message = "No interceptions found for this user." });

            return Ok(interceptions);
        }
    }
}
