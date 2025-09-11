// ───────────────────────────────────────────────────────
//  Controllers/MedicalRecordsController.cs
// ───────────────────────────────────────────────────────
using IOPath = System.IO.Path;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Commons.Actions;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Portal.Controllers
{
    [Authorize(Roles = "Patient")]
    public class MedicalRecordsController : Controller
    {
        private readonly AppDbContext        _ctx;
        private readonly IWebHostEnvironment _env;

        public MedicalRecordsController(AppDbContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        /*──────── LIST ────────*/
        public async Task<IActionResult> Index(string? search)
        {
            var userName = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName)) return Unauthorized();

            var patient = await _ctx.PatientProfiles
                                    .Include(p => p.User)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.User != null && p.User.UserName == userName);
            if (patient is null) return NotFound();

            var query = _ctx.MedicalRecords
                            .Include(r => r.Appointment)
                                .ThenInclude(a => a.PatientProfile)
                            .AsNoTracking()
                            .Where(r => r.Appointment != null &&
                                        r.Appointment.PatientProfileId == patient.Id);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r => (r.Description ?? "").ToLower().Contains(s));
            }

            var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            ViewData["Search"] = search;
            return View(list);
        }

        /*──────── SINGLE PDF ────────*/
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var userName = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName)) return Unauthorized();

            var rec = await _ctx.MedicalRecords
                                .Include(r => r.Appointment)
                                    .ThenInclude(a => a.PatientProfile)
                                        .ThenInclude(pp => pp.User)
                                .Include(r => r.Appointment)
                                    .ThenInclude(a => a.DoctorProfile)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r =>
                                    r.Id == id &&
                                    r.Appointment != null &&
                                    r.Appointment.PatientProfile != null &&
                                    r.Appointment.PatientProfile.User != null &&
                                    r.Appointment.PatientProfile.User.UserName == userName);

            if (rec is null) return NotFound();

            var pdf = BuildPdf(rec);
            return File(pdf, "application/pdf", $"MedicalRecord_{id}.pdf");
        }

        /*──────── BULK ZIP ────────*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDownload([FromForm] int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["ErrorMessage"] = "No records selected.";
                return RedirectToAction(nameof(Index));
            }

            var userName = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName)) return Unauthorized();

            var records = await _ctx.MedicalRecords
                                    .Include(r => r.Appointment)
                                        .ThenInclude(a => a.PatientProfile)
                                            .ThenInclude(pp => pp.User)
                                    .Include(r => r.Appointment)
                                        .ThenInclude(a => a.DoctorProfile)
                                    .AsNoTracking()
                                    .Where(r => ids.Contains(r.Id) &&
                                                r.Appointment != null &&
                                                r.Appointment.PatientProfile != null &&
                                                r.Appointment.PatientProfile.User != null &&
                                                r.Appointment.PatientProfile.User.UserName == userName)
                                    .ToListAsync();

            if (records.Count == 0) return NotFound();

            using var zipMs = new MemoryStream();
            using (var zip = new ZipArchive(zipMs, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var rec in records)
                {
                    var entry = zip.CreateEntry($"MedicalRecord_{rec.Id}.pdf");
                    await using var es = entry.Open();
                    var pdfBytes = BuildPdf(rec);
                    await es.WriteAsync(pdfBytes);
                }
            }

            zipMs.Position = 0;
            return File(zipMs.ToArray(), "application/zip", "MedicalRecords.zip");
        }

        /*──────── PDF builder ────────*/
        private byte[] BuildPdf(MedicalRecord rec)
        {
            using var ms  = new MemoryStream();
            using var pdf = new PdfDocument(new PdfWriter(ms));
            using var doc = new Document(pdf, PageSize.A4);

            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageFooter());

            // logo (bezpiecznie sprawdzamy WebRootPath)
            if (!string.IsNullOrEmpty(_env.WebRootPath))
            {
                var logoPath = IOPath.Combine(_env.WebRootPath, "images", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    doc.Add(new Image(ImageDataFactory.Create(logoPath))
                            .ScaleToFit(140, 60)
                            .SetHorizontalAlignment(HorizontalAlignment.LEFT));
                }
            }

            // header
            doc.Add(Bold("Medical Record", 20).SetMarginBottom(10));
            doc.Add(new Paragraph($"Record ID: {rec.Id}"));
            doc.Add(new Paragraph($"Created:   {rec.CreatedAt:dd MMM yyyy HH:mm}"));
            doc.Add(new Paragraph($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}")
                        .SetMarginBottom(15));

            // safe navigation
            var appt = rec.Appointment;
            var patientName = appt?.PatientProfile != null
                                ? $"{appt.PatientProfile.FirstName} {appt.PatientProfile.LastName}"
                                : "-";
            var doctorName = appt?.DoctorProfile != null
                                ? $"{appt.DoctorProfile.FirstName} {appt.DoctorProfile.LastName}"
                                : "-";
            var visitText = appt != null ? appt.AppointmentDate.ToString("f") : "-";

            var tbl = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
            Row(tbl, "Patient", patientName);
            Row(tbl, "Doctor",  doctorName);
            Row(tbl, "Visit",   visitText);
            doc.Add(tbl.SetMarginBottom(15));

            // description
            doc.Add(Bold("Description"));
            doc.Add(new Paragraph(rec.Description ?? string.Empty)
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetPadding(8));

            // signature
            doc.Add(new Paragraph("\nPatient signature:\n\n______________________________")
                        .SetMarginTop(25));

            doc.Close();
            return ms.ToArray();
        }

        /*──────── small DRY helpers ────────*/
        static Paragraph Bold(string text, float? size = null)
        {
            var p = new Paragraph(text).SimulateBold();
            if (size.HasValue) p.SetFontSize(size.Value);
            return p;
        }

        static void Row(Table t, string label, string value)
        {
            t.AddCell(Cell(label, true));
            t.AddCell(Cell(value));
        }

        static Cell Cell(string text, bool bold = false)
        {
            var p = new Paragraph(text ?? string.Empty);
            if (bold) p.SimulateBold();
            return new Cell().Add(p).SetPadding(5);
        }

        /*──────── footer handler ────────*/
        private sealed class PageFooter : AbstractPdfDocumentEventHandler
        {
            protected override void OnAcceptedEvent(AbstractPdfDocumentEvent ev)
            {
                var e   = (PdfDocumentEvent)ev;
                var pdf = e.GetDocument();
                var pg  = e.GetPage();

                var canvas = new PdfCanvas(pg.NewContentStreamAfter(), pg.GetResources(), pdf);
                new Canvas(canvas, pg.GetPageSize())
                    .ShowTextAligned(
                        $"Page {pdf.GetPageNumber(pg)} / {pdf.GetNumberOfPages()}",
                        520, 20, TextAlignment.RIGHT);
            }
        }
    }
}
