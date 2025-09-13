using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthSync.Intranet.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // ───────────────────────── DASHBOARD ─────────────────────────
        public async Task<IActionResult> Index()
        {
            // 1. bieżący lekarz (User.Identity.Name -> e-mail/login ASP.NET Identity)
            var doctorUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(doctorUserName))
                return Unauthorized();

            var doctorUser = await _context.Users
                                           .FirstOrDefaultAsync(u => u.UserName == doctorUserName);
            if (doctorUser == null)
                return NotFound("Doctor user not found.");

            // 2. najbliższe wizyty
            var appointments = await _context.Appointments
                                             .Include(a => a.PatientProfile)
                                             .Where(a => a.AppointmentDate >= DateTime.Now)
                                             .OrderBy(a => a.AppointmentDate)
                                             .Take(5)
                                             .ToListAsync();

            // 3. powiadomienia + flagi odczytu dla tego lekarza
            var notifications = await _context.Notifications
                                              .Include(n => n.ReadStatuses
                                                             .Where(rs => rs.UserId == doctorUser.Id))
                                              .OrderByDescending(n => n.CreatedAt)
                                              .Take(3)
                                              .ToListAsync();

            // 4. recepty
            var prescriptions = await _context.Prescriptions
                                              .Include(p => p.Appointment)
                                              .ThenInclude(a => a.PatientProfile)
                                              .OrderByDescending(p => p.Id)
                                              .Take(3)
                                              .ToListAsync();

            // 5. rekordy medyczne
            var medicalRecords = await _context.MedicalRecords
                                               .Include(r => r.Appointment)
                                               .ThenInclude(a => a.PatientProfile)
                                               .OrderByDescending(r => r.CreatedAt)
                                               .Take(3)
                                               .ToListAsync();

            // 6. statystyki
            var stats = new
            {
                TotalPatients      = await _context.PatientProfiles.CountAsync(),
                TotalAppointments  = await _context.Appointments.CountAsync(),
                TotalPrescriptions = await _context.Prescriptions.CountAsync(),
                TotalReports       = await _context.Reports.CountAsync()
            };

            // 7. sekcja „What’s new”
            var whatsNew = await _context.SiteContents
                                         .Where(c => c.Key.ToLower() == "whatsnew")
                                         .Select(c => c.Value)
                                         .FirstOrDefaultAsync();

            // 8. przekazanie danych do widoku
            ViewData["Stats"]          = stats;
            ViewData["Appointments"]   = appointments;
            ViewData["Notifications"]  = notifications;
            ViewData["Prescriptions"]  = prescriptions;
            ViewData["MedicalRecords"] = medicalRecords;
            ViewData["WhatsNew"]       = whatsNew;

            return View();
        }

        // ───────────────────────── MARK-AS-READ ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var doctorUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(doctorUserName))
                return Unauthorized();

            var doctorUser = await _context.Users
                                           .FirstOrDefaultAsync(u => u.UserName == doctorUserName);
            if (doctorUser == null)
                return NotFound();

            var status = await _context.NotificationReadStatuses
                                       .FirstOrDefaultAsync(rs => rs.NotificationId == id &&
                                                                  rs.UserId        == doctorUser.Id);

            if (status == null)
            {
                _context.NotificationReadStatuses.Add(new NotificationReadStatus
                {
                    NotificationId = id,
                    UserId         = doctorUser.Id,
                    IsRead         = true,
                    ReadAt         = DateTime.UtcNow
                });
            }
            else if (!status.IsRead)
            {
                status.IsRead = true;
                status.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
