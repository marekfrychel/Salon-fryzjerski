using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalonFryzjerski.Data;
using SalonFryzjerski.Models;

namespace SalonFryzjerski.Services;

public interface IAccessService
{
    Task<bool> HasAccessAsync(ClaimsPrincipal user, AppModule module, AccessLevel required);
}

public class AccessService : IAccessService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public AccessService(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<bool> HasAccessAsync(ClaimsPrincipal user, AppModule module, AccessLevel required)
    {
        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var access = await _db.UserModuleAccesses
            .Where(x => x.UserId == userId && x.Module == module)
            .Select(x => (int)x.Access)
            .FirstOrDefaultAsync(); // brak rekordu => 0 => None

        return access >= (int)required;
    }
}
