using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers;

[Authorize(Roles = "Doctor")]
public class PrescriptionsController : Controller
{
    private readonly AppDbContext _context;

    public PrescriptionsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var prescriptions = await _context.Prescriptions
            .Include(p => p.Appointment)
                .ThenInclude(a => a.PatientProfile)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return View(prescriptions);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var appointments = _context.Appointments
            .Include(a => a.PatientProfile)
            .OrderByDescending(a => a.AppointmentDate)
            .ToList();

        ViewData["Appointments"] = appointments;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [Bind("AppointmentId,MedicationName,Dosage,Duration,Instructions")] Prescription prescription)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Appointments"] = _context.Appointments
                .Include(a => a.PatientProfile)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(prescription);
        }

        prescription.AccessCode ??= $"RX{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Prescription created successfully.";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();

        ViewData["Appointments"] = _context.Appointments
            .Include(a => a.PatientProfile)
            .OrderByDescending(a => a.AppointmentDate)
            .ToList();

        return View(prescription);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Prescription prescription)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Appointments"] = _context.Appointments
                .Include(a => a.PatientProfile)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(prescription);
        }

        _context.Prescriptions.Update(prescription);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Prescription updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription != null)
        {
            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Prescription deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
