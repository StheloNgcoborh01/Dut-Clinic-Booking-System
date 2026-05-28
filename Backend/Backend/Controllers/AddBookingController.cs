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
        var appointmentDateUtc = request.AppointmentDate.ToUniversalTime();

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

        // 2. VALIDATE REQUIRED FIELDS
        if (string.IsNullOrWhiteSpace(request.IdNumber))
        {
            return BadRequest(new { message = "ID Number is required" });
        }

        if (request.IdNumber.Length != 13)
        {
            return BadRequest(new { message = "ID Number must be 13 digits" });
        }

        if (appointmentDateUtc == default(DateTime))
        {
            return BadRequest(new { message = "Appointment date is required" });
        }

        if (appointmentDateUtc < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "Appointment date cannot be in the past" });
        }

        if (string.IsNullOrWhiteSpace(request.AppointmentType))
        {
            return BadRequest(new { message = "Appointment type is required" });
        }

// Check if this slot is already booked
var existingBooking = await _context.Bookings
    .Where(b => b.AppointmentDate.Date == appointmentDateUtc.Date  // ← Added .Date here
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

Console.WriteLine($"DEBUG: Email params - To: {userEmail}, Name: {firstName}, Ref: {booking.Reference}, Date: {booking.AppointmentDate}, Time: {booking.AppointmentTime}, Type: {booking.AppointmentType}");
        // 6. SEND CONFIRMATION EMAIL (DON'T FAIL IF EMAIL FAILS)
        try
        {
            var emailBody = $@"
                <h2>Booking Confirmed!</h2>
                <p>Dear {firstName} {lastName},</p>
                <p>Your appointment has been confirmed.</p>
                <p><strong>Reference:</strong> {booking.Reference}</p>
                <p><strong>Date:</strong> {booking.AppointmentDate:yyyy-MM-dd}</p>
                <p><strong>Time:</strong> {booking.AppointmentTime.Hours:D2}:{booking.AppointmentTime.Minutes:D2}</p>              
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

    }

}
