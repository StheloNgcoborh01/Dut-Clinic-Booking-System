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
    public class AdminContact : ControllerBase
    {
        
        private readonly AppDbContext _context;
        public readonly IAuthService _authService;

        public readonly EmailService _EmailService;
        private readonly ITokenService _tokenService;

    public AdminContact(AppDbContext context, IAuthService authService, EmailService emailService, ITokenService tokenService)
        {
            _context = context;
            _authService = authService;
            _EmailService = emailService;
            _tokenService = tokenService;
        }

     public async Task<bool> IsAdmin()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    
   
    if (userIdClaim == null) 
    {   
        return false; 
    }
    
    var userId = int.Parse(userIdClaim.Value);
    var user = await _context.Users.FindAsync(userId);
    
    if (user == null)
    {
        return false;
    }
    
    return user.IsAdmin;
}   


  [Authorize]
  [HttpGet("allMessages")]

  public async Task<IActionResult> AllMessages()
        {



            try
            {

                if (! await IsAdmin())
                {
                    
                    return Unauthorized(new
                    {
                        message= "unable to acess do that request"
                    });
                }

                var AllMessages = await _context.Contacts
                                 .OrderByDescending(c => c.CreatedAt)
                                 .ToListAsync();

                if(AllMessages == null)
                {
                    return NotFound(new
                    {
                        message = "no messages found"
                    });
                }  

                return Ok(new
                {
                    message = "Messages found",
                    AllMessages = AllMessages
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
  [HttpGet("UnreadMessages")]

  public async Task<IActionResult> UnreadMessages()
        {

            try
            {

                if (! await IsAdmin())
                {
                    
                    return Unauthorized(new
                    {
                        message= "unable to acess do that request"
                    });
                }

                var AllMessages = await _context.Contacts
                                 .Where(c => c.IsRead == false)
                                 .OrderByDescending(c => c.CreatedAt)
                                 .ToListAsync();

                if(AllMessages == null)
                {
                    return NotFound(new
                    {
                        message = "no messages found"
                    });
                }  

                return Ok(new
                {
                    message = "Messages found",
                    AllMessages = AllMessages
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
  [HttpGet("ReadMessages")]

  public async Task<IActionResult> ReadMessages()
        {

            try
            {

                if (! await IsAdmin())
                {
                    
                    return Unauthorized(new
                    {
                        message= "unable to acess do that request"
                    });
                }

                var AllMessages = await _context.Contacts
                                 .Where(c => c.IsRead == true)
                                 .OrderByDescending(c => c.CreatedAt)
                                 .ToListAsync();

                if(AllMessages == null)
                {
                    return NotFound(new
                    {
                        message = "no messages found"
                    });
                }  

                return Ok(new
                {
                    message = "Messages found",
                    AllMessages = AllMessages
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
  [HttpGet("message/{id}")]

  public async Task<IActionResult> SpecificMessage(int id)
        {

            try
            {

                if (! await IsAdmin())
                {
                    
                    return Unauthorized(new
                    {
                        message= "unable to acess do that request"
                    });
                }

                var AllMessages = await _context.Contacts
                                  .FirstOrDefaultAsync(m => m.Id == id);
                             

                if(AllMessages == null)
                {
                    return NotFound(new
                    {
                        message = "no message found"
                    });
                }  

                return Ok(new
                {
                    message = "Message found",
                    AllMessages = AllMessages
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
    [HttpPatch("MarkMessage/toggleRead/{id}")]

    public async Task<IActionResult> ToggleRead(int id)
        {
            try
            {

                if(! await IsAdmin()){
                   
                    return Unauthorized(new
                    {
                        message= "unable to acess do that request"
                    });

                }

            if (id <= 0)
{
    return BadRequest(new { message = "Invalid message ID" });
}

            var Message = await _context.Contacts
                          .FirstOrDefaultAsync(m => m.Id == id);

            if (Message == null)
                {
                    return NotFound(new
                    {
                        message = "message not found"
                    });
                }

              var markMessage = Message.IsRead ? false : true;
              Message.IsRead = markMessage;
             
            await _context.SaveChangesAsync();
  
            return Ok(new
            {
                message = "message marked",
                marked = markMessage
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