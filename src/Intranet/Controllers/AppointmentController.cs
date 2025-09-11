using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers;

[Authorize(Roles = "Doctor")]
public class AppointmentController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public AppointmentController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var appointments = await _context.Appointments
            .Include(a => a.PatientProfile)
            .Include(a => a.DoctorProfile)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Patients"] = _context.PatientProfiles.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(IFormCollection form)
    {
        ViewData["Patients"] = _context.PatientProfiles.ToList();

        var dateStr = form["AppointmentDate"];
        var patientIdStr = form["PatientProfileId"];
        var status = form["Status"];

        if (!DateTime.TryParse(dateStr, out var appointmentDate))
        {
            ModelState.AddModelError("AppointmentDate", "Invalid appointment date.");
            return View();
        }

        if (!int.TryParse(patientIdStr, out var patientId))
        {
            ModelState.AddModelError("PatientProfileId", "Please select a patient.");
            return View();
        }

        if (string.IsNullOrEmpty(status))
        {
            ModelState.AddModelError("Status", "Please select a status.");
            return View();
        }

        var userId = _userManager.GetUserId(User);
        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null)
        {
            ModelState.AddModelError("", "Doctor profile not found.");
            return View();
        }

        var appointment = new Appointment
        {
            AppointmentDate = appointmentDate,
            PatientProfileId = patientId,
            DoctorProfileId = doctor.Id,
            Status = status
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Appointment created successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound();

        ViewData["Patients"] = _context.PatientProfiles.ToList();
        return View(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(IFormCollection form)
    {
        ViewData["Patients"] = _context.PatientProfiles.ToList();

        // 1. Parsowanie Id
        if (!int.TryParse(form["Id"], out var id))
            return NotFound();

        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound();

        // 2. Parsowanie pola Patient
        if (!int.TryParse(form["PatientProfileId"], out var patientId))
        {
            ModelState.AddModelError("PatientProfileId", "Please select a valid patient.");
            return View(appointment);
        }

        // 3. Parsowanie daty i godziny
        if (!DateTime.TryParse(form["AppointmentDate"], out var appointmentDate))
        {
            ModelState.AddModelError("AppointmentDate", "Invalid appointment date.");
            return View(appointment);
        }

        // 4. Status
        var status = form["Status"];
        if (string.IsNullOrEmpty(status))
        {
            ModelState.AddModelError("Status", "Please select a status.");
            return View(appointment);
        }

        // 5. Aktualizacja danych
        appointment.PatientProfileId = patientId;
        appointment.AppointmentDate = appointmentDate;
        appointment.Status = status;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Appointment updated successfully.";
        return RedirectToAction("Index");
    }


    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment deleted successfully.";
        }

        return RedirectToAction("Index");
    }
}
