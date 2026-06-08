using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.models;


public class Contact
{
    
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

     public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
     public string Email { get; set; } = string.Empty;

    public string Message {get; set; } =  string.Empty; 
    
    public bool IsRead {get; set; } = false;

    public bool IsAnonymous  {get; set;}

}


