using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.models;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly User _testUser;

    public TokenServiceTests()
    {
        // Arrange: Create a real configuration with JwtSettings
        var config = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
          ["JwtSettings:Secret"] = "this-is-a-test-secret-key-for-unit-tests-32chars",
          ["JwtSettings:Issuer"] = "TestIssuer",
          ["JwtSettings:Audience"] = "TestAudience",
          ["JwtSettings:ExpiryMinutes"] = "60"
      })
     .Build();
        _tokenService = new TokenService(config);

        _testUser = new User
        {
            Id = 99,
            Email = "test@unittest.com",
            Fname = "Unit",
            Lname = "Test",
            IsVerified = true
        };
    }

    [Fact]
    public void CreateToken_ReturnsNonNullString()
    {
        // Act
        var token = _tokenService.CreateToken(_testUser);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void CreateToken_TokenContainsUserIdClaim()
    {
        // Act
        var token = _tokenService.CreateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        // Assert
        var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(userIdClaim);
        Assert.Equal("99", userIdClaim.Value);
    }

    [Fact]
    public void CreateToken_TokenContainsEmailClaim()
    {
        // Act
        var token = _tokenService.CreateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        // Assert
        var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal("test@unittest.com", emailClaim.Value);
    }

    [Fact]
    public void CreateToken_TokenContainsFullNameClaim()
    {
        // Act
        var token = _tokenService.CreateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        // Assert
        var fullNameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "FullName");
        Assert.NotNull(fullNameClaim);
        Assert.Equal("Unit Test", fullNameClaim.Value);
    }

    [Fact]
    public void CreateToken_TokenContainsIsVerifiedClaim()
    {
        // Act
        var token = _tokenService.CreateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        // Assert
        var isVerifiedClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "IsVerified");
        Assert.NotNull(isVerifiedClaim);
        Assert.Equal("True", isVerifiedClaim.Value);
    }

    [Fact]
    public void CreateToken_TokenExpiresInFuture()
    {
        // Act
        var token = _tokenService.CreateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        // Assert
        Assert.True(jsonToken.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public void CreateToken_DifferentUsers_DifferentTokens()
    {
        // Arrange
        var user2 = new User
        {
            Id = 100,
            Email = "user2@test.com",
            Fname = "Another",
            Lname = "User",
            IsVerified = false
        };

        // Act
        var token1 = _tokenService.CreateToken(_testUser);
        var token2 = _tokenService.CreateToken(user2);

        // Assert
        Assert.NotEqual(token1, token2);
    }
}