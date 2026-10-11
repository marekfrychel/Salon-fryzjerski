using System.ComponentModel.DataAnnotations;

namespace SalonFryzjerski.Models;

public class Availability
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    // 1 = Monday ... 7 = Sunday (albo 0..6, byle konsekwentnie)
    [Range(1, 7)]
    public int DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public bool IsWorking { get; set; } = true;
}
