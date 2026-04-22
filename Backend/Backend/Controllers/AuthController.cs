using Backend.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Backend.Service;
using BCrypt.Net;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        public readonly IAuthService _authService;

        public readonly EmailService _EmailService;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, IAuthService authService, EmailService emailService, ITokenService tokenService)
        {
            _context = context;
            _authService = authService;
            _EmailService = emailService;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> AddUser(User user)
        {
            try
            {
                if (await _authService.CheckEmptyField(user.Fname) || await _authService.CheckEmptyField(user.Lname) || await _authService.CheckEmptyField(user.Email) || await _authService.CheckEmptyField(user.Password))
                {
                    return Conflict("Check Empty Fields");
                }

                if (await _authService.CheckEmailExistsAsync(user.Email))
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


                await _EmailService.SendVerificationEmail(user.Email, code);

                return Ok($"{user.Email}...Registered");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message
                });

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
            try
            {

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

        public class ForgotRequest
        {
            public string Email { get; set; } = string.Empty;

        }

        [HttpPost("Forgot")]
        public async Task<IActionResult> ForgotUser([FromBody] ForgotRequest request)
        {
            try
            {

                var UserObject = await _context.Users.FirstOrDefaultAsync<User>(u => u.Email == request.Email);

                if (UserObject != null)
                {
                    string code = await _authService.GenerateRandom();
                    UserObject.VerifyCode = code;
                    UserObject.CodeExpiry = DateTime.UtcNow.AddMinutes(10);

                    await _context.SaveChangesAsync();
                    await _EmailService.SendVerificationEmail(UserObject.Email, code);

                    return Ok("Code has Been sent to your Email");

                }
                return Ok("If the email exists, a reset code has been sent.");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }




        [HttpPost("verifyForgot")]
        public async Task<IActionResult> VerifyForgot([FromBody] VerifyRequest verifyRequest)
        {
            try
            {
                var UserObject = await _context.Users.FirstOrDefaultAsync<User>(u => u.Email == verifyRequest.Email);

                if (UserObject != null)
                {
                    var timeNow = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(UserObject.VerifyCode))
                    {
                        return BadRequest(new { message = "No verification code found. Request a new one." });
                    }

                    var userCode = verifyRequest.VerifyCode;
                    var DBcode = UserObject.VerifyCode;

                    if (DBcode != userCode)
                    {
                        return Unauthorized(new { message = "Invalid verification code" });
                    }

                    if (UserObject.CodeExpiry < timeNow)
                    {
                        return BadRequest(new { message = "Verification code has expired. Please request a new code." });
                    }

                    UserObject.CodeExpiry = null;
                    UserObject.VerifyCode = null;
                    UserObject.ResetToken = null;

                    if (!UserObject.IsVerified)
                    {
                        UserObject.IsVerified = true;
                    }

                    string resetToken = Guid.NewGuid().ToString();

                    UserObject.ResetToken = resetToken;
                    UserObject.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Code verified successfully. You can now reset your password.",
                        resetToken = resetToken,
                        expiresInMinutes = 10
                    });
                }

                return NotFound(new { message = "Email not found. Please check and try again." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

            }

        }


        public class ResetPassword
        {
            public string ResetToken { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

        }

        [HttpPost("passwordReset")]
        public async Task<IActionResult> PasswordReset([FromBody] ResetPassword resetPassword)
        {
            try
            {
                var UserObject = await _context.Users.FirstOrDefaultAsync<User>(u => u.ResetToken == resetPassword.ResetToken);
                if (UserObject != null)

                {
                    var timeNow = DateTime.UtcNow;

                    if (string.IsNullOrEmpty(UserObject.ResetToken))
                    {
                        return BadRequest(new { message = "No reset token found. Please request a password reset first." });
                    }

                    if (UserObject.ResetTokenExpiry == null || UserObject.ResetTokenExpiry < timeNow)
                    {
                        return BadRequest(new { message = "Reset Token code has expired. Please try again." });
                    }

                    var userPassword = resetPassword.Password;

                    if (!await _authService.CheckPasswordStrength(userPassword))
                    {
                        return Conflict("password is too weak . alteast 8 letters with Capical letter");
                    }

                    var hashedPassword = await _authService.HashPassword(userPassword);
                    UserObject.Password = hashedPassword;
                    UserObject.ResetToken = null;
                    UserObject.ResetTokenExpiry = null;
                     await _context.SaveChangesAsync();

                    return Ok("Your have successfull reseted your passsword");

                }

                return NotFound("User Not Found");

            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message
                });
            }

        }

        public class Login
        {
            public string? Email { get; set; }

            public string? Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] Login login)
        {

            try
            {
                var UserObject = await _context.Users.FirstOrDefaultAsync<User>(u => u.Email == login.Email);

                if (UserObject == null || UserObject.IsVerified != true || !BCrypt.Net.BCrypt.Verify(login.Password, UserObject.Password))
                {
                     return Unauthorized(new { message = "Invalid email or password, or account not verified" });
                }
                   var token = _tokenService.CreateToken(UserObject);
                    return Ok(new
                    {
                        message = "You have logged in",
                         token = token,
                         expiresInMinutes = 60, 
                        
                    });
                }
            
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message,
                   
                });
            }


        }


[Authorize]
[HttpGet("me")]
public async Task<IActionResult> GetCurrentUser()
{
    // Get user ID from the token
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    
    var user = await _context.Users.FindAsync(userId);
    
    if (user == null)
        return NotFound();

    return Ok(new
    {
        id = user.Id,
        email = user.Email,
        name = $"{user.Fname} {user.Lname}",
        isVerified = user.IsVerified
    });
}



    }



}
