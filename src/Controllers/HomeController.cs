using HealthSync.Shared.Data;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Intranet.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Welcome()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        var content = _context.SiteContents.FirstOrDefault(c => c.Key.ToLower() == "privacy");
        ViewData["Content"] = content?.Value ?? "<p>Privacy Policy not found.</p>";
        return View();
    }

    public IActionResult FAQ()
    {
        var content = _context.SiteContents.FirstOrDefault(c => c.Key.ToLower() == "faq");
        ViewData["Content"] = content?.Value ?? "<p>No FAQ content available.</p>";
        return View();
    }
}