using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalonFryzjerski.Data;
using SalonFryzjerski.Models;
using SalonFryzjerski.Security;

namespace SalonFryzjerski.Controllers
{
    // Cały kontroler: minimum PODGLĄD fryzjerów
    [RequireModuleAccess(AppModule.Fryzjerzy, AccessLevel.View)]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // GET: Employees/Create -> EDYCJA
        [RequireModuleAccess(AppModule.Fryzjerzy, AccessLevel.Edit)]
        public async Task<IActionResult> Create()
        {
            await PopulateUsersDropDown();
            return View();
        }

        // POST: Employees/Create -> EDYCJA
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Fryzjerzy, AccessLevel.Edit)]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Title,IsActive,UserId")] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                await PopulateUsersDropDown(employee.UserId);
                return View(employee);
            }

            _context.Add(employee);
            await _context.SaveChangesAsync();

            // Domyślny grafik: pon–pt 10:00–18:00 (tylko jeśli nie istnieje)
            for (int dow = 1; dow <= 5; dow++)
            {
                var exists = await _context.Availabilities.AnyAsync(a =>
                    a.EmployeeId == employee.Id && a.DayOfWeek == dow);

                if (!exists)
                {
                    _context.Availabilities.Add(new Availability
                    {
                        EmployeeId = employee.Id,
                        DayOfWeek = dow,
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0),
                        IsWorking = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5 -> EDYCJA
        [RequireModuleAccess(AppModule.Fryzjerzy, AccessLevel.Edit)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            await PopulateUsersDropDown(employee.UserId);
            return View(employee);
        }

        // POST: Employees/Edit/5 -> EDYCJA
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Fryzjerzy, AccessLevel.Edit)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Title,IsActive,UserId")] Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateUsersDropDown(employee.UserId);
                return View(employee);
            }

            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5 -> EDYCJA
        [RequireModuleAccess(AppModule.Fryzjerzy, AccessLevel.Edit)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5 -> EDYCJA
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Fryzjerzy, AccessLevel.Edit)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
                _context.Employees.Remove(employee);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        private async Task PopulateUsersDropDown(string? selectedUserId = null)
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            ViewData["UserId"] = new SelectList(users, "Id", "Email", selectedUserId);
        }
    }
}
