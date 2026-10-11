using System.ComponentModel.DataAnnotations;

namespace SalonFryzjerski.Models;

public class UserModuleAccess
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = ""; // AspNetUsers.Id

    public AppModule Module { get; set; }

    public AccessLevel Access { get; set; }
}
