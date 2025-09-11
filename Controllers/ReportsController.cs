// ─────────────────────────────────────────────────────────────
//  ReportsController  (.NET 8, role Doctor)
//  Single report: PDF + XLSX  (iTextSharp 5 + ClosedXML)
//  Bulk download: PDF ZIP  &  XLSX ZIP   ← fixed disposal bug
//  DB holds only PDF path  → zero migrations
// ─────────────────────────────────────────────────────────────
using System.IO.Compression;
using IOPath = System.IO.Path;

using ClosedXML.Excel;
using HealthSync.Intranet.Services;
using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers;

[Authorize(Roles = "Doctor")]
public sealed class ReportsController : Controller
{
    private readonly AppDbContext        _ctx;
    private readonly IWebHostEnvironment _env;

    public ReportsController(AppDbContext ctx, IWebHostEnvironment env)
    {
        _ctx = ctx;
        _env = env;
    }

    /*──────── LIST ───────────────────────────────────────────*/
    public async Task<IActionResult> Index()
        => View(await _ctx.Reports
                          .OrderByDescending(r => r.GeneratedAt)
                          .ToListAsync());

    /*──────── FORM GET ───────────────────────────────────────*/
    [HttpGet]
    public IActionResult Generate()
    {
        ViewData["ReportTemplates"] = ReportTemplateService.Templates;
        return View();
    }

