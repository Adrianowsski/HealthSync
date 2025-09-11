using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Portal.Controllers
{
    [Authorize(Roles = "Patient")]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = User.Identity!.Name;

            var patient = await _context.PatientProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.UserName == user);

            if (patient == null)
                return Unauthorized();

            var appointment = await _context.Appointments
                .Include(a => a.DoctorProfile)
                .FirstOrDefaultAsync(a => a.PatientProfileId == patient.Id);

            if (appointment == null)
                return NotFound("No appointment found.");

            var messages = await _context.ChatMessages
                .Where(m => m.AppointmentId == appointment.Id)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewData["DoctorName"] = $"{appointment.DoctorProfile.FirstName} {appointment.DoctorProfile.LastName}";
            ViewData["AppointmentId"] = appointment.Id;
            ViewData["ReceiverId"] = appointment.DoctorProfile.UserId;

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Send(int appointmentId, string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Index");

            var message = new ChatMessage
            {
                AppointmentId = appointmentId,
                SenderId = User.Identity!.Name!,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message == null || message.SenderId != User.Identity!.Name)
                return Unauthorized();

            return View(message);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ChatMessage model)
        {
            var msg = await _context.ChatMessages.FindAsync(model.Id);
            if (msg == null || msg.SenderId != User.Identity!.Name)
                return Unauthorized();

            msg.Content = model.Content;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await _context.ChatMessages.FindAsync(id);
            if (msg == null || msg.SenderId != User.Identity!.Name)
                return Unauthorized();

            _context.ChatMessages.Remove(msg);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
