using API.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para la autenticación y gestión de usuarios
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(AuthService authService, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Autentica a un usuario y genera un token JWT
        /// </summary>
        /// <param name="request">Credenciales del usuario</param>
        /// <returns>Token JWT para autenticación</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username) ?? 
                       await _userManager.FindByEmailAsync(request.Username);

            if (user == null)
                return Unauthorized("Usuario no encontrado");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized("Email no confirmado");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
                return Unauthorized("Contraseña incorrecta");

            var token = await _authService.GenerateToken(user.UserName);

            return Ok(new { Token = token });
        }
    }

    /// <summary>
    /// Modelo para la solicitud de inicio de sesión
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Nombre de usuario o correo electrónico
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
