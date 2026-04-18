using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.models;

public class User
{
    public int Id { get; set; }
   [Required]
    public string Fname  { get; set; } = string.Empty;
    [Required]
    public string Lname { get; set; } = string.Empty;

     [Required]
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsAdmin { get; set; } = false;

    public bool IsVerified { get; set; } = false;

}
