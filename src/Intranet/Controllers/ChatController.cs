using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers;

[Authorize(Roles = "Doctor")]
public class ChatController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ChatController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Lista pacjentów do czatu
    public async Task<IActionResult> Patients()
    {
        var userId = _userManager.GetUserId(User); // Zwraca GUID zalogowanego lekarza

        var patients = await _context.Appointments
            .Include(a => a.PatientProfile)
            .Include(a => a.DoctorProfile)
            .Where(a => a.DoctorProfile.UserId == userId)
            .Select(a => a.PatientProfile)
            .Distinct()
            .ToListAsync();

        return View(patients); // Widok Views/Chat/Patients.cshtml
    }

    // Widok rozmowy z pacjentem
    public async Task<IActionResult> Conversation(string patientUserId)
    {
        var userId = _userManager.GetUserId(User);
        var appointment = await _context.Appointments
            .Include(a => a.PatientProfile)
            .Include(a => a.DoctorProfile)
            .FirstOrDefaultAsync(a => a.PatientProfile.UserId == patientUserId && a.DoctorProfile.UserId == userId);

        if (appointment == null) return NotFound();

        var messages = await _context.ChatMessages
            .Where(m => m.AppointmentId == appointment.Id)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        ViewData["Patient"] = appointment.PatientProfile;
        ViewData["DoctorId"] = userId;

        return View("Conversation", messages);
    }

    [HttpPost]
    public async Task<IActionResult> Send(string receiverId, string content)
    {
        var senderId = _userManager.GetUserId(User);

        var appointment = await _context.Appointments
            .Include(a => a.PatientProfile)
            .Include(a => a.DoctorProfile)
            .FirstOrDefaultAsync(a => a.PatientProfile.UserId == receiverId && a.DoctorProfile.UserId == senderId);

        if (appointment == null)
        {
            TempData["Error"] = "Appointment not found.";
            return RedirectToAction("Patients");
        }

        var message = new ChatMessage
        {
            AppointmentId = appointment.Id,
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Conversation", new { patientUserId = receiverId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var message = await _context.ChatMessages.FindAsync(id);
        if (message == null) return NotFound();

        return View(message); // Views/Chat/Edit.cshtml
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ChatMessage model)
    {
        var message = await _context.ChatMessages.FindAsync(model.Id);
        if (message == null) return NotFound();

        message.Content = model.Content;
        await _context.SaveChangesAsync();

        return RedirectToAction("Conversation", new { patientUserId = message.ReceiverId });
    }


    [HttpPost]
    public async Task<IActionResult> Delete(int id, string returnPatientUserId)
    {
        var message = await _context.ChatMessages.FindAsync(id);
        if (message != null)
        {
            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Conversation", new { patientUserId = returnPatientUserId });
    }


    public IActionResult Index()
    {
        return RedirectToAction("Patients");
    }
}
