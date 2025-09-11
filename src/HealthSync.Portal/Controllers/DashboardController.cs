using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HealthSync.Portal.Controllers
{
    [Authorize(Roles = "Patient")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) => _context = context;

        
        public async Task<IActionResult> Index()
        {
            
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.UserName == userEmail);
            if (user == null) return NotFound("User not found.");

            
            var patient = await _context.PatientProfiles
                                        .FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null) return NotFound("Patient not found.");

            
            var appointments = await _context.Appointments
                                             .Include(a => a.DoctorProfile)
                                             .Where(a => a.PatientProfileId == patient.Id)
                                             .OrderByDescending(a => a.AppointmentDate)
                                             .Take(5)
                                             .ToListAsync();

            var prescriptions = await _context.Prescriptions
                                              .Include(p => p.Appointment)
                                              .ThenInclude(a => a.DoctorProfile)
                                              .Where(p => p.Appointment.PatientProfileId == patient.Id)
                                              .OrderByDescending(p => p.Id)
                                              .Take(5)
                                              .ToListAsync();

            var records = await _context.MedicalRecords
                                        .Include(m => m.Appointment)
                                        .ThenInclude(a => a.DoctorProfile)
                                        .Where(m => m.Appointment.PatientProfileId == patient.Id)
                                        .OrderByDescending(m => m.CreatedAt)
                                        .Take(5)
                                        .ToListAsync();

            var chatCount = await _context.ChatMessages
                                          .Where(m => m.SenderId == userEmail || m.ReceiverId == userEmail)
                                          .CountAsync();

            
            var notifications = await _context.Notifications
                                              .Include(n => n.ReadStatuses
                                                             .Where(rs => rs.UserId == user.Id))
                                              .OrderByDescending(n => n.CreatedAt)
                                              .Take(10)
                                              .ToListAsync();

            
            var siteContents = await _context.SiteContents.ToListAsync();
            var whatsNewList = siteContents
                               .Where(s => s.Key.Equals("whatsnew", StringComparison.OrdinalIgnoreCase))
                               .ToList();
            

            
            ViewBag.Patient       = patient;
            ViewBag.Prescriptions = prescriptions;
            ViewBag.Records       = records;
            ViewBag.ChatCount     = chatCount;
            ViewBag.Notifications = notifications;
            ViewBag.WhatsNewList  = whatsNewList;
            

            return View(appointments);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);
            if (user == null) return NotFound();

            var status = await _context.NotificationReadStatuses
                                       .FirstOrDefaultAsync(rs => rs.NotificationId == id &&
                                                                  rs.UserId        == user.Id);

            if (status == null)
            {
                _context.NotificationReadStatuses.Add(new NotificationReadStatus
                {
                    NotificationId = id,
                    UserId         = user.Id,
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
