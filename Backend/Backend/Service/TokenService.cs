using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.models;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(User user)
        {
            // 1. Get settings from appsettings.json
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] 
            ?? throw new InvalidOperationException("EmailSettings:FromEmail is missing in appsettings.json")
            );

            // 2. Create claims (user data to store in token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", $"{user.Fname} {user.Lname}"),
                new Claim("IsVerified", user.IsVerified.ToString())
            };

            // 3. Create signing key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? throw new InvalidOperationException("EmailSettings:FromEmail is missing in appsettings.json"))) ;
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Create the token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            // 5. Return the token as string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}