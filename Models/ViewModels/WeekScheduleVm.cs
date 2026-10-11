using SalonFryzjerski.Models;

namespace SalonFryzjerski.Models.ViewModels;

public class WeekScheduleVm
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }

    public List<Employee> Employees { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<Availability> Availabilities { get; set; } = new();

}
