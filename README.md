[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/) [![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-blue)](https://learn.microsoft.com/aspnet/core) [![Build Status](https://img.shields.io/github/actions/workflow/status/YourUsername/HealthSync/ci.yml?branch=main)](https://github.com/YourUsername/HealthSync/actions) [![License: MIT](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Build](https://github.com/Adrianowsski/HealthSync/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Adrianowsski/HealthSync/actions)

# 🏥 HealthSync

*A unified healthcare platform that connects **patients** and **providers** for friction‑less scheduling, secure messaging, and digital medical records.*

---

## 📌 Table of Contents

* [🚀 Overview](#🚀-overview)
* [✨ Features](#✨-features)
* [🛠️ Tech Stack](#🛠️-tech-stack)
* [🏗️ Solution Layout](#🏗️-solution-layout)
* [⚙️ Getting Started](#⚙️-getting-started)
* [📸 Screenshot Galleries](#📸-screenshot-galleries)
* [📄 License](#📄-license)

---

## 🚀 Overview

HealthSync streamlines care coordination by letting:

| Role         | Capabilities                                                                                                     |
| ------------ | ---------------------------------------------------------------------------------------------------------------- |
| **Patients** | ‑ Book & cancel appointments  <br> ‑ View medical records & prescriptions  <br> ‑ Chat securely with staff       |
| **Staff**    | ‑ Manage appointments & records  <br> ‑ Approve prescriptions  <br> ‑ Bulk reports & XLS/PDF exports             |
| **Admins**   | ‑ Generate invite codes  <br> ‑ Maintain site content (FAQ, Privacy Policy, What’s New)  <br> ‑ System reporting |

---

## ✨ Features

1. **User Management** – ASP.NET Core Identity with "Patient", "Staff", "Admin" roles.
2. **Appointment Scheduling** – calendar UI, status workflow, reminders.
3. **Medical Records** – secure CRUD, image attachments, one‑click PDF export.
4. **Secure Messaging** – real‑time chat (SignalR‑ready) between patients & staff.
5. **Prescription Management** – request → approve lifecycle, QR code & PDF.
6. **Admin Dashboard** – KPI widgets, content CMS, registration‑code management.
7. **Bulk Reporting** – multi‑select delete and batch export to XLS/PDF.
8. **Excel Roster Export** – one‑click `.xlsx` of all registered patients.

---

## 🛠️ Tech Stack

| Layer         | Technology                                                    |
| ------------- | ------------------------------------------------------------- |
| **Framework** | ASP.NET Core MVC (.NET 8)                                     |
| **Database**  | Entity Framework Core 8 + SQL Server (Code‑First)             |
| **Auth**      | ASP.NET Core Identity (JWT ready)                             |
| **Realtime**  | SignalR‑compatible architecture (classic controllers for now) |
| **Frontend**  | Razor Views + Bootstrap 5, jQuery Validation                  |
| **Testing**   | xUnit, Moq, FluentAssertions                                  |
| **DevOps**    | GitHub Actions – CI, code analysis, automated tests           |

---

## 🏗️ Solution Layout

```text
HealthSync.sln
│
├─ HealthSync.Portal/      # Patient‑facing app (MVC)
├─ HealthSync.Intranet/    # Staff/Admin portal (MVC)
└─ HealthSync.Shared/      # DbContext, entities, migrations, utilities
```

---

## ⚙️ Getting Started

### 🔑 Prerequisites

* **.NET 8 SDK**
* **SQL Server** (Express / LocalDB)

### 🏃‍♂️ Quick Setup

```bash
# 1 Clone
 git clone https://github.com/YourUsername/HealthSync.git
 cd HealthSync

# 2 Restore & build
 dotnet restore
 dotnet build -c Release

# 3 Configure DB (appsettings.json) – default uses LocalDB
#    "Server=(localdb)\\mssqllocaldb;Database=HealthSync;Trusted_Connection=True;"

# 4 Migrate & seed sample data
 cd HealthSync.Shared
 dotnet ef database update

# 5 Run apps (two terminals)
# Portal (patients)
 cd ../HealthSync.Portal
 dotnet run

# Intranet (staff/admin)
 cd ../HealthSync.Intranet
 dotnet run
```

> **Docker** 💡  A `docker-compose.yml` is included for one‑command spin‑up of SQL Server and both MVC apps:
>
> ```bash
> docker compose up -d
> ```

---

## 📸 Screenshot Galleries

### Intranet (Staff/Admin)

> Images live in `images/intranet/` – 19 shots total.

| #  | Screenshot                                      | Description                  |
| -- | ----------------------------------------------- | ---------------------------- |
| 01 | ![](images/intranet/01-landing.png)             | Landing page with login link |
| 02 | ![](images/intranet/02-login.png)               | Doctor login                 |
| 03 | ![](images/intranet/03-dashboard-top.png)       | Dashboard KPIs               |
| 04 | ![](images/intranet/04-dashboard-details.png)   | Dashboard details            |
| 05 | ![](images/intranet/05-patients.png)            | Patients list                |
| 06 | ![](images/intranet/06-appointments.png)        | Appointments board           |
| 07 | ![](images/intranet/07-new-appointment.png)     | New appointment form         |
| 08 | ![](images/intranet/08-prescriptions.png)       | Prescriptions list           |
| 09 | ![](images/intranet/09-medical-records.png)     | Medical records CRUD         |
| 10 | ![](images/intranet/10-chat.png)                | Realtime chat                |
| 11 | ![](images/intranet/11-reports.png)             | Reports list + batch actions |
| 12 | ![](images/intranet/12-report-pdf.png)          | PDF preview                  |
| 13 | ![](images/intranet/12,5-excel.png)             | Excel roster export          |
| 14 | ![](images/intranet/13-registration-codes.png)  | Registration codes manager   |
| 15 | ![](images/intranet/14-notifications-admin.png) | Admin notifications          |
| 16 | ![](images/intranet/15-site-content.png)        | CMS – site content           |
| 17 | ![](images/intranet/16-user-notifications.png)  | Patient notifications banner |
| 18 | ![](images/intranet/17-whats-new.png)           | What’s New highlights        |
| 19 | ![](images/intranet/18-privacy-policy.png)      | Privacy Policy page          |
| 20 | ![](images/intranet/19-faq.png)                 | FAQ page                     |

### Portal (Patients)

> Images live in `images/portal/` – 11 shots total.

| #  | Screenshot                                | Description            |
| -- | ----------------------------------------- | ---------------------- |
| 01 | ![](images/portal/01-landing.png)         | Landing page           |
| 02 | ![](images/portal/02-registration.png)    | Registration form      |
| 03 | ![](images/portal/03-login.png)           | Login form             |
| 04 | ![](images/portal/04-login-error.png)     | Login error            |
| 05 | ![](images/portal/05-dashboard.png)       | Patient dashboard      |
| 06 | ![](images/portal/06-notifications.png)   | Notifications list     |
| 07 | ![](images/portal/08-appointments.png)    | Appointments list      |
| 08 | ![](images/portal/09-prescriptions.png)   | Prescriptions list     |
| 09 | ![](images/portal/10-medical-records.png) | Medical records viewer |
| 10 | ![](images/portal/11-record-pdf.png)      | Record PDF preview     |
| 11 | ![](images/portal/12-chat.png)            | Doctor‑patient chat    |

---

## 📄 License

Released under the [MIT License](LICENSE).

---

*Update URLs, badges, and connection strings to your environment before release.*
