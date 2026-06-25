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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class Bookings : ControllerBase
    {
        private readonly AppDbContext _context;
        public readonly IAuthService _authService;

        public readonly EmailService _EmailService;
        private readonly ITokenService _tokenService;

        public Bookings(AppDbContext context, IAuthService authService, EmailService emailService, ITokenService tokenService)
        {
            _context = context;
            _authService = authService;
            _EmailService = emailService;
            _tokenService = tokenService;
        }

     [Authorize]
     [HttpGet("available-dates")]
     public async Task<IActionResult> GetAvailableDates()
{
    // 1. Define date range (tomorrow to 30 days from today)
    var today = DateTime.UtcNow.Date;
    var startDate = today.AddDays(1);       // Tomorrow
    var endDate = today.AddDays(30);       // 30 days from today


    // 2. Get all upcoming bookings within this date range
    var bookingsInRange = await _context.Bookings
        .Where(b => b.Status == "Upcoming"
                    && b.AppointmentDate >= startDate
                    && b.AppointmentDate <= endDate)
        .ToListAsync();

    // 3. Define all possible time slots (30-minute intervals, 9 AM to 4 PM)
    var allTimeSlots = new List<TimeSpan>
    {
        TimeSpan.FromHours(9),      // 9:00 AM
        TimeSpan.FromHours(9.5),    // 9:30 AM
        TimeSpan.FromHours(10),     // 10:00 AM
        TimeSpan.FromHours(10.5),   // 10:30 AM
        TimeSpan.FromHours(11),     // 11:00 AM
        TimeSpan.FromHours(11.5),   // 11:30 AM
        TimeSpan.FromHours(13),     // 1:00 PM (after lunch)        
        TimeSpan.FromHours(13.5),   // 1:30 PM
        TimeSpan.FromHours(14),     // 2:00 PM
        TimeSpan.FromHours(14.5),   // 2:30 PM
        TimeSpan.FromHours(15),     // 3:00 PM
        TimeSpan.FromHours(15.5)    // 3:30 PM
    };

    // 4. Build availability dictionary (day number -> list of free times)
    var availability = new Dictionary<int, List<string>>();

    for (var date = startDate; date <= endDate; date = date.AddDays(1))
    {

        // Get already booked times for this specific date
        var bookedTimes = bookingsInRange
            .Where(b => b.AppointmentDate.Date == date.Date)
            .Select(b => b.AppointmentTime)
            .ToList();

        // Find free slots (all time slots minus booked ones)
        var freeSlots = allTimeSlots
            .Where(slot => !bookedTimes.Contains(slot))
            .ToList();

        // If at least one free slot exists, add to dictionary
        if (freeSlots.Any())
        {
            var freeSlotStrings = freeSlots
                .Select(slot => slot.ToString(@"hh\:mm"))
                .ToList();

            availability.Add(date.Day, freeSlotStrings);
        }
    }

    // 5. Return the result
    return Ok(new
    {
        startDate = startDate.ToString("yyyy-MM-dd"),
        endDate = endDate.ToString("yyyy-MM-dd"),
        availability = availability
    });
}
  




}
         
    
}

