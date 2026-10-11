using SalonFryzjerski.Models;

namespace SalonFryzjerski.Models;

public class AdminUserListItemVm
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
}

public class AdminUserEditVm
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public List<ModuleAccessRowVm> Permissions { get; set; } = new();
}

public class ModuleAccessRowVm
{
    public AppModule Module { get; set; }
    public AccessLevel Access { get; set; }
}
