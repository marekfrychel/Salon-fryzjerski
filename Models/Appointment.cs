using System.ComponentModel.DataAnnotations;

namespace SalonFryzjerski.Models;

public enum AppointmentStatus
{
    Planned = 0,
    Cancelled = 1,
    Done = 2
}

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Planned;

    // Powiązanie z użytkownikiem (Identity)
    [Required]
    public string UserId { get; set; } = string.Empty;

    [StringLength(80)]
    public string? ClientName { get; set; }   // osoba do wiztyty

    [StringLength(300)]
    public string? Note { get; set; }         // kom do wizyty
}
