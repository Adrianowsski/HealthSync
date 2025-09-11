using HealthSync.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Portal.Controllers
{
    [Authorize(Roles = "Patient")]
    public class AppointmentsController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity!.Name;

            var appointments = await _context.Appointments
                .Include(a => a.DoctorProfile)
                .Include(a => a.PatientProfile)
                .Where(a => a.PatientProfile.User.UserName == userEmail)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }
    }
}