
using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using HealthSync.Intranet.Models.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HealthSync.Intranet.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _ctx;
        public NotificationController(AppDbContext ctx) => _ctx = ctx;
        
        public async Task<IActionResult> Index()
        {
            var notifications = await _ctx.Notifications
                                          .Include(n => n.ReadStatuses)
                                          .OrderByDescending(n => n.CreatedAt)
                                          .ToListAsync();
            return View(notifications);
        }
        
        public async Task<IActionResult> Details(int id)
        {

            var note = await _ctx.Notifications
                .Include(n => n.ReadStatuses)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (note == null) return NotFound();


            var allPatients = await _ctx.PatientProfiles
                .Include(p => p.User)
                .ToListAsync();


            var rows = allPatients.Select(p =>
                {
                    var st = note.ReadStatuses.FirstOrDefault(rs => rs.UserId == p.User.Id);
                    return new NotificationDetailsViewModel.Row
                    {
                        Name   = $"{p.FirstName} {p.LastName}",
                        Email  = p.User.Email,
                        IsRead = st?.IsRead ?? false,
                        ReadAt = st?.ReadAt
                    };
                })

                .OrderBy(r => r.Name)
                .ToList();


            var vm = new NotificationDetailsViewModel
            {
                Message   = note.Message,
                Created   = note.CreatedAt,
                Patients  = rows
            };

            return View(vm);
        }



        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Patients"] = GetPatientSelectList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Notification model, List<string> targetUserIds)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Patients"] = GetPatientSelectList();
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _ctx.Notifications.Add(model);
            await _ctx.SaveChangesAsync();

            var statuses = targetUserIds.Distinct().Select(uid => new NotificationReadStatus
            {
                NotificationId = model.Id,
                UserId         = uid,
                IsRead         = false
            });
            _ctx.NotificationReadStatuses.AddRange(statuses);
            await _ctx.SaveChangesAsync();

            TempData["SuccessMessage"] = "Notification created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var note = await _ctx.Notifications.FindAsync(id);
            return note == null ? NotFound() : View(note);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Notification model)
        {
            if (!ModelState.IsValid) return View(model);

            var note = await _ctx.Notifications.FindAsync(model.Id);
            if (note == null) return NotFound();

            note.Message = model.Message;
            await _ctx.SaveChangesAsync();

            TempData["SuccessMessage"] = "Notification updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _ctx.Notifications.FindAsync(id);
            if (note != null)
            {
                _ctx.Notifications.Remove(note);
                _ctx.NotificationReadStatuses
                    .RemoveRange(_ctx.NotificationReadStatuses.Where(rs => rs.NotificationId == id));
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ───────── helper ──────────────────────────────────────────────────
        private IEnumerable<SelectListItem> GetPatientSelectList() =>
            _ctx.PatientProfiles
                .Include(p => p.User)
                .Select(p => new SelectListItem
                {
                    Value = p.User.Id,
                    Text  = $"{p.FirstName} {p.LastName} ({p.User.Email})"
                })
                .OrderBy(s => s.Text);
    }
}
