using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers;

[Authorize(Roles = "Doctor")]
public class PatientsController : Controller
{
    private readonly AppDbContext _context;

    public PatientsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var patients = await _context.PatientProfiles.Include(p => p.User).ToListAsync();
        return View(patients);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _context.PatientProfiles.FindAsync(id);
        if (patient == null)
            return NotFound();

        return View(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PatientProfile model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existing = await _context.PatientProfiles.FindAsync(model.Id);
        if (existing == null)
            return NotFound();

        existing.FirstName = model.FirstName;
        existing.LastName = model.LastName;
        existing.PESEL = model.PESEL;
        existing.Address = model.Address;
        existing.PhoneNumber = model.PhoneNumber;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Patient updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.PatientProfiles.FindAsync(id);
        if (patient != null)
        {
            _context.PatientProfiles.Remove(patient);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Patient deleted successfully.";
        }

        return RedirectToAction("Index");
    }
}