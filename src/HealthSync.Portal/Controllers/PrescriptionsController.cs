using System;
using System.Linq;
using System.Threading.Tasks;
using HealthSync.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Portal.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PrescriptionsController : Controller
    {
        private readonly AppDbContext _context;

        public PrescriptionsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userName = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
                return Unauthorized();

            // znajdź pacjenta po nazwie użytkownika
            var patientId = await _context.PatientProfiles
                .Where(p => p.User != null && p.User.UserName == userName)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            if (patientId is null)
                return Unauthorized();

            // ładujemy wizytę i lekarza; filtr z null-checkiem
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.DoctorProfile)
                .Where(p => p.Appointment != null &&
                            p.Appointment.PatientProfileId == patientId.Value)
                .OrderByDescending(p => p.Appointment != null
                                            ? p.Appointment.AppointmentDate
                                            : DateTime.MinValue)
                .AsNoTracking()
                .ToListAsync();

            return View(prescriptions);
        }
    }
}
