using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using WebBlog.Exceptions;
using WebBlog.Models;
using WebBlog.Repository.UserRepository;

public class AuthService
{
    private readonly string _userFilePath;
    private readonly IConfiguration _configuration;
    private IUserRepository _userRepository;

    public AuthService(string userFilePath, IConfiguration configuration)
    {
        _userFilePath = userFilePath;
        _configuration = configuration;
        _userRepository = new FileUserRepository(userFilePath);
    }

    public UserModel Register(string email, string password, string role)
    {
        if (!Regex.IsMatch(email, @"^\S+@\S+\.\S+$"))
            throw new BadRequest400Exception("Invalid email format.");
        if (string.IsNullOrEmpty(password))
            throw new BadRequest400Exception("Empty password.");
        if (string.IsNullOrEmpty(role) || (role != "Reader" && role != "Author"))
            throw new BadRequest400Exception("Invalid role.");
        if (_userRepository.UserExistsByEmail(email))
            throw new Forbidden403Exception("Email already exists.");

        var newUser = new UserModel
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        };

        _userRepository.AddUser(newUser);

        return newUser;
    }

    public UserModel Authenticate(string email, string password)
    {
        var user = _userRepository.GetUserByEmail(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new Forbidden403Exception("Invalid credentials.");

        return user;
    }

    public string GenerateRefreshToken(UserModel user)
    {
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        _userRepository.UpdateUser(user);

        return refreshToken;
    }

    public object GenerateTokens(UserModel user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

        return new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private string GenerateAccessToken(UserModel user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new Exception("JWT Secret not found."));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public UserModel ValidateRefreshToken(string refreshToken)
    {
        var user = _userRepository.GetUserByRefreshToken(refreshToken);
        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new BadRequest400Exception("Invalid or expired refresh token.");

        return user;
    }
}