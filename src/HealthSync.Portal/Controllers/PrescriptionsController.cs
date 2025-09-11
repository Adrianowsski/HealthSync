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
            var userId = User.Identity!.Name;

            var patientId = await _context.PatientProfiles
                .Where(p => p.User.UserName == userId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (patientId == 0)
                return Unauthorized();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Appointment)
                .ThenInclude(a => a.DoctorProfile)
                .Where(p => p.Appointment.PatientProfileId == patientId)
                .OrderByDescending(p => p.Appointment.AppointmentDate)
                .ToListAsync();

            return View(prescriptions);
        }
    }
}