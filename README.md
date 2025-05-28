# HealthSync

> A unified healthcare platform connecting patients and providers through scheduling, messaging, records, and more.

---

## 🚀 Overview
HealthSync streamlines care coordination by enabling:
- **Patients** to book appointments, view records, chat with staff, and manage prescriptions  
- **Staff** to administer site content, handle registrations, and generate reports  

> <!-- High-level summary of both user-facing and admin functionality. -->

---

## 🛠️ Tech Stack
- **Framework**: ASP.NET Core MVC (.NET 8)  
- **Data**: Entity Framework Core + SQL Server  
- **Auth**: ASP.NET Core Identity (Roles: Patient, Staff, Admin)  
- **Real-time**: Chat via MVC controllers & EF Core  
- **Frontend**: Razor Views, Bootstrap 5  
- **Structure**:  
  - **HealthSync.Portal** (Patient portal)  
  - **HealthSync.Intranet** (Staff admin)  
  - **HealthSync.Shared** (Shared models, DbContext, migrations)  

> <!-- “Tech Stack” groups related pieces; project structure avoids repetition. -->

---

## ✨ Features

1. **User Management**  
   Registration, login, role-based access (Patient, Staff, Admin).

2. **Appointment Scheduling**  
   Book, view, and cancel appointments with calendar integration.

3. **Medical Records**  
   Secure CRUD operations for patient records and historical data.

4. **Secure Messaging**  
   Real-time chat between patients and staff (via ChatController).

5. **Prescription Management**  
   View, request, and approve prescriptions.

6. **Admin Dashboard**  
   Content management (SiteContent), registration codes, and reporting.

> <!-- Numbered list for clarity—no repeated “Users can…” intro. -->

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)  
- SQL Server (LocalDB or full instance)  

### Clone & Install
```bash
git clone https://github.com/YourUsername/HealthSync.git
cd HealthSync
dotnet restore                   # restore NuGet packages


Configuration
Connection String
In each appsettings.json (Portal & Intranet):

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HealthSync;Trusted_Connection=True;"
}

Apply Migrations
cd HealthSync.Shared
dotnet ef database update

▶️ Running the Apps

Portal:

bash
cd HealthSync.Portal
dotnet run
Intranet:

bash
cd HealthSync.Intranet
dotnet run


🧑‍💻 My Contributions
Architecture
Designed multi-project solution (Portal, Intranet, Shared).

Data Layer
Modeled EF Core entities, migrations, and AppDbContext.

Controllers & Views
Implemented MVC controllers (Appointments, Chat, Records, Prescriptions) and reusable Razor components.

Security
Configured Identity with role-based policies and authorization filters.

Testing & QA
Wrote unit tests for core services and controllers (xUnit, Moq).
