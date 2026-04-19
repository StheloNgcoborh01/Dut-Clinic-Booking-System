using System;
using BCrypt.Net;

namespace Backend.Service;

public interface IAuthService
{

    Task<bool> CheckEmailExistsAsync(string email);
    Task<bool> CheckPasswordStrength(string password);
    Task<bool> CheckEmptyField(string field);
    Task<bool> CheckEmailDomain(string Email);

    Task<string> HashPassword(string Password);
    

}
