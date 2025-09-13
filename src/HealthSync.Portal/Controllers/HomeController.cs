using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context) => _context = context;


        public async Task<IActionResult> Index()
        {

            var content = await _context.SiteContents
                .Where(c => c.Key.ToLower() == "home")
                .ToListAsync();

            return View(content);   
        }


        public async Task<IActionResult> FAQ()
        {
            var items = await _context.SiteContents
                .Where(c => c.Key.ToLower() == "faq")
                .ToListAsync();

            return View(items);
        }


        public async Task<IActionResult> Privacy()
        {
            var items = await _context.SiteContents
                .Where(c => c.Key.ToLower() == "privacy")
                .ToListAsync();

            return View(items);
        }
    }
}