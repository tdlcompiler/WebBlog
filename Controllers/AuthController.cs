using Microsoft.AspNetCore.Mvc;
using WebBlog.Exceptions;
using Microsoft.AspNetCore.Authorization;
using WebBlog.Models.Requests;

/// <summary>
/// Контроллер пользователей.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    /// <param name="registerModel">Модель данных для регистрации, содержащая email, пароль и роль.</param>
    /// <returns>Токены для доступа.</returns>
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequestModel registerModel)
    {
        try
        {
            var newUser = _authService.Register(registerModel.Email, registerModel.Password, registerModel.Role);
            var tokens = _authService.GenerateTokens(newUser);

            return Ok(new { Message = "Registration successful", Tokens = tokens });
        }
        catch (Forbidden403Exception ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
        }
        catch (BadRequest400Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
        }
    }

    /// <summary>
    /// Вход в систему.
    /// </summary>
    /// <param name="loginModel">Модель данных для входа, содержащая email и пароль.</param>
    /// <returns>Токены для доступа.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public IActionResult Login([FromBody] LoginRequestModel loginModel)
    {
        try
        {
            var user = _authService.Authenticate(loginModel.Email, loginModel.Password);
            var tokens = _authService.GenerateTokens(user);

            return Ok(new { Message = "Login successful", Tokens = tokens });
        }
        catch (Forbidden403Exception ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
        }
    }

    /// <summary>
    /// Обновление токена.
    /// </summary>
    /// <param name="refreshTokenModel">Модель данных для обновления accessToken, содержащая refreshToken.</param>
    /// <returns>Токены для доступа.</returns>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequestModel refreshTokenModel)
    {
        try
        {
            var user = _authService.ValidateRefreshToken(refreshTokenModel.RefreshToken);
            var newTokens = _authService.GenerateTokens(user);

            return Ok(new { Tokens = newTokens });
        }
        catch (BadRequest400Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
        }
    }
}
