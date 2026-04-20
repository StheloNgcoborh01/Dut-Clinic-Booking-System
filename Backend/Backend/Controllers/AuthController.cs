using Backend.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Backend.Service;
using BCrypt.Net;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

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

      [HttpPost("Register")]
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

               var hashedPassword = await _authService.HashPassword(user.Password);
               user.Password = hashedPassword;
                
               string code = await _authService.GenerateRandom();
               user.VerifyCode = code;
               user.CodeExpiry = DateTime.UtcNow.AddMinutes(10); 
               
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

            var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();
            await emailService.SendVerificationEmail(user.Email, code);

                return Ok($"{user.Email}...Registered");

            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message); 
            }

        }


        public class VerifyRequest
        {
            public string Email { get; set; } = string.Empty;
            public string VerifyCode { get; set; } = string.Empty;
        }

   [HttpPost("Verify")]
   public async Task<IActionResult> VerifyUser([FromBody] VerifyRequest request)
        {
            try{

           var UserObject = await _context.Users.FirstOrDefaultAsync<User>(u => u.Email == request.Email);
           if (UserObject == null)
                {
                    return NotFound();
                }

            var DbCode = UserObject.VerifyCode;
            var UserCode = request.VerifyCode;


            if (UserObject.IsVerified)
                {
                 return Conflict("User is already verified");
                }
              var timeNow = DateTime.UtcNow;

           if (UserObject.CodeExpiry < timeNow)
                {
                    return BadRequest(new { message = "Verification code has expired. Please request a new code." });
                }
            

            if (DbCode != UserCode)
                {
                    return Unauthorized(new { message = "Invalid verification code" });
                }



              UserObject.IsVerified = true;
              UserObject.CodeExpiry = null;
              UserObject.VerifyCode = "";

            _context.Users.Update(UserObject);
             await _context.SaveChangesAsync();
            return Ok("sucess...");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message); 
            }
        }
    }
}
