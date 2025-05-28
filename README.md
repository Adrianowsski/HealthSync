# HealthSync

> A unified healthcare platform connecting patients and providers through scheduling, messaging, records, and more.

---

## 🚀 Overview
HealthSync streamlines care coordination by enabling:
- **Patients** to book appointments, view records, chat with staff, and manage prescriptions  
- **Staff** to administer site content, handle registrations, and generate reports  



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


---

⚙️ Getting Started
Prerequisites
.NET 8 SDK: https://dotnet.microsoft.com/download

SQL Server (LocalDB or full instance)

Clone & Install
Clone the repo and restore packages
git clone https://github.com/YourUsername/HealthSync.git
cd HealthSync
dotnet restore

Configure your database connection
In each appsettings.json (Portal & Intranet), set:

json
Kopiuj
Edytuj
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HealthSync;Trusted_Connection=True;"
}
Apply EF Core migrations
cd HealthSync.Shared
dotnet ef database update

▶️ Running the Apps

Portal
cd HealthSync.Portal
dotnet run

Intranet
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
Configured ASP.NET Core Identity with role-based policies and authorization filters.

Testing & QA
Wrote unit tests for core services and controllers using xUnit and Moq.



# HealthSync Intranet (Staff/Admin Portal) — Full Screenshot Gallery

> A complete set of 19 annotated screenshots showcasing every major view in the Intranet portal.

| #  | Screenshot                                                                                   | Description                                                        |
|----|----------------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| 01 | ![Landing Page](/images/intranet/01-landing.png)                                             | **Landing Page** – “Welcome to HealthSync” banner and Log In link. |
| 02 | ![Doctor Login](/images/intranet/02-login.png)                                              | **Doctor Login** – secure email/password form.                     |
| 03 | ![Dashboard (top)](/images/intranet/03-dashboard-top.png)                                    | **Dashboard** – welcome greeting, current date/time, upcoming count. |
| 04 | ![Dashboard (details)](/images/intranet/04-dashboard-details.png)                            | **Dashboard Details** – stats cards, notifications, “What’s New,” recent items. |
| 05 | ![Patients List](/images/intranet/05-patients.png)                                           | **Patients List** – search by name/PESEL, filter by initial, edit. |
| 06 | ![Appointments](/images/intranet/06-appointments.png)                                        | **Appointments** – search/filter, edit booking, start chat.        |
| 07 | ![New Appointment](/images/intranet/07-new-appointment.png)                                  | **Add Appointment** – patient selector, date, time slot, status.   |
| 08 | ![Prescriptions](/images/intranet/08-prescriptions.png)                                      | **Prescriptions** – list with patient, medication, dosage, code.   |
| 09 | ![Medical Records](/images/intranet/09-medical-records.png)                                  | **Medical Records** – CRUD table; view/export notes.               |
| 10 | ![Chat Interface](/images/intranet/10-chat.png)                                              | **Chat Interface** – real-time messaging with individual patients. |
| 11 | ![Reports List](/images/intranet/11-reports.png)                                             | **Reports List** – generate, download, edit, or delete PDFs.       |
| 12 | ![Report Preview](/images/intranet/12-report-pdf.png)                                        | **Report Preview** – sample “Medical Records Summary” PDF output.  |
| 13 | ![Registration Codes](/images/intranet/13-registration-codes.png)                            | **Registration Codes** – generate/revoke invite codes; status.     |
| 14 | ![Admin Notifications](/images/intranet/14-notifications-admin.png)                          | **Notifications** – compose and manage site-wide notices.          |
| 15 | ![Site Content](/images/intranet/15-site-content.png)                                        | **Site Content** – edit FAQs, Privacy Policy, “What’s New” items.  |
| 16 | ![Patient Notifications](/images/intranet/16-user-notifications.png)                         | **Patient View: Notifications** – banner of notices with read status. |
| 17 | ![Patient What’s New](/images/intranet/17-whats-new.png)                                     | **Patient View: What’s New** – list of recent feature highlights.  |
| 18 | ![Privacy Policy](/images/intranet/18-privacy-policy.png)                                    | **Patient View: Privacy Policy** – data collection/use/disclosure. |
| 19 | ![FAQ](/images/intranet/19-faq.png)                                                          | **Patient View: FAQ** – collapsible question/answer entries.       |



# HealthSync Portal (Patient Interface) — Full Screenshot Gallery

> A complete set of 12 annotated screenshots showcasing every major view in the Patient portal.

| #   | Screenshot                                                                    | Description                                                      |
|-----|-------------------------------------------------------------------------------|------------------------------------------------------------------|
| 01  | ![Landing](/images/portal/01-landing.png)                                     | **Landing** – “Welcome to HealthSync” banner with Log In & Register buttons. |
| 02  | ![Registration](/images/portal/02-registration.png)                           | **Patient Registration** – email, password, name, PESEL & code.  |
| 03  | ![Login](/images/portal/03-login.png)                                         | **Patient Login** – secure email/password form.                  |
| 04  | ![Login Error](/images/portal/04-login-error.png)                             | **Login Error** – invalid credentials message.                   |
| 05  | ![Dashboard](/images/portal/05-dashboard.png)                                 | **Dashboard** – summary cards (Appointments, Prescriptions, Records, Messages) & upcoming appointments. |
| 06  | ![Notifications](/images/portal/06-notifications.png)                         | **Your Notifications** – unread/read site notices,latest feature highlights.              |
| 07  | ![Appointments](/images/portal/08-appointments.png)                           | **Your Appointments** – list with doctor, date & status.         |
| 08  | ![Prescriptions](/images/portal/09-prescriptions.png)                         | **Your Prescriptions** – list with doctor, date, medication & code. |
| 09  | ![Medical Records](/images/portal/10-medical-records.png)                     | **Medical Records** – visit notes with PDF download action.      |
| 10  | ![Record PDF](/images/portal/11-record-pdf.png)                               | **Record PDF** – detailed PDF view with signature line.         |
| 11  | ![Chat](/images/portal/12-chat.png)                                           | **Chat** – real-time messaging with your doctor.                 |









