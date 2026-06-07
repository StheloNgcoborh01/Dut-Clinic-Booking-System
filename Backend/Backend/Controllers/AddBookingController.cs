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
namespace Backend.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class AddBooking : ControllerBase
    {
        
        private readonly AppDbContext _context;
        public readonly IAuthService _authService;

        public readonly EmailService _EmailService;
        private readonly ITokenService _tokenService;

    public AddBooking(AppDbContext context, IAuthService authService, EmailService emailService, ITokenService tokenService)
        {
            _context = context;
            _authService = authService;
            _EmailService = emailService;
            _tokenService = tokenService;
        }

    [Authorize]
    [HttpPost("AddBooking")]
public async Task<IActionResult> AddingBooking([FromBody] Booking request)
{
    try
    {
        // 1. GET USER INFO FROM JWT TOKEN
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var emailClaim = User.FindFirst(ClaimTypes.Email);
        var fullNameClaim = User.FindFirst("FullName");

        // Convert to UTC immediately for the db
       var appointmentDateUtc = DateTime.SpecifyKind(request.AppointmentDate, DateTimeKind.Utc);
       
        if (userIdClaim == null || emailClaim == null || fullNameClaim == null)
        {
            return Unauthorized(new { message = "Invalid token. Please login again." });
        }

        var userId = int.Parse(userIdClaim.Value);
        var userEmail = emailClaim.Value;
        var fullName = fullNameClaim.Value;

        // Split full name into first and last name
        var nameParts = fullName.Split(' ');
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

       var today = DateTime.UtcNow.Date;
       var startDate = today.AddDays(1);
       var endDate = today.AddDays(30);

    // var isExist  =  await _context.Bookings.AnyAsync(user => user.IdNumber == request.IdNumber);

        // 2. VALIDATE REQUIRED FIELDS
        if (string.IsNullOrWhiteSpace(request.IdNumber))
        {
            return BadRequest(new { message = "ID Number is required" });
        }

        if (request.IdNumber.Length != 13)
        {
            return BadRequest(new { message = "ID Number must be 13 digits" });
        }

         if (await _authService.CheckExistingIdAsync( request.IdNumber))
         {
            return BadRequest(new { message = "Id Number is Linked to Anouther Account" });
         }

        if (appointmentDateUtc == default(DateTime))
        {
            return BadRequest(new { message = "Appointment date is required" });
        }

        if (appointmentDateUtc < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "Appointment date cannot be in the past" });
        }

        if(appointmentDateUtc < startDate || appointmentDateUtc > endDate)
         {
         return BadRequest(new
             {
              message = "Bookings are for the next 30 days only"
            });
        }

        if (string.IsNullOrWhiteSpace(request.AppointmentType))
        {
            return BadRequest(new { message = "Appointment type is required" });
        }

// Check if this slot is already booked
var existingBooking = await _context.Bookings
    .Where(b => b.AppointmentDate.Date == appointmentDateUtc.Date  
                && b.AppointmentTime == request.AppointmentTime
                && b.Status == "Upcoming")
    .FirstOrDefaultAsync();

    if (existingBooking != null)
{
    return Conflict(new { 
        message = "This time slot is no longer available. Please select another time.",
        date = appointmentDateUtc,
        time = request.AppointmentTime
    });
}

        // 3. CREATE BOOKING OBJECT
        var booking = new Booking
        {
            UserId = userId,
            Name = firstName,
            Surname = lastName,
            IdNumber = request.IdNumber,
            AppointmentDate = appointmentDateUtc.Date,
            AppointmentTime = request.AppointmentTime,
            AppointmentType = request.AppointmentType,
            Status = "Upcoming",
            CreatedAt = DateTime.UtcNow,
            Reference = "" // 
        };

        // 4. SAVE TO DATABASE (FIRST TIME - GETS ID)
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // 5. GENERATE REFERENCE USING THE NEW ID
        booking.Reference = $"BK-{booking.Id:D4}";
        await _context.SaveChangesAsync();

        // 6. SEND CONFIRMATION EMAIL (DON'T FAIL IF EMAIL FAILS)
        try
        {
            var emailBody = $@"
                <h2>Booking Confirmed!</h2>
                <p>Dear {firstName} {lastName},</p>
                <p>Your appointment has been confirmed.</p>
                <p><strong>Reference:</strong> {booking.Reference}</p>
                <p><strong>Date:</strong>📅  {booking.AppointmentDate:yyyy-MM-dd}</p>
                <p><strong>Time:</strong>⏰ {booking.AppointmentTime.Hours:D2}:{booking.AppointmentTime.Minutes:D2}</p>              
                <p><strong>Type:</strong> {booking.AppointmentType}</p>
                <p>Please bring your ID to the appointment.</p>
            ";

          await _EmailService.SendBookingConfirmation(userEmail, firstName, booking.Reference, booking.AppointmentDate, booking.AppointmentTime, booking.AppointmentType);
        
        }
        catch (Exception ex)
        {
            // Log error but don't fail the booking
            Console.WriteLine($"Email failed to send: {ex.Message}");
        }

        // 7. RETURN SUCCESS RESPONSE
        return Ok(new
        {
            message = "Booking confirmed successfully",
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
            message = "An error occurred while creating your booking",
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }
}

[Authorize]
[HttpGet("MyBookings")]

public async Task<IActionResult> GetAllBookings()
        {
            try
            {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ;

             var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.AppointmentDate) // newest first
            .ToListAsync();

             if(bookings == null)
                {
                return NotFound(new
                {
                    message = "bookings not found"
                });
                }

                return Ok( new
                {
                    bookings = bookings
                }

                );
            }

    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            message = "An error occurred while getting your booking",
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

        }

    [Authorize]
    [HttpGet("MyPastBookings")]

