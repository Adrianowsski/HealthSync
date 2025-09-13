// alias, by nie kolidować z iText.Kernel.Geom.Path
using IOPath = System.IO.Path;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HealthSync.Shared.Data;
using HealthSync.Shared.Models;
using iText.IO.Image;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Intranet.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class MedicalRecordsController : Controller
    {
        private readonly AppDbContext        _ctx;
        private readonly IWebHostEnvironment _env;

        public MedicalRecordsController(AppDbContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        /*──────── INDEX ────────*/
        public async Task<IActionResult> Index()
        {
            var list = await _ctx.MedicalRecords
                                 .Include(r => r.Appointment)
                                 .ThenInclude(a => a.PatientProfile)
                                 .OrderByDescending(r => r.CreatedAt)
                                 .ToListAsync();
            return View(list);
        }

        /*──────── CREATE ───────*/
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Appointments"] = _ctx.Appointments
                                           .Include(a => a.PatientProfile)
                                           .OrderByDescending(a => a.AppointmentDate)
                                           .ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("AppointmentId,Description")] MedicalRecord rec)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Appointments"] = _ctx.Appointments
                                               .Include(a => a.PatientProfile)
                                               .OrderByDescending(a => a.AppointmentDate)
                                               .ToList();
                return View(rec);
            }

            rec.CreatedAt = DateTime.UtcNow;
            _ctx.MedicalRecords.Add(rec);
            await _ctx.SaveChangesAsync();
            TempData["SuccessMessage"] = "Medical record added.";
            return RedirectToAction(nameof(Index));
        }

        /*──────── EDIT ─────────*/
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var rec = await _ctx.MedicalRecords.FindAsync(id);
            if (rec == null) return NotFound();

            ViewData["Appointments"] = _ctx.Appointments
                                           .Include(a => a.PatientProfile)
                                           .OrderByDescending(a => a.AppointmentDate)
                                           .ToList();
            return View(rec);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("Id,AppointmentId,Description")] MedicalRecord rec)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Appointments"] = _ctx.Appointments
                                               .Include(a => a.PatientProfile)
                                               .OrderByDescending(a => a.AppointmentDate)
                                               .ToList();
                return View(rec);
            }

            var db = await _ctx.MedicalRecords.FindAsync(rec.Id);
            if (db == null) return NotFound();

            db.AppointmentId = rec.AppointmentId;
            db.Description   = rec.Description;
            await _ctx.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medical record updated.";
            return RedirectToAction(nameof(Index));
        }

        /*──────── DELETE ───────*/
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rec = await _ctx.MedicalRecords.FindAsync(id);
            if (rec == null)
            {
                TempData["ErrorMessage"] = "Record not found.";
                return RedirectToAction(nameof(Index));
            }

            _ctx.MedicalRecords.Remove(rec);
            await _ctx.SaveChangesAsync();
            TempData["SuccessMessage"] = "Record deleted.";
            return RedirectToAction(nameof(Index));
        }

        /*──────── DOWNLOAD PDF ─*/
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var rec = await _ctx.MedicalRecords
                                .Include(r => r.Appointment).ThenInclude(a => a.PatientProfile)
                                .Include(r => r.Appointment).ThenInclude(a => a.DoctorProfile)
                                .FirstOrDefaultAsync(r => r.Id == id);

            if (rec == null) return NotFound();
            return File(BuildPdf(rec), "application/pdf", $"MedicalRecord_{id}.pdf");
        }

        /*──────── helper PDF ───*/
        private byte[] BuildPdf(MedicalRecord rec)
        {
            using var ms  = new MemoryStream();
            using var wr  = new PdfWriter(ms);
            using var pdf = new PdfDocument(wr);
            using var doc = new Document(pdf, PageSize.A4);

            /* paginacja */
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageEventHandler());

            /* logo */
            var logo = IOPath.Combine(_env.WebRootPath, "images", "logo.png");
            if (System.IO.File.Exists(logo))
            {
                doc.Add(new Image(ImageDataFactory.Create(logo))
                        .ScaleToFit(140, 60)
                        .SetHorizontalAlignment(HorizontalAlignment.LEFT));
            }

            /* nagłówek */
            doc.Add(new Paragraph("Medical Record").SetBold().SetFontSize(20).SetMarginBottom(10));
            doc.Add(new Paragraph($"Record ID: {rec.Id}"));
            doc.Add(new Paragraph($"Created:   {rec.CreatedAt:dd MMM yyyy HH:mm}"));
            doc.Add(new Paragraph($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}"));
            doc.Add(new Paragraph(" "));

            var p  = rec.Appointment.PatientProfile;
            var dr = rec.Appointment.DoctorProfile;

            var info = new Table(UnitValue.CreatePercentArray(new float[] {110, 350}))
                       .UseAllAvailableWidth();
            info.AddCell(Label("Patient")).AddCell(Value($"{p.FirstName} {p.LastName}"));
            info.AddCell(Label("Doctor" )).AddCell(Value($"{dr.FirstName} {dr.LastName}"));
            info.AddCell(Label("Visit"  )).AddCell(Value($"{rec.Appointment.AppointmentDate:f}"));
            doc.Add(info);

            doc.Add(new Paragraph(" "));

            var desc = new Table(1).UseAllAvailableWidth();
            desc.AddHeaderCell(new Cell().Add(new Paragraph("Description").SetBold()));
            desc.AddCell(new Cell().Add(new Paragraph(rec.Description))
                                   .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                                   .SetPadding(8));
            doc.Add(desc);

            /* podpis / pieczątka */
            doc.Add(new Paragraph("\nDoctor signature / stamp:\n\n______________________________")
                     .SetMarginTop(25));

            doc.Close();
            return ms.ToArray();

            static Cell Label(string t) => new Cell().Add(new Paragraph(t).SetBold()).SetPadding(5);
            static Cell Value(string t) => new Cell().Add(new Paragraph(t)).SetPadding(5);
        }

        /*──────── Page N / M handler ───────*/
        private sealed class PageEventHandler : IEventHandler
        {
            public void HandleEvent(Event ev)
            {
                var docEvent = (PdfDocumentEvent)ev;
                var pdf      = docEvent.GetDocument();
                var page     = docEvent.GetPage();
                int n        = pdf.GetPageNumber(page);
                int p        = pdf.GetNumberOfPages();

                var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
                /*  ↓ 2-gi argument to Rectangle – wystarczy page.GetPageSize() */
                new Canvas(canvas, page.GetPageSize())
                    .ShowTextAligned($"Page {n} / {p}", 520, 20, TextAlignment.RIGHT);
            }
        }
    }
}
