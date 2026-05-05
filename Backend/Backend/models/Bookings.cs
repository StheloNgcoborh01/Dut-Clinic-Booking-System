using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.models;

public class Bookings
{
    // Primary Key
    public int Id { get; set; }

    // Link to the user who made the booking
    [Required]
    public int UserId { get; set; }

    // Booking Reference (e.g., BK-0042)
    public string Reference { get; set; } = string.Empty;

    // Patient Info (copied from User at booking time)
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Surname { get; set; } = string.Empty;

    // South African ID Number (stored as string because it's 13 digits and never used for math)
    [Required]
    [MaxLength(13)]
    [MinLength(13)]
    public string IdNumber { get; set; } = string.Empty;

    // Appointment Details
    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan AppointmentTime { get; set; }

    [Required]
    public string AppointmentType { get; set; } = string.Empty;

    // Status: Upcoming, Completed, Cancelled
    public string Status { get; set; } = "Upcoming";

    // System fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}

