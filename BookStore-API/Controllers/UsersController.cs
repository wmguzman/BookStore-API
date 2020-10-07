using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using BookStore_API.DTOs;
using BookStore_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILoggerService _logger;

        public UsersController(SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IConfiguration config,
            ILoggerService logger)
        {
            _signinManager = signInManager;
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// User Login endpoint
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location} - Intentando Login()");
                var username = userDTO.Username;
                var password = userDTO.Password;
                var result = await _signinManager.PasswordSignInAsync(username, password, false, false);
                if (result.Succeeded)
                {
                    _logger.LogInfo($"{location} - Login fue exitoso");
                    var user = await _userManager.FindByNameAsync(username);
                    var tokenString = await GenerateJSONWebToken(user);
                    return Ok(new { token = tokenString });
                    //return Ok();
                }
                _logger.LogWarn($"{location} - No autorizado");
                return Unauthorized(userDTO);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
            
        }

        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var secutiryKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(secutiryKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));
            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Issuer"], claims, null, expires: DateTime.Now.AddMinutes(5), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(Exception message)
        {
            var location = GetControllerActionNames();
            _logger.LogError($"{location} - {message.Message} - {message.InnerException}");
            return StatusCode(500, "Algo salió mal. Por favor contacte al administrador.");
        }

    }
}