public async Task<IActionResult> GetAPastBookings()
        {
            try
            {
                 var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ;

          
           var today = DateTime.UtcNow.Date;
             var bookings = await _context.Bookings
            .Where(b => b.UserId == userId && b.AppointmentDate < today && b.Status == "Past")
            .OrderByDescending(b => b.AppointmentDate)  // newest first
            .ToListAsync();


                return Ok( new
                {
                    bookings = bookings
                }

                );
            }

    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            message = "An error occurred while getting your booking",
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

        }

    [Authorize]
    [HttpGet("FutureBookings")]

public async Task<IActionResult> GetFutureBookings()
        {
            try
            {
                 var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ;

          
           var today = DateTime.UtcNow.Date;
             var bookings = await _context.Bookings
            .Where(b => b.UserId == userId && b.AppointmentDate >= today && b.Status == "Upcoming")
            .OrderByDescending(b => b.AppointmentDate)  // newest first
            .ToListAsync();


                return Ok( new
                {
                    bookings = bookings
                }

                );
            }

    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            message = "An error occurred while getting your booking",
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

        }

    [Authorize]
    [HttpDelete("cancel/{id}")]
      public async Task<IActionResult> DeleteBooking(int id)
        {

            try
         {
          var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ;

          var booking = await _context.Bookings.
          FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
         
          //check null

          if(booking == null)
                {
                    return NotFound( new
                    {
                        message = "Booking not Found"
                    });
                }
          //check booking date
          if (booking.AppointmentDate < DateTime.UtcNow.Date)
                {
                    return BadRequest( new
                    {
                        message = "This Booking is Already Past"
                    });
                }
          //status
          if (booking.Status != "Upcoming")
                {
                    return BadRequest(new
                    {
                        message = "the you can cancell only upcoming bookings"
                    });
                }

         //then make the staus == cancelled
         booking.Status = "Cancelled";
         await _context.SaveChangesAsync();

         return Ok(new
         {
             message = "booking Cancelled successfully"
         });

                
            }
        catch (Exception ex)
        {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            message = "An error occurred while deleting your booking",
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
        }
            
        }


[Authorize]
[HttpGet("getspeficbooking/{id}")]

public async Task<IActionResult> GetSpeficibooking(int id)
        {
            try
            {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ;

              var booking = await _context.Bookings.
              FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
                 
                 if(booking == null)
                {
                return NotFound(new
                {
                    message = "booking not found"
                });
                }

                return Ok( new
                {
                    booking = booking
                }

                );
            }

    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            message = "An error occurred while getting your booking",
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }

        }


    }
}
