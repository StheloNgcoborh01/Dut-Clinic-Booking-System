using Xunit;
using Backend.Services;

namespace Backend.Service;

public class AuthServiceTests
{
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(null);
    }

    [Fact]
    public async Task CheckPasswordStrength_TooShort_ReturnsFalse()
    {
        string password = "12345";
        bool result = await _authService.CheckPasswordStrength(password);
        Assert.False(result);
    }

    [Fact]
    public async Task CheckPasswordStrength_NoCapitalLetter_ReturnsFalse()
    {
        string password = "password123";
        bool result = await _authService.CheckPasswordStrength(password);
        Assert.False(result);
    }

    [Fact]
    public async Task CheckPasswordStrength_StrongPassword_ReturnsTrue()
    {
        string password = "StrongP@ssw0rd123";
        bool result = await _authService.CheckPasswordStrength(password);
        Assert.True(result);
    }

    [Fact]
    public async Task CheckEmptyField_EmptyString_ReturnsTrue()
    {
        string emptyString = "";
        bool result = await _authService.CheckEmptyField(emptyString);
        Assert.True(result);
    }

    [Fact]
    public async Task CheckEmptyField_NullString_ReturnsTrue()
    {
        string nullString = null;
        bool result = await _authService.CheckEmptyField(nullString);
        Assert.True(result);
    }

    [Fact]
    public async Task CheckEmptyField_ValidString_ReturnsFalse()
    {
        string validString = "Asanda";
        bool result = await _authService.CheckEmptyField(validString);
        Assert.False(result);
    }

    [Fact]
    public async Task HashPassword_ReturnsDifferentHashForSamePassword()
    {
        string password = "MySecret123";
        string hash1 = await _authService.HashPassword(password);
        string hash2 = await _authService.HashPassword(password);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async Task HashPassword_ReturnsNonNullString()
    {
        string password = "MySecret123";
        string hash = await _authService.HashPassword(password);
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public async Task GenerateRandom_ReturnsSixDigitCode()
    {
        string code = await _authService.GenerateRandom();
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.True(int.TryParse(code, out _));
    }
}





