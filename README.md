# ScheduleSystem

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8">
  <img src="https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core">
  <img src="https://img.shields.io/badge/PostgreSQL-17-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/Entity_Framework_Core-8.0-512BD4?style=for-the-badge" alt="Entity Framework Core">
</p>

<p align="center">
  <strong>A modern web-based college timetable management system.</strong>
</p>

<p align="center">
  Create • Manage • Validate • Generate • Export
</p>

<p align="center">
  <a href="https://github.com/Artem56x/ScheduleSystem">
    <img src="https://img.shields.io/badge/GitHub-Repository-181717?style=flat-square&logo=github" alt="GitHub">
  </a>
  <img src="https://img.shields.io/badge/Status-Completed-success?style=flat-square" alt="Status">
  <img src="https://img.shields.io/badge/License-Educational-blue?style=flat-square" alt="License">
</p>

---

## Overview

**ScheduleSystem** is a web application for creating, managing, validating, automatically generating, and exporting college class schedules.

The project was developed as a college practice project using **ASP.NET Core MVC, .NET 8, Entity Framework Core, and PostgreSQL**.

The main goal of the system is to replace manual timetable management with a centralized application that can handle teachers, subjects, student groups, classrooms, scheduling rules, conflicts, and exports.

---

## Features

### Schedule Management

* Create and edit schedules
* View the complete timetable
* Filter schedules by:

  * Group
  * Teacher
  * Subject
  * Classroom
  * Day
  * Time
* Detect schedule conflicts
* Validate timetable rules
* Prevent invalid schedule combinations

### Automatic Schedule Generation

The system includes an automatic timetable generator that considers:

* Teacher availability
* Teacher-subject assignments
* Group-subject assignments
* Classroom capacity
* Classroom categories
* Subject requirements
* Course compatibility
* Existing schedule conflicts
* Daily lesson limits
* Weekly lesson requirements
* Allowed gaps

### Academic Management

The application provides complete management of:

* 👨‍🏫 Teachers
* 📚 Subjects
* 👥 Student groups
* 🏫 Classrooms
* 📅 Schedules

Teachers can be assigned to multiple subjects using a dedicated many-to-many relationship.

---

## Dashboard

The main dashboard provides a quick overview of the system and its current data.

```text
┌─────────────────────────────────────────────────────────┐
│                    ScheduleSystem                       │
├─────────────────────────────────────────────────────────┤
│                                                         │
│   👨‍🏫 Teachers     👥 Groups     📚 Subjects             │
│                                                         │
│   🏫 Classrooms    📅 Schedules                         │
│                                                         │
│                 Schedule Overview                       │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Schedule Generator

The generator uses configurable scheduling parameters.

| Setting                 | Default |
| ----------------------- | ------: |
| Weekly lessons          |       3 |
| Maximum lessons per day |       4 |
| Teaching days per week  |       5 |
| Maximum gaps per day    |       2 |
| Allow gaps              |     Yes |

The generator attempts to create a valid timetable while respecting the configured constraints.

---

## Validation

Schedule validation checks multiple constraints before data is saved.

### Teacher conflicts

A teacher cannot be assigned to two lessons at the same time.

### Group conflicts

A student group cannot have multiple lessons at the same time.

### Classroom conflicts

A classroom cannot be occupied by multiple lessons simultaneously.

### Classroom capacity

The classroom capacity must be sufficient for the selected group.

### Classroom requirements

Subjects can require specific classroom categories.

For example:

```text
Computer Graphics
        ↓
Computer Classroom
```

### Teacher compatibility

The system checks whether the selected teacher is assigned to the selected subject.

The administrator can also explicitly confirm a teacher-subject combination when necessary.

---

## Data Model

The main relationships can be represented as:

```text
                         ┌──────────────┐
                         │    Teacher   │
                         └──────┬───────┘
                                │
                                │ many-to-many
                                │
                         ┌──────▼───────┐
                         │TeacherSubject│
                         └──────┬───────┘
                                │
                                │
                         ┌──────▼───────┐
                         │    Subject   │
                         └──────────────┘


┌──────────────┐       ┌──────────────┐
│    Group     │──────▶│ GroupSubject │
└──────────────┘       └──────┬───────┘
                              │
                              ▼
                       ┌──────────────┐
                       │    Subject   │
                       └──────────────┘


