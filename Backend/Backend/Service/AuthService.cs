using Microsoft.EntityFrameworkCore;
using Backend.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Backend.Service;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;


    public AuthService(AppDbContext context)
    {
        _context = context;

    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(user => user.Email == email);

    }


    public async Task<bool> CheckPasswordStrength(string password)
    {
    return Regex.IsMatch(password, @"[a-z]") && Regex.IsMatch(password, @"[A-Z]") && password.Length >= 8;

    }

public async Task<bool> CheckEmptyField(string field)
    {
        return string.IsNullOrEmpty(field);
    }

    public async Task<bool> CheckEmailDomain(string email)
    {
        return email.EndsWith("@gmail.com");
    }


   public async Task<string> HashPassword(string Password)
    {
        
        return BCrypt.Net.BCrypt.HashPassword(Password);

    }


}




