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
   
   
   //search by reference
[Authorize]
[HttpGet("admincancel/{id}")]

public async Task<IActionResult >CancelAppointment(int id)
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
                           .FindAsync(id);

            if(booking == null)
                {
                    return NotFound(new
                    {
                        message = "booking not found"
                    });
                }
            if(booking.AppointmentDate < DateTime.UtcNow.Date)
                {
                    return BadRequest(new
                    {
                        message = "you cannot cancell a past booking"
                    });
                }
            if(booking.Status != "Upcoming")
                {
                    return BadRequest(new
                    {
                        message = "the booking status can no longer be changed"
                    });
                }
            
            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

              return Ok(new
              {
                  message = "booking cancelled"
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
   

    [Authorize]
     [HttpGet("AllBookingFor/{userid}")]

public async Task<IActionResult> AllBookingFor(int userid)
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

             var allBookings = await _context.Bookings
                               .Where(b => b.UserId == userid)
                               .OrderByDescending(b => b.AppointmentDate)
                               .ToListAsync();
            

            if(allBookings == null)
                {
                    return NotFound(new
                    {
                        message = "bookings not found"
                    });
                }

            return Ok(new
            {
                message = $"bookings for {userid}",
                allBookings  = allBookings
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



  public class Reschedule
        {
           public DateTime  newdate { get; set; }

           public TimeSpan newTime { get; set; } 
        }


    [Authorize]
    [HttpPatch("reschedule/{id}")]

public async Task<IActionResult> AdminReschedule([FromBody] Reschedule reschedule,int id)
        {

            try
            {

                if(!await IsAdmin())
                {  
                     return Unauthorized(new
                     {
                         message = "unable to acess"
                     });
                }
            
            var booking = await _context.Bookings
                                .FirstOrDefaultAsync(b => b.Id == id);

             
             var appointmentDateUtc = DateTime.SpecifyKind(reschedule.newdate, DateTimeKind.Utc);
             var today = DateTime.UtcNow.Date;
             var startdate = today.AddDays(1);

             var endDate = today.AddDays(30);


                if (booking == null)
                {
                    return BadRequest(new
                    {
                        message = "booking is not found"
                    });

                }

                if(booking.AppointmentDate < DateTime.UtcNow.Date)
                {
                    return BadRequest(new
                    {
                        message = "booking is in the past"
                    });
                }
                if(booking.Status != "Upcoming")
                {
                    return BadRequest (new
                    {
                        message = "cannot change the booking"
                    });
                }

                 if(appointmentDateUtc < startdate || appointmentDateUtc > endDate)
                {
                    return Conflict(new
                    {
                        message = " you can only book for the next 30 days"
                    });
                }

      var existingBooking = await _context.Bookings
    .Where(b => b.AppointmentDate.Date == appointmentDateUtc
                && b.AppointmentTime == reschedule.newTime
                && b.Status == "Upcoming")
    .FirstOrDefaultAsync();

                if (existingBooking != null)
                {
                    return Conflict(new
                    {
                        message ="seesion is already taken..choose anouther date or time"
                    });
                }
        if (appointmentDateUtc == default(DateTime))
        {
            return BadRequest(new { message = "Appointment date is required" });
        }

              

              booking.AppointmentDate = appointmentDateUtc;
              booking.AppointmentTime = reschedule.newTime;

              await _context.SaveChangesAsync();

              var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == booking.UserId);
              
               
        // 6. SEND CONFIRMATION EMAIL 
        
        try
        {
            var emailBody = $@"
                <h2>Booking Confirmed!</h2>
                <p>Dear {booking.Name},</p>
                <p>Your appointment has been rescheduled.</p>
                <p><strong>Reference:</strong> {booking.Reference}</p>
                <p><strong>Date:</strong>📅  {booking.AppointmentDate:yyyy-MM-dd}</p>
                <p><strong>Time:</strong>⏰ {booking.AppointmentTime.Hours:D2}:{booking.AppointmentTime.Minutes:D2}</p>              
                <p><strong>Type:</strong> {booking.AppointmentType}</p>
                <p>Please bring your ID to the appointment.</p>
                <p> Also please Note that you cannot reschedule you booking again </p>
            ";

          await _EmailService.SendBookingConfirmation(user.Email, booking.Name , booking.Reference, booking.AppointmentDate, booking.AppointmentTime, booking.AppointmentType);
        
    
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"Email failed to send: {ex.Message}");
        }

                return Ok(new
        {
             message = "Booking reschedulled successfully",
             reference = booking.Reference,
            appointmentDate = booking.AppointmentDate.ToString("yyyy-MM-dd"),
            appointmentTime = $"{booking.AppointmentTime.Hours:D2}:{booking.AppointmentTime.Minutes:D2}",
            appointmentType = booking.AppointmentType
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



//see all admins

    [Authorize]
    [HttpGet("allAdmins")]
     public async Task<IActionResult> AllAdmins()
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
                
                var AllAdmins = await _context.Users
                             .Select(u => new
                              {
                              u.Id,
                              u.Email,
                              u.Fname,
                              u.Lname,
                              u.IsVerified,
                              u.IsAdmin
                             }).
                             Where(u => u.IsAdmin == true)
                             .ToListAsync();            
                             
                return Ok( new
                {
                    message = "All Admins",
                    admins = AllAdmins.Count,
                    AllAdmins = AllAdmins
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


public class Toggleadmin
        {
            public string Password {get; set;} = string.Empty;

        }

[Authorize]
[HttpPatch("toggleadmin/{id}")]

public async Task<IActionResult> ToggleAdmin([FromBody] Toggleadmin adminInfo  , int id)
        {
            try
            {
                    if(!await IsAdmin())
                {  
                     return Unauthorized(new
                     {
                         message = "unable to acess"
                     });
                }

         var  GetAdmin = await _context.Users.FindAsync(id);
    
         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ;

        var CurrentAdmin = await _context.Users.FindAsync(userId);

        if(!BCrypt.Net.BCrypt.Verify(adminInfo.Password, CurrentAdmin.Password))
                {
                    return Unauthorized(new
                    {
                        message = "Only Admins Can Configure this request"
                    });
                }
                                

          var isAdmin = GetAdmin.IsAdmin ? false : true;

          GetAdmin.IsAdmin = isAdmin;
          await _context.SaveChangesAsync();

          return Ok(new
          {
              message = isAdmin ? $"{GetAdmin.Fname} is now an Admin" : $"{GetAdmin.Fname} is removed as an Admin"
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