    /*──────── GENERATE (PDF + XLSX) ──────────────────────────*/
    [HttpPost]
    public async Task<IActionResult> Generate(string title,
                                              DateTime? fromDate,
                                              DateTime? toDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(title), "Select report type.");
            ViewData["ReportTemplates"] = ReportTemplateService.Templates;
            return View();
        }

        var from = fromDate ?? DateTime.MinValue;
        var to   = toDate   ?? DateTime.MaxValue;

        /* 1 — unified names (GUID) */
        var guid     = Guid.NewGuid();
        var pdfName  = $"{guid}.pdf";
        var xlsxName = $"{guid}.xlsx";
        var pdfAbs   = IOPath.Combine(_env.WebRootPath, "reports", pdfName);
        var xlsxAbs  = IOPath.Combine(_env.WebRootPath, "reports", xlsxName);
        Directory.CreateDirectory(IOPath.GetDirectoryName(pdfAbs)!);

        /* 2 — fetch data once */
        object? data = title switch
        {
            "Appointments Summary"    => await _ctx.Appointments
                                                   .Include(a => a.PatientProfile)
                                                   .Where(a => a.AppointmentDate >= from &&
                                                               a.AppointmentDate <= to)
                                                   .ToListAsync(),
            "Prescription Overview"   => await _ctx.Prescriptions
                                                   .Include(p => p.Appointment)
                                                        .ThenInclude(a => a.PatientProfile)
                                                   .Where(p => p.Appointment.AppointmentDate >= from &&
                                                               p.Appointment.AppointmentDate <= to)
                                                   .ToListAsync(),
            "Chat Overview"           => await _ctx.ChatMessages
                                                   .Include(c => c.Appointment)
                                                        .ThenInclude(a => a.PatientProfile)
                                                   .Where(c => c.SentAt >= from && c.SentAt <= to)
                                                   .OrderByDescending(c => c.SentAt)
                                                   .Take(20)
                                                   .ToListAsync(),
            "Patient List"            => await _ctx.PatientProfiles.ToListAsync(),
            "Medical Records Summary" => await _ctx.MedicalRecords
                                                   .Include(r => r.Appointment)
                                                        .ThenInclude(a => a.PatientProfile)
                                                   .Where(r => r.CreatedAt >= from &&
                                                               r.CreatedAt <= to)
                                                   .ToListAsync(),
            _ => null
        };

        /* 3 — PDF */
        using (var fs  = System.IO.File.Create(pdfAbs))
        using (var doc = new Document(PageSize.A4, 50, 50, 60, 50))
        {
            PdfWriter.GetInstance(doc, fs);
            doc.Open();
            AddHeader(doc, title, from, to);
            await FillPdfBodyAsync(doc, title, data);
            doc.Close();
        }

        /* 4 — Excel */
        using (var wb = new XLWorkbook())
        {
            var ws = wb.AddWorksheet("Report");
            ws.Cell(1, 1).Value = $"Report: {title}";
            ws.Cell(2, 1).Value = $"Period: {from:d} – {to:d}";
            ws.Cell(3, 1).Value = $"Generated: {DateTime.Now:g}";
            ws.Cell(4, 1).Value = $"Generated by: {User.Identity?.Name ?? "System"}";

            int startRow = 6;
            switch (title)
            {
                case "Appointments Summary":
                    ws.Cell(startRow, 1).InsertTable(
                        ((List<Appointment>)data!).Select(a => new
                        {
                            a.AppointmentDate,
                            Patient = a.PatientProfile.FirstName + " " +
                                      a.PatientProfile.LastName,
                            a.Status
                        }));
                    break;

                case "Prescription Overview":
                    ws.Cell(startRow, 1).InsertTable(
                        ((List<Prescription>)data!)
                            .GroupBy(p => p.MedicationName)
                            .Select(g => new { Medication = g.Key, Count = g.Count() }));
                    break;

                case "Chat Overview":
                    ws.Cell(startRow, 1).InsertTable(
                        ((List<ChatMessage>)data!).Select(c => new
                        {
                            c.SentAt,
                            Patient = c.Appointment.PatientProfile.FirstName,
                            c.Content
                        }));
                    break;

                case "Patient List":
                    ws.Cell(startRow, 1).InsertTable(
                        ((List<PatientProfile>)data!)
                            .Select(p => new { p.FirstName, p.LastName, p.PESEL }));
                    break;

                case "Medical Records Summary":
                    ws.Cell(startRow, 1).InsertTable(
                        ((List<MedicalRecord>)data!).Select(r => new
                        {
                            r.CreatedAt,
                            Patient = r.Appointment.PatientProfile.FirstName + " " +
                                      r.Appointment.PatientProfile.LastName,
                            r.Description
                        }));
                    break;
            }
            wb.SaveAs(xlsxAbs);
        }

        /* 5 — DB record (PDF path only) */
        _ctx.Reports.Add(new Report
        {
            Title       = title,
            GeneratedAt = DateTime.Now,
            GeneratedBy = User.Identity?.Name ?? "System",
            FilePath    = $"/reports/{pdfName}",
            PeriodFrom  = fromDate,
            PeriodTo    = toDate
        });
        await _ctx.SaveChangesAsync();

        TempData["SuccessMessage"] = "PDF and Excel successfully generated.";
        return RedirectToAction(nameof(Index));
    }

    /*──────── BULK ZIP (PDF) ─────────────────────────────────*/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkDownloadPdf([FromForm] int[] ids)
        => await BulkZipAsync(ids,
                              ".pdf",
                              "No PDF files to download.",
                              "reports_pdf_{0:yyyyMMdd_HHmmss}.zip");

    /*──────── BULK ZIP (XLSX) ───────────────────────────────*/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkDownloadXlsx([FromForm] int[] ids)
        => await BulkZipAsync(ids,
                              ".xlsx",
                              "No Excel files to download.",
                              "reports_xlsx_{0:yyyyMMdd_HHmmss}.zip");

    /*──────── central ZIP helper ────────────────────────────*/
    private async Task<IActionResult> BulkZipAsync(int[]  ids,
                                                   string ext,
                                                   string emptyMsg,
                                                   string zipFmt)
    {
        if (ids == null || ids.Length == 0)
        {
            TempData["ErrorMessage"] = "Nothing selected.";
            return RedirectToAction(nameof(Index));
        }

        var reports = await _ctx.Reports
                                .Where(r => ids.Contains(r.Id))
                                .ToListAsync();

        var files = reports
            .Select(r =>
            {
                var pdfAbs = IOPath.Combine(_env.WebRootPath,
                                            r.FilePath.TrimStart('/'));
                return IOPath.ChangeExtension(pdfAbs, ext);
            })
            .Where(System.IO.File.Exists)
            .ToList();

        if (files.Count == 0)
        {
            TempData["ErrorMessage"] = emptyMsg;
            return RedirectToAction(nameof(Index));
        }

        /* ⬇⬇  FIX: nie zamykać MemoryStream przed wysyłką  ⬇⬇ */
        var zipMs = new MemoryStream();                 // brak using !!

        using (var zip = new ZipArchive(zipMs,
                                        ZipArchiveMode.Create,
                                        leaveOpen: true))
        {
            foreach (var file in files)
            {
                zip.CreateEntryFromFile(file,
                                        IOPath.GetFileName(file),
                                        CompressionLevel.Fastest);
            }
        }
        zipMs.Position = 0;                             // reset

        return File(zipMs,
                    "application/zip",
                    string.Format(zipFmt, DateTime.Now));
    }

    /*──────── helpers (PDF layout) ──────────────────────────*/
    private static void AddHeader(Document d, string t,
                                  DateTime f, DateTime to)
    {
        var h1 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var h2 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
        var p  = FontFactory.GetFont(FontFactory.HELVETICA, 11);

        d.Add(new Paragraph("HealthSync Medical Report", h1));
        d.Add(new Paragraph($"Report: {t}", h2));
        d.Add(new Paragraph($"Period: {f:d} – {to:d}", p));
        d.Add(new Paragraph($"Generated: {DateTime.Now:g}", p));
        d.Add(Chunk.NEWLINE);
    }

    private async Task FillPdfBodyAsync(Document d,
                                        string   t,
                                        object?  data)
    {
        var h2 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
        var th = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

        PdfPTable T(int c, params string[] head)
        {
            var tbl = new PdfPTable(c) { WidthPercentage = 100 };
            foreach (var h in head) tbl.AddCell(new Phrase(h, th));
            return tbl;
        }

        switch (t)
        {
            case "Appointments Summary":
                var a = (List<Appointment>)data!;
                d.Add(new Paragraph($"Total: {a.Count}", h2));
                var ta = T(3, "Date", "Patient", "Status");
                foreach (var x in a)
                {
                    ta.AddCell(x.AppointmentDate.ToString("g"));
                    ta.AddCell($"{x.PatientProfile.FirstName} {x.PatientProfile.LastName}");
                    ta.AddCell(x.Status);
                }
                d.Add(ta);
                break;

            case "Prescription Overview":
                var p = (List<Prescription>)data!;
                var grp = p.GroupBy(x => x.MedicationName)
                           .OrderByDescending(g => g.Count());
                d.Add(new Paragraph($"Total: {p.Count}", h2));
                var tp = T(2, "Medication", "Count");
                foreach (var g in grp)
                {
                    tp.AddCell(g.Key);
                    tp.AddCell(g.Count().ToString());
                }
                d.Add(tp);
                break;

            case "Chat Overview":
                var c = (List<ChatMessage>)data!;
                d.Add(new Paragraph("Last 20 Messages", h2));
                var tc = T(3, "Time", "Patient", "Message");
                foreach (var m in c)
                {
                    tc.AddCell(m.SentAt.ToString("g"));
                    tc.AddCell(m.Appointment.PatientProfile.FirstName);
                    tc.AddCell(m.Content);
                }
                d.Add(tc);
                break;

            case "Patient List":
                var ppl = (List<PatientProfile>)data!;
                d.Add(new Paragraph($"Total: {ppl.Count}", h2));
                var tpl = T(3, "First Name", "Last Name", "PESEL");
                foreach (var q in ppl)
                {
                    tpl.AddCell(q.FirstName);
                    tpl.AddCell(q.LastName);
                    tpl.AddCell(q.PESEL);
                }
                d.Add(tpl);
                break;

            case "Medical Records Summary":
                var r = (List<MedicalRecord>)data!;
                d.Add(new Paragraph($"Total: {r.Count}", h2));
                var tr = T(3, "Date", "Patient", "Description");
                foreach (var z in r)
                {
                    tr.AddCell(z.CreatedAt.ToString("g"));
                    tr.AddCell($"{z.Appointment.PatientProfile.FirstName} " +
                               $"{z.Appointment.PatientProfile.LastName}");
                    tr.AddCell(z.Description);
                }
                d.Add(tr);
                break;
        }
    }

    /*──────── Delete & BulkDelete – unchanged ───────────────*/
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
        => await DoDeleteAsync(new[] { id });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkDelete([FromForm] int[] ids)
        => await DoDeleteAsync(ids);

    private async Task<IActionResult> DoDeleteAsync(int[] ids)
    {
        if (ids.Length == 0)
        {
            TempData["ErrorMessage"] = "Nothing selected.";
            return RedirectToAction(nameof(Index));
        }

        var reps = await _ctx.Reports
                             .Where(r => ids.Contains(r.Id))
                             .ToListAsync();

        foreach (var rep in reps)
        {
            var pdfAbs  = IOPath.Combine(_env.WebRootPath,
                                         rep.FilePath.TrimStart('/'));
            var xlsxAbs = IOPath.ChangeExtension(pdfAbs, ".xlsx");
            if (System.IO.File.Exists(pdfAbs))  System.IO.File.Delete(pdfAbs);
            if (System.IO.File.Exists(xlsxAbs)) System.IO.File.Delete(xlsxAbs);
        }

        _ctx.Reports.RemoveRange(reps);
        await _ctx.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{reps.Count} report(s) deleted.";
        return RedirectToAction(nameof(Index));
    }
}
