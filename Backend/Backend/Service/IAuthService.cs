using System;

namespace Backend.Service;

public interface IAuthService
{

    Task<bool> CheckEmailExistsAsync(string email);
    Task<bool> CheckPasswordStrength(string password);
    Task<bool> CheckEmptyField(string field);
    Task<bool> CheckEmailDomain(string Email);
    

}
