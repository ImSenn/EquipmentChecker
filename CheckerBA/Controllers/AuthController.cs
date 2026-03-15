using CheckerBA.Application.DTOs;
using CheckerBA.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckerBA.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>Đăng nhập → trả về JWT</summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
        {
            var result = await _authService.LoginAsync(req);
            if (result is null)
            {
                _logger.LogWarning("[AUTH] Login thất bại cho user: {Username}", req.Username);
                return Unauthorized(new { message = "Sai username hoặc password." });
            }
            return Ok(result);
        }

        /// <summary>Tạo tài khoản – chỉ Admin mới được gọi</summary>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest req)
        {
            var result = await _authService.RegisterAsync(req);
            if (result is null)
                return BadRequest(new { message = "Username đã tồn tại hoặc role không hợp lệ." });

            _logger.LogInformation("[AUTH] Tạo tài khoản mới: {Username} / {Role}", req.Username, req.Role);
            return Created($"/api/auth/{req.Username}", result);
        }
    }
}
