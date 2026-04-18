using Backend.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Backend.Service;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        public readonly IAuthService _authService;

        public AuthController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

      [HttpPost]

    public async Task<IActionResult> AddUser(User user)
    {
        try
        {
              if ( await _authService.CheckEmptyField(user.Fname) || await _authService.CheckEmptyField(user.Lname) || await _authService.CheckEmptyField(user.Email) || await _authService.CheckEmptyField(user.Password) )
            {
                return Conflict("Check Empty Fields");
            }
        
                if ( await _authService.CheckEmailExistsAsync(user.Email))
                {
                    return Conflict($"email {user.Email} Already exist");
                }
                
                
                if (!await _authService.CheckPasswordStrength(user.Password))
                {
                    return Conflict("password is too weak . alteast 8 letters with Capical letter");
                }


                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(user);
                
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message); // internal server error
            }
            

        }

        
    }
}
