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


       [HttpGet("available-dates")]

       public async Task<IActionResult> GetAvailableDates(int year, int month)
{
       // 1. Get the current logged-in user's ID from the JWT token
     // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    
       // 2. Define the start and end of the requested month
var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).Date;
var endDate = startDate.AddMonths(1).AddDays(-1).Date;
    
    // 3. Get all UPCOMING bookings for this month from database
    var bookingsInMonth = await _context.Bookings
        .Where(b => b.Status == "Upcoming"
                    && b.AppointmentDate >= startDate
                    && b.AppointmentDate <= endDate)
        .ToListAsync();
    
    // 4. Define ALL possible time slots in a day (30-minute intervals from 9 AM to 4 PM)
    var allTimeSlots = new List<TimeSpan>
    {
        TimeSpan.FromHours(9),      // 9:00  AM
        TimeSpan.FromHours(9.5),    // 9:30  AM
        TimeSpan.FromHours(10),     // 10:00 AM
        TimeSpan.FromHours(10.5),   // 10:30 AM
        TimeSpan.FromHours(11),     // 11:00 AM
        TimeSpan.FromHours(11.5),   // 11:30 AM
        TimeSpan.FromHours(13),     // 1:00 PM  (after lunch)       TimeSpan.FromHours(13.5),   // 1:30 PM
        TimeSpan.FromHours(14),     // 2:00 PM
        TimeSpan.FromHours(14.5),   // 2:30 PM
        TimeSpan.FromHours(15),     // 3:00 PM
        TimeSpan.FromHours(15.5)    // 3:30 PM
    };

   // 5. Get all dates in this month that have aleastt  ONE free slot
   var availability = new Dictionary<int, List<string>>();

for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
{
    var currentDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    
    // Skip past dates
    if (currentDate < DateTime.Today)
        continue;
    
    // Get all booked time slots for this specific date
    var bookedTimes = bookingsInMonth
        .Where(b => b.AppointmentDate.Date == currentDate.Date)
        .Select(b => b.AppointmentTime)
        .ToList();

     // Find which time slots are free (not booked)
     var freeSlots = allTimeSlots
        .Where(slot => !bookedTimes.Contains(slot))
        .ToList();
    
    // If there's at least one free slot, add to dictionary with times
    if (freeSlots.Any())
    {
        
        // Convert TimeSpan to readable string (e.g., "09:00")
        var freeSlotStrings = freeSlots
            .Select(slot => slot.ToString(@"hh\:mm"))
            .ToList();
        
        availability.Add(day, freeSlotStrings);

    }

}

return Ok(new
{
    year = year,
    month = month,
    availability = availability
});
    

}

    }
}
