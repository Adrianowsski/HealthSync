using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers;

[Authorize(Roles = "Doctor")]
public class SiteContentController : Controller
{
    private readonly AppDbContext _context;

    public SiteContentController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var content = await _context.SiteContents
            .OrderBy(c => c.Key)
            .ToListAsync();

        return View(content);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(SiteContent model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.SiteContents.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Content added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var content = await _context.SiteContents.FindAsync(id);
        return content == null ? NotFound() : View(content);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(SiteContent model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existing = await _context.SiteContents.FindAsync(model.Id);
        if (existing == null) return NotFound();

        existing.Key = model.Key;
        existing.Value = model.Value;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Content updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var content = await _context.SiteContents.FindAsync(id);
        if (content != null)
        {
            _context.SiteContents.Remove(content);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Content deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
