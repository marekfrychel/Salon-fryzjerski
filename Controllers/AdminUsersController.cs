using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonFryzjerski.Data;
using SalonFryzjerski.Models;
using SalonFryzjerski.Security;

namespace SalonFryzjerski.Controllers;

[RequireModuleAccess(AppModule.Admin, AccessLevel.View)]
public class AdminUsersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminUsersController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /AdminUsers
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users
            .OrderBy(u => u.Email)
            .Select(u => new AdminUserListItemVm
            {
                UserId = u.Id,
                Email = u.Email ?? "",
                UserName = u.UserName ?? ""
            })
            .ToListAsync();

        return View(users);
    }

    // GET: /AdminUsers/Edit/{id}
    [RequireModuleAccess(AppModule.Admin, AccessLevel.Edit)]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var existing = await _db.UserModuleAccesses
            .Where(x => x.UserId == id)
            .ToListAsync();

        var modules = Enum.GetValues<AppModule>()
            .Where(m => m != AppModule.Admin) // Admin ustawiamy też, ale pokażemy osobno niżej
            .OrderBy(m => (int)m)
            .ToList();

        // Do UI wrzućmy też Admin jako moduł
        modules.Add(AppModule.Admin);

        var vm = new AdminUserEditVm
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? "",
            Permissions = modules.Select(m => new ModuleAccessRowVm
            {
                Module = m,
                Access = existing.FirstOrDefault(e => e.Module == m)?.Access ?? AccessLevel.None
            }).ToList()
        };

        return View(vm);
    }

    // POST: /AdminUsers/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireModuleAccess(AppModule.Admin, AccessLevel.Edit)]
    public async Task<IActionResult> Edit(AdminUserEditVm vm)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == vm.UserId);
        if (user == null) return NotFound();

        // Zabezpieczenie: jak ktoś wyśle puste, nie wywalaj się
        vm.Permissions ??= new List<ModuleAccessRowVm>();

        vm.Permissions = vm.Permissions
    .GroupBy(p => p.Module)
    .Select(g => g.Last())
    .ToList();


        // Pobierz istniejące wpisy usera
        var existing = await _db.UserModuleAccesses
            .Where(x => x.UserId == vm.UserId)
            .ToListAsync();

        foreach (var row in vm.Permissions)
        {
            var rec = existing.FirstOrDefault(x => x.Module == row.Module);

            if (rec == null)
            {
                // jeśli None -> nie musimy tworzyć rekordu (opcja oszczędna)
                if (row.Access != AccessLevel.None)
                {
                    _db.UserModuleAccesses.Add(new UserModuleAccess
                    {
                        UserId = vm.UserId,
                        Module = row.Module,
                        Access = row.Access
                    });
                }
            }
            else
            {
                if (row.Access == AccessLevel.None)
                {
                    // None -> usuń rekord
                    _db.UserModuleAccesses.Remove(rec);
                }
                else
                {
                    // update
                    rec.Access = row.Access;
                }
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
