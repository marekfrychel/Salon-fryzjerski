using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalonFryzjerski.Data;
using SalonFryzjerski.Models;
using SalonFryzjerski.Security;

namespace SalonFryzjerski.Controllers
{
    // Cały kontroler: minimum PODGLĄD
    [RequireModuleAccess(AppModule.Uslugi, AccessLevel.View)]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
            return View(await _context.Services.ToListAsync());
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // GET: Services/Create  -> EDYCJA
        [RequireModuleAccess(AppModule.Uslugi, AccessLevel.Edit)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Services/Create -> EDYCJA
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Uslugi, AccessLevel.Edit)]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,DurationMinutes,Price,IsActive")] Service service)
        {
            if (!ModelState.IsValid) return View(service);

            _context.Add(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Services/Edit/5 -> EDYCJA
        [RequireModuleAccess(AppModule.Uslugi, AccessLevel.Edit)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            return View(service);
        }

        // POST: Services/Edit/5 -> EDYCJA
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Uslugi, AccessLevel.Edit)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DurationMinutes,Price,IsActive")] Service service)
        {
            if (id != service.Id) return NotFound();

            if (!ModelState.IsValid) return View(service);

            try
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(service.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Services/Delete/5 -> EDYCJA
        [RequireModuleAccess(AppModule.Uslugi, AccessLevel.Edit)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // POST: Services/Delete/5 -> EDYCJA
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequireModuleAccess(AppModule.Uslugi, AccessLevel.Edit)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
