
using System;
using Backend.models;

namespace Backend.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
    
}