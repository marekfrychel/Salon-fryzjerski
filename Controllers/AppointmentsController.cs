using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalonFryzjerski.Data;
using SalonFryzjerski.Models;
using SalonFryzjerski.Security;

namespace SalonFryzjerski.Controllers
{
    // Każda akcja w tym kontrolerze wymaga co najmniej PODGLĄDU grafiku
    [RequireModuleAccess(AppModule.Grafik, AccessLevel.View)]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Appointments.Include(a => a.Employee).Include(a => a.Service);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Appointments/Week
        public async Task<IActionResult> Week(DateTime? start)
        {
            // Ustal początek tygodnia (poniedziałek)
            var baseDate = (start ?? DateTime.Today).Date;
            int diff = (7 + (baseDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = baseDate.AddDays(-diff);
            var weekEnd = weekStart.AddDays(7);

            // Fryzjerzy
            var employees = await _context.Employees
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            // Wizyty w tym tygodniu
            var appts = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .Where(a => a.StartAt >= weekStart && a.StartAt < weekEnd)
                .OrderBy(a => a.StartAt)
                .ToListAsync();

            var avails = await _context.Availabilities
                .Where(a => a.IsWorking)
                .ToListAsync();

            var vm = new SalonFryzjerski.Models.ViewModels.WeekScheduleVm
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Employees = employees,
                Appointments = appts,
                Availabilities = avails
            };

            return View(vm);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        /// GET: Appointments/Create
        // EDYCJA wymagana do tworzenia
        [RequireModuleAccess(AppModule.Grafik, AccessLevel.Edit)]
        public IActionResult Create(int? employeeId, DateTime? startAt)
        {
            // BLOKADA OTWIERANIA FORMULARZA DLA PRZESZŁYCH TERMINÓW
            if (startAt.HasValue && startAt.Value < DateTime.Now.AddMinutes(5))
            {
                return RedirectToAction(nameof(Week));
            }

            PopulateDropDowns(selectedServiceId: null, selectedEmployeeId: employeeId);

            if (startAt.HasValue)
            {
                var model = new Appointment
                {
                    EmployeeId = employeeId ?? 0,
                    StartAt = startAt.Value
                };
                return View(model);
            }

            return View();
        }

        // POST: Appointments/Create
        // EDYCJA wymagana do tworzenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Grafik, AccessLevel.Edit)]
        public async Task<IActionResult> Create([Bind("ServiceId,EmployeeId,StartAt,ClientName,Note")] Appointment appointment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            // 1) Pobierz usługę (czas trwania)
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);
            if (service == null)
                ModelState.AddModelError("ServiceId", "Wybrana usługa nie istnieje.");

            // 2) Minimum 5 minut od teraz
            if (appointment.StartAt < DateTime.Now.AddMinutes(5))
                ModelState.AddModelError("StartAt", "Wybierz termin co najmniej 5 minut od teraz.");

            // 3) Jeżeli mamy usługę -> wylicz EndAt
            if (service != null)
                appointment.EndAt = appointment.StartAt.AddMinutes(service.DurationMinutes);

            // 4) Godziny pracy WYBRANEGO FRYZJERA (Availability)
            if (service != null)
            {
                int dow = appointment.StartAt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)appointment.StartAt.DayOfWeek;

                var avail = await _context.Availabilities.FirstOrDefaultAsync(a =>
                    a.EmployeeId == appointment.EmployeeId &&
                    a.IsWorking &&
                    a.DayOfWeek == dow
                );

                if (avail == null)
                {
                    ModelState.AddModelError("", "Wybrany fryzjer nie pracuje w tym dniu.");
                }
                else
                {
                    if (appointment.StartAt.TimeOfDay < avail.StartTime || appointment.EndAt.TimeOfDay > avail.EndTime)
                    {
                        ModelState.AddModelError("StartAt",
                            $"Wybrany fryzjer pracuje w tym dniu w godzinach {avail.StartTime:hh\\:mm}–{avail.EndTime:hh\\:mm}.");
                    }
                }
            }

            // 5) Konflikt terminów (nakładanie wizyt fryzjera)
            if (service != null)
            {
                var hasConflict = await _context.Appointments.AnyAsync(a =>
                    a.EmployeeId == appointment.EmployeeId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.StartAt < appointment.EndAt &&
                    a.EndAt > appointment.StartAt
                );

                if (hasConflict)
                    ModelState.AddModelError("", "Ten fryzjer ma już wizytę w wybranym czasie. Wybierz inny termin.");
            }

            // 6) Pola, których nie ma w formularzu
            appointment.UserId = userId;
            appointment.Status = AppointmentStatus.Planned;

            // 7) Usuń błędy ModelState dla pól niewidocznych w formularzu
            ModelState.Remove("UserId");
            ModelState.Remove("Status");
            ModelState.Remove("EndAt");

            if (!ModelState.IsValid)
            {
                PopulateDropDowns(appointment.ServiceId, appointment.EmployeeId);
                return View(appointment);
            }

            _context.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Edit/5
        // EDYCJA wymagana do edycji
        [RequireModuleAccess(AppModule.Grafik, AccessLevel.Edit)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            PopulateDropDowns(appointment.ServiceId, appointment.EmployeeId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // EDYCJA wymagana do edycji
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Grafik, AccessLevel.Edit)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceId,EmployeeId,StartAt,ClientName,Note,Status")] Appointment input)
        {
            if (id != input.Id) return NotFound();

            // bierz istniejącą wizytę z bazy (ona ma UserId)
            var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null) return NotFound();

            // usuń walidację pól, których nie ma w formularzu
            ModelState.Remove("UserId");
            ModelState.Remove("EndAt");
            ModelState.Remove("Employee");
            ModelState.Remove("Service");

            if (!ModelState.IsValid)
            {
                PopulateDropDowns(input.ServiceId, input.EmployeeId);
                return View(input);
            }

            // przepisz tylko edytowane pola
            appt.ServiceId = input.ServiceId;
            appt.EmployeeId = input.EmployeeId;
            appt.StartAt = input.StartAt;
            appt.ClientName = input.ClientName;
            appt.Note = input.Note;
            appt.Status = input.Status;

            // przelicz EndAt po usłudze
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == appt.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Wybrana usługa nie istnieje.");
                PopulateDropDowns(input.ServiceId, input.EmployeeId);
                return View(input);
            }

            appt.EndAt = appt.StartAt.AddMinutes(service.DurationMinutes);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Delete/5
        // EDYCJA wymagana do usuwania
        [RequireModuleAccess(AppModule.Grafik, AccessLevel.Edit)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        // EDYCJA wymagana do usuwania
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Grafik, AccessLevel.Edit)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropDowns(int? selectedServiceId = null, int? selectedEmployeeId = null)
        {
            ViewData["ServiceId"] = new SelectList(
                _context.Services.OrderBy(s => s.Name),
                "Id", "Name",
                selectedServiceId
            );

            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.OrderBy(e => e.FirstName),
                "Id", "FirstName",
                selectedEmployeeId
            );
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
