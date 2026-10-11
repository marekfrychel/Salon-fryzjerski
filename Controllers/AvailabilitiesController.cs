using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonFryzjerski.Data;
using SalonFryzjerski.Models;
using SalonFryzjerski.Models.ViewModels;

namespace SalonFryzjerski.Controllers
{
    public class AvailabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Availabilities/EditWeek?employeeId=1
        public async Task<IActionResult> EditWeek(int employeeId)
        {
            var emp = await _context.Employees.FindAsync(employeeId);
            if (emp == null) return NotFound();

            var avails = await _context.Availabilities
                .Where(a => a.EmployeeId == employeeId)
                .ToListAsync();

            var vm = new EditAvailabilityWeekVm
            {
                EmployeeId = employeeId,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                Days = Enumerable.Range(1, 7).Select(dow =>
                {
                    var a = avails.FirstOrDefault(x => x.DayOfWeek == dow);

                    return new EditAvailabilityWeekVm.DayRow
                    {
                        DayOfWeek = dow,
                        DayName = dow switch
                        {
                            1 => "Poniedziałek",
                            2 => "Wtorek",
                            3 => "Środa",
                            4 => "Czwartek",
                            5 => "Piątek",
                            6 => "Sobota",
                            7 => "Niedziela",
                            _ => ""
                        },
                        IsWorking = a?.IsWorking ?? (dow <= 5),
                        StartTime = a?.StartTime ?? new TimeSpan(10, 0, 0),
                        EndTime = a?.EndTime ?? new TimeSpan(18, 0, 0)
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWeek(EditAvailabilityWeekVm vm)
        {
            var emp = await _context.Employees.FindAsync(vm.EmployeeId);
            if (emp == null) return NotFound();

            foreach (var day in vm.Days)
            {
                if (day.IsWorking && day.EndTime <= day.StartTime)
                    ModelState.AddModelError("", $"{day.DayName}: koniec musi być później niż start.");
            }

            if (!ModelState.IsValid)
            {
                vm.EmployeeName = $"{emp.FirstName} {emp.LastName}";
                return View(vm);
            }

            var existing = await _context.Availabilities
                .Where(a => a.EmployeeId == vm.EmployeeId)
                .ToListAsync();

            foreach (var day in vm.Days)
            {
                var row = existing.FirstOrDefault(a => a.DayOfWeek == day.DayOfWeek);

                if (row == null)
                {
                    row = new Availability
                    {
                        EmployeeId = vm.EmployeeId,
                        DayOfWeek = day.DayOfWeek
                    };
                    _context.Availabilities.Add(row);
                }

                row.IsWorking = day.IsWorking;
                row.StartTime = day.StartTime;
                row.EndTime = day.EndTime;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Employees");
        }
    }
}