┌──────────────┐
│   Schedule   │
├──────────────┤
│ Group        │
│ Teacher      │
│ Subject      │
│ Classroom    │
│ Date / Time  │
└──────────────┘
```

---

## Authentication & Authorization

ScheduleSystem uses **ASP.NET Core Identity**.

### Roles

* `Admin`
* `User`

Administrators have access to system management and configuration functionality.

Regular users have access to standard timetable functionality according to their permissions.

### Secure Administrator Password

The administrator password is **not hardcoded in the source code**.

It can be supplied through configuration:

```text
Admin:Password
```

or through an environment variable:

```text
Admin__Password
```

Never commit real passwords or other secrets to the repository.

---

## Notifications

The application includes a notification system for important timetable events.

Notifications can contain information such as:

```text
Computer Graphics
Group: 25БД-1

Time:
08:30–09:30

Classroom:
Computer Classroom
```

The interface includes a notification bell for quick access to user notifications.

---

## Audit Logs

Administrative actions and important system events can be recorded through the audit logging system.

This provides additional visibility into changes made within the application.

---

## Export

Schedules can be exported in multiple formats.

| Format | Supported |
| ------ | :-------: |
| PDF    |     ✅     |
| Excel  |     ✅     |
| CSV    |     ✅     |

The system supports exporting both the general timetable and schedules filtered by specific teachers or groups.

---

## User Interface

ScheduleSystem uses a modern **Liquid Glass-inspired** interface.

### Design principles

* Clean white background
* Blue accent colors
* Glass-style cards
* Rounded interfaces
* Responsive layouts
* Modern forms
* Clear navigation
* Consistent spacing
* Smooth interactive elements

The interface is designed to feel closer to a modern desktop or mobile application than a traditional college administration system.

---

## Technology Stack

<p align="center">

| Layer             | Technology                    |
| ----------------- | ----------------------------- |
| Language          | **C#**                        |
| Framework         | **ASP.NET Core MVC**          |
| Runtime           | **.NET 8**                    |
| ORM               | **Entity Framework Core**     |
| Database          | **PostgreSQL**                |
| Authentication    | **ASP.NET Core Identity**     |
| Database Provider | **Npgsql**                    |
| Frontend          | **HTML5 / CSS3 / JavaScript** |
| Views             | **Razor**                     |
| Version Control   | **Git / GitHub**              |

</p>

---

## Project Structure

```text
ScheduleSystem/
│
├── Controllers/
│   ├── AccountController.cs
│   ├── AnalyticsController.cs
│   ├── AuditLogsController.cs
│   ├── ClassroomsController.cs
│   ├── GroupsController.cs
│   ├── GroupSubjectsController.cs
│   ├── HomeController.cs
│   ├── NotificationsController.cs
│   ├── ScheduleGeneratorController.cs
│   ├── SchedulesController.cs
│   ├── SubjectsController.cs
│   └── TeachersController.cs
│
├── Data/
│   ├── ApplicationDbContext.cs
│   └── IdentitySeeder.cs
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── AuditLog.cs
│   ├── Classroom.cs
│   ├── ClassroomCategory.cs
│   ├── Group.cs
│   ├── GroupSubject.cs
│   ├── Notification.cs
│   ├── Schedule.cs
│   ├── ScheduleGenerationSettings.cs
│   ├── ScheduleGenerationResult.cs
│   ├── ScheduleTimeSlot.cs
│   ├── Subject.cs
│   ├── SubjectClassroomCategory.cs
│   ├── SubjectType.cs
│   ├── Teacher.cs
│   └── TeacherSubject.cs
│
├── Services/
│   ├── ClassroomRecommendationService.cs
│   ├── ScheduleExportService.cs
│   ├── ScheduleGeneratorService.cs
│   └── ScheduleValidationService.cs
│
├── ViewComponents/
│   └── NotificationBellViewComponent.cs
│
├── Views/
│   ├── Account/
│   ├── Analytics/
│   ├── AuditLogs/
│   ├── Classrooms/
│   ├── Groups/
│   ├── Home/
│   ├── Notifications/
│   ├── Schedules/
│   ├── Shared/
│   ├── Subjects/
│   └── Teachers/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── ...
│
├── Migrations/
│
├── Program.cs
├── appsettings.json
└── ScheduleSystem.csproj
```

---

## Database

ScheduleSystem uses **PostgreSQL**.

Database:

```text
schedulesystem
```

Entity Framework Core migrations are included in the repository.

The main database entities include:

```text
Users
Teachers
Subjects
TeacherSubjects
Groups
GroupSubjects
Classrooms
ClassroomCategories
SubjectClassroomCategories
Schedules
ScheduleTimeSlots
ScheduleGenerationSettings
Notifications
AuditLogs
```

---

## Requirements

Before running the project, install:

* [.NET 8 SDK](https://dotnet.microsoft.com/)
* [PostgreSQL](https://www.postgresql.org/)
* Git
* Entity Framework Core CLI

Verify .NET:

```bash
dotnet --version
```

---

## Installation

### 1. Clone the repository

```bash
git clone https://github.com/Artem56x/ScheduleSystem.git
cd ScheduleSystem
```

### 2. Create the database

Create a PostgreSQL database:

```text
schedulesystem
```

### 3. Configure the connection

Configure the PostgreSQL connection string.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=schedulesystem;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

For production, use environment variables or a secure secret-management solution.

### 4. Configure administrator password

PowerShell:

```powershell
$env:Admin__Password="YOUR_ADMIN_PASSWORD"
```

Command Prompt:

```cmd
set Admin__Password=YOUR_ADMIN_PASSWORD
```

### 5. Restore dependencies

```bash
dotnet restore
```

### 6. Apply migrations

```bash
dotnet ef database update
```

### 7. Build

```bash
dotnet build
```

### 8. Run

```bash
dotnet run
```

The terminal will display the local application URL.

---

## Development

Run with automatic recompilation:

```bash
dotnet watch
```

Create a new migration:

```bash
dotnet ef migrations add MigrationName
```

Apply migrations:

```bash
dotnet ef database update
```

Check for pending model changes:

```bash
dotnet ef migrations has-pending-model-changes
```

---

## Git Workflow

The project uses the `main` branch.

```bash
git pull origin main
git add .
git commit -m "Describe your changes"
git push origin main
```

---

## Project Status

<p align="center">

### ✅ Project Completed

</p>

Current functionality includes:

* ✅ Authentication
* ✅ Authorization
* ✅ Teacher management
* ✅ Multiple subjects per teacher
* ✅ Group management
* ✅ Subject management
* ✅ Classroom management
* ✅ Classroom categories
* ✅ Group-subject relationships
* ✅ Schedule management
* ✅ Automatic schedule generation
* ✅ Schedule validation
* ✅ Conflict detection
* ✅ Notifications
* ✅ Audit logs
* ✅ Filtering
* ✅ PDF export
* ✅ Excel export
* ✅ CSV export
* ✅ User profile
* ✅ Settings
* ✅ PostgreSQL persistence
* ✅ Entity Framework Core migrations
* ✅ Responsive Liquid Glass interface

---

## Roadmap

Possible future improvements:

* [ ] Advanced schedule optimization
* [ ] More detailed analytics
* [ ] Calendar integration
* [ ] Email notifications
* [ ] Mobile application
* [ ] Deployment to a cloud platform
* [ ] Additional scheduling algorithms
* [ ] Improved timetable visualization

---

## Author

<p align="center">
  <strong>Artem Dudnik</strong>
</p>

<p align="center">
  Software Development Student · Kazakhstan
</p>

<p align="center">
  <a href="https://github.com/Artem56x">
    <img src="https://img.shields.io/badge/GitHub-Artem56x-181717?style=for-the-badge&logo=github&logoColor=white" alt="GitHub">
  </a>
</p>

---

## Academic Project

ScheduleSystem was developed as a **college practice project** focused on applying software development skills to a real-world timetable management problem.

The project combines:

```text
Software Development
        +
Database Design
        +
Web Development
        +
Business Logic
        +
Scheduling Algorithms
        +
User Interface Design
```

---

## License

This project was developed for educational and demonstration purposes as part of college practice.

Unless otherwise specified, the source code is intended for educational use.

---

<p align="center">

**ScheduleSystem**

*Modern timetable management for educational institutions.*

</p>
