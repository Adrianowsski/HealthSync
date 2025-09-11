using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers;

[Authorize(Roles = "Doctor")]
public class RegistrationCodeController : Controller
{
    private readonly AppDbContext _context;

    public RegistrationCodeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var codes = await _context.RegistrationCodes
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(codes);
    }

    [HttpPost]
    public async Task<IActionResult> Generate()
    {
        var code = new RegistrationCode(); 
        _context.RegistrationCodes.Add(code);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var code = await _context.RegistrationCodes.FindAsync(id);
        return code == null ? NotFound() : View(code);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(RegistrationCode model)
    {
        if (!ModelState.IsValid) return View(model);

        var code = await _context.RegistrationCodes.FindAsync(model.Id);
        if (code == null) return NotFound();

        code.IsUsed = model.IsUsed;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var code = await _context.RegistrationCodes.FindAsync(id);
        if (code != null)
        {
            _context.RegistrationCodes.Remove(code);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}