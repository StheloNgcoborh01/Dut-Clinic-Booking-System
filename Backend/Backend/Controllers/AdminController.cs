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
    public class Admin : ControllerBase
    {
        
        private readonly AppDbContext _context;
        public readonly IAuthService _authService;

        public readonly EmailService _EmailService;
        private readonly ITokenService _tokenService;

    public Admin(AppDbContext context, IAuthService authService, EmailService emailService, ITokenService tokenService)
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
     [HttpGet("TodaysBookings")]

     public async Task<IActionResult> TodaysBookings()
        {

     try
    {
                if (!await IsAdmin())
                {
                    
                     return Unauthorized(new
                     {
                         message = "unalble to acess"
                     });
                }
            
       var today = DateTime.UtcNow.Date;
       var thatday = today.AddDays(3);
       var bookings = await _context.Bookings
                      .Where(b => b.AppointmentDate == thatday && b.Status == "Upcoming")
                      .OrderBy( b => b.AppointmentTime)
                      .ToListAsync();

             return Ok(new
            {
                date = thatday.ToString("yyyy-MM-dd"),
                count = bookings.Count,
                bookings = bookings
            });

    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

        }


  //the All Bookings endpoint

    [Authorize]
    [HttpGet("allBookings")]
     public async Task<IActionResult> AllBookings()
        {
            
            try
            {

               if (!await IsAdmin())
                {
                    
                     return Unauthorized(new
                     {
                         message = "unalble to acess"
                     });
                }

                var bookings = await _context.Bookings
                                .OrderByDescending(b => b.AppointmentDate)
                                .ToListAsync();
                return Ok( new
                {
                    message = "All Bookings",
                    bookings = bookings
                });
            }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }
            
        }


//the Complete Booking
[Authorize]
[HttpPatch("markComplete/{id}")]
 public async Task<IActionResult> MarkComplete(int id)
        {
            try
            {
              if (!await IsAdmin())
                {
                    
                     return Unauthorized(new
                     {
                         message = "unable to acess"
                     });
                }

          var booking = await _context.Bookings
                         .FirstOrDefaultAsync(b => b.Id == id);
          var today = DateTime.UtcNow.Date;

               if(booking == null)
                {
                    return BadRequest(new
                    {
                        message = "booking not found"
                    });
                }
               if(booking.AppointmentDate != today)
                {
                    return BadRequest(new
                    {
                        message = "You can only mark complete the dates for today"
                    });
                }
                if(booking.Status != "Upcoming")
                {
                   return BadRequest( new
                    {
                   message = "unable to mark the booking"

                    });
                    
                }

         booking.Status = "Completed";
         await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Appointment completed"
                });
                
            }
                catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

        }


//no attended
[Authorize]
[HttpPatch("MarkNoAttended/{id}")]
 public async Task<IActionResult> MarkNoAttended(int id)
        {
            try
            {
              if (!await IsAdmin())
                {
                    
                     return Unauthorized(new
                     {
                         message = "unable to acess"
                     });
                }

          var booking = await _context.Bookings
                         .FirstOrDefaultAsync(b => b.Id == id);
          var today = DateTime.UtcNow.Date;

               if(booking == null)
                {
                    return BadRequest(new
                    {
                        message = "booking not found"
                    });
                }
               if(booking.AppointmentDate != today)
                {
                    return BadRequest(new
                    {
                        message = "You can only mark complete the dates for today"
                    });
                }
                if(booking.Status != "Upcoming")
                {
                   return BadRequest( new
                    {
                   message = "unable to mark the booking"

                    });
                    
                }

         booking.Status = "Not Attended";
         await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Appointment Not Attended"
                });
                
            }
                catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

        }


//search by reference
[Authorize]
[HttpGet("getbyreference/{reference}")]

public async Task<IActionResult> GetByReference(string reference)
        {

            try
            {

             if (!await IsAdmin())
                {  
                     return Unauthorized(new
                     {
                         message = "unable to acess"
                     });
                }

                var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Reference == reference);
                            

                if (booking == null)
                {
                    return NotFound(new
                    {
                        message = "No Booking"
                    });
                }

                return Ok(new
                {
                    message = "booking found",
                    booking = booking
                });
                
            }

                catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

            
}
    }

}