using System.ComponentModel.DataAnnotations;

namespace SalonFryzjerski.Models;

using Microsoft.AspNetCore.Identity;



public class Employee
{
    public string? UserId { get; set; }            // Id z AspNetUsers
    public IdentityUser? User { get; set; }        // nawigacja (opcjonalnie)

    public int Id { get; set; }

    [Required, StringLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Title { get; set; } // np. "Senior xd, albo że jest specjalistą od rudych"

    public bool IsActive { get; set; } = true;

    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public string FullName => $"{FirstName} {LastName}";
}
