using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Backend.models;
using Microsoft.AspNetCore.Http;
using Backend.Service;
using BCrypt.Net;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
namespace Backend.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class Contacts : ControllerBase
    {
        
        private readonly AppDbContext _context;
        public readonly IAuthService _authService;

        public readonly EmailService _EmailService;
        private readonly ITokenService _tokenService;

    public Contacts(AppDbContext context, IAuthService authService, EmailService emailService, ITokenService tokenService)
        {
            _context = context;
            _authService = authService;
            _EmailService = emailService;
            _tokenService = tokenService;
        }


     [Authorize]
     [HttpPost("SendMessage")]

      public async Task<IActionResult> SendMessage([FromBody] Contact requestContact)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(requestContact.Message))
                    {
                   return Conflict(new
                   {
                       message = "message is empty"
                   });
 
                    }

                if (!requestContact.IsAnonymous)
                
                {

         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ;
         var userEmail = User.FindFirst(ClaimTypes.Email).Value;
         var fullName = User.FindFirst("FullName").Value;

            


              var FullMessageinfo = new Contact{
              UserId = userId,
              Email = userEmail,
              Name = fullName,
              Message = requestContact.Message,
              CreatedAt = DateTime.UtcNow,
              IsRead = false,
              IsAnonymous = false
              };

             _context.Add(FullMessageinfo);   
             await _context.SaveChangesAsync();



                }

        else
                {

         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
         
             var FullMessageinfo = new Contact{
              UserId = userId,
              Email = "unknown",
              Name = "Anonymous",
              Message = requestContact.Message,
              CreatedAt = DateTime.UtcNow,
              IsRead = false,
              IsAnonymous = true
              };

             _context.Add(FullMessageinfo);   
             await _context.SaveChangesAsync();


                }

              return Ok(new
             {
                 message = "Message sent"
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




    }

}


