using System.ComponentModel.DataAnnotations;

namespace SalonFryzjerski.Models;

public class Service
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Description { get; set; }

    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}