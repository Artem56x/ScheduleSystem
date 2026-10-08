<div align="center">

<img src="https://capsule-render.vercel.app/api?type=blur&color=0:0A84FF,50:5E5CE6,100:AF52DE&height=220&section=header&text=ScheduleSystem&fontSize=58&fontColor=FFFFFF&animation=fadeIn&fontAlignY=38&desc=Smart%20College%20Schedule%20Management%20System&descAlignY=62&descSize=18&descColor=FFFFFF" width="100%"/>

<br>

<img src="https://readme-typing-svg.demolab.com?font=JetBrains+Mono&weight=600&size=20&duration=2800&pause=900&color=0A84FF&center=true&vCenter=true&width=750&lines=Smart+Schedule+Management;ASP.NET+Core+MVC+%2B+PostgreSQL;Conflict+Detection+%2B+Schedule+Validation;Automatic+Schedule+Generation;Modern+Liquid+Glass+UI;Built+with+C%23+%F0%9F%92%99" alt="Typing SVG"/>

<br><br>

<img src="https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white" alt="C#"/>
<img src="https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core"/>
<img src="https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8"/>
<img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL"/>
<img src="https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="Entity Framework Core"/>

<br>

<img src="https://img.shields.io/badge/MVC-0A84FF?style=for-the-badge" alt="MVC"/>
<img src="https://img.shields.io/badge/Razor-0A84FF?style=for-the-badge" alt="Razor"/>
<img src="https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white" alt="HTML5"/>
<img src="https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white" alt="CSS3"/>
<img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black" alt="JavaScript"/>

<br><br>

<a href="https://github.com/Artem56x/ScheduleSystem">
<img src="https://img.shields.io/github/stars/Artem56x/ScheduleSystem?style=flat-square&logo=github&label=Stars" alt="Stars"/>
</a>

<a href="https://github.com/Artem56x/ScheduleSystem/network/members">
<img src="https://img.shields.io/github/forks/Artem56x/ScheduleSystem?style=flat-square&logo=github&label=Forks" alt="Forks"/>
</a>

<a href="https://github.com/Artem56x/ScheduleSystem">
<img src="https://img.shields.io/github/repo-size/Artem56x/ScheduleSystem?style=flat-square&label=Repository%20Size" alt="Repository Size"/>
</a>

<img src="https://img.shields.io/badge/Status-Completed-34C759?style=flat-square" alt="Status"/>

<br><br>

<a href="docs/">
<img src="https://img.shields.io/badge/📚%20Documentation-0A84FF?style=for-the-badge&logo=readthedocs&logoColor=white" alt="Documentation"/>
</a>

</div>

---


---

# 🗓️ ScheduleSystem

**ScheduleSystem** is a modern web application for creating, managing, validating, automatically generating, and exporting college schedules.

The project was developed with **ASP.NET Core MVC, .NET 8, Entity Framework Core, and PostgreSQL**.

The main idea is not simply to store a timetable, but to help create a **consistent schedule without conflicts and invalid combinations**.

---

## ✨ Features

### 📅 Schedule Management

* Create lessons
* Edit lessons
* Delete lessons
* View the complete schedule
* Filter schedules
* View schedules by group
* View schedules by teacher
* View schedules by classroom
* View schedules by subject
* Filter by day and time
* Validate schedule conflicts
* Export schedules

---

### 🔎 Smart Search & Filtering

The schedule can be filtered by:

```text
👥 Group
👨‍🏫 Teacher
📚 Subject
🏫 Classroom
📆 Day
⏰ Time
```

This makes it possible to quickly find the required lessons without navigating through the entire timetable.

---

# 🧠 Smart Schedule Validation

One of the core parts of ScheduleSystem is its **schedule validation system**.

Before a lesson is saved, the system checks whether the selected combination is logically valid.

### ⏱️ Time Conflicts

For example:

```text
10:00 ───────────── 11:30
              11:00 ───────────── 12:30
                    ▲
                    │
               CONFLICT
```

The system checks several resources simultaneously:

```text
👨‍🏫 Teacher
👥 Group
🏫 Classroom
```

This prevents situations where the same teacher, group, or classroom is assigned to multiple lessons at the same time.

---

## 🏫 Smart Classroom Recommendations

The classroom recommendation system considers more than simple availability.

It can take into account:

* Classroom capacity
* Number of students
* Classroom category
* Subject requirements
* Existing classroom schedule
* Group requirements

For example:

```text
Group: 25БД-1
Students: 12

Classroom 404
Capacity: 10

❌ Not suitable
```

The system can recommend classrooms that satisfy the required conditions.

```text
Available suitable classrooms:

102   → +8 seats
105   → +15 seats
107   → +23 seats
```

---

# 💻 Classroom Categories

Different subjects can require different types of classrooms.

For example:

```text
💻 Computer Science
        ↓
Computer Classroom

🔬 Laboratory Work
        ↓
Laboratory

📖 Lecture
        ↓
Lecture Room
```

Classroom categories are stored in the database and can be extended without changing the fundamental application structure.

---

# 👨‍🏫 Teachers

The teacher management system supports:

* Creating teachers
* Editing teachers
* Deleting teachers
* Viewing teacher information
* Assigning multiple subjects
* Viewing individual teacher schedules

### Teacher ↔ Subject

Teachers and subjects use a **many-to-many relationship**.

```text
┌──────────────┐
│    Teacher   │
└──────┬───────┘
       │
       │
       ▼
┌────────────────┐
│ TeacherSubject │
└───────┬────────┘
        │
        ▼
┌──────────────┐
│    Subject   │
└──────────────┘
```

This allows:

```text
Teacher A
 ├── Mathematics
 ├── Programming
 └── Computer Science
```

while a subject can also have multiple teachers.

---

# 👥 Student Groups

Groups contain information used by the scheduling system, including:

* Group name
* Course
* Number of students
* Group-subject relationships

The number of students is especially important when selecting a suitable classroom.

```text
Group
   │
   ├── Course
   ├── Student Count
   └── Subjects
```

---

# 📚 Subjects

Subjects are independent database entities.

They can be associated with:

* Teachers
* Student groups
* Classroom requirements
* Course
* Subject type

This allows the same subject to be consistently used throughout the scheduling system.

---

# 🏫 Classrooms

Each classroom contains information such as:

```text
Name
Category
Capacity
```

Classroom categories are used by the recommendation and validation systems to determine whether a room is suitable for a particular lesson.

---

# 🤖 Automatic Schedule Generation

ScheduleSystem includes an automatic timetable generation system.

The generator considers:

```text
┌──────────────────────────────────┐
│      Schedule Generator          │
├──────────────────────────────────┤
│                                  │
│  Teacher availability            │
│  Group requirements              │
│  Subject assignments             │
│  Classroom capacity              │
│  Classroom categories            │
│  Course compatibility            │
│  Existing conflicts              │
│  Daily lesson limits             │
│  Weekly lesson requirements      │
│  Allowed schedule gaps           │
│                                  │
└──────────────────────────────────┘
```

### Default Generation Settings

| Setting                 | Default |
| ----------------------- | ------: |
| Weekly lessons          |       3 |
| Maximum lessons per day |       4 |
| Teaching days per week  |       5 |
| Maximum gaps per day    |       2 |
| Allow gaps              |     Yes |

---

# 🔔 Notifications

The application includes a notification system for important schedule events.

Example:

```text
┌──────────────────────────────────────┐
│ 🔔 Notification                      │
├──────────────────────────────────────┤
│                                      │
│ Computer Graphics                    │
│ Group: 25БД-1                        │
│                                      │
│ Time: 08:30–09:30                    │
│                                      │
└──────────────────────────────────────┘
```

Users can access notifications through the notification interface integrated into the application layout.

---

# 📝 Audit Logs

ScheduleSystem includes an audit logging system for tracking important application events.

This provides additional visibility into administrative actions and changes made within the system.

---

# 🔐 Authentication & Authorization

Authentication is implemented using **ASP.NET Core Identity**.

Supported roles:

```text
Admin
User
```

Administrators have access to management and administrative functionality.

Regular users have access to standard timetable functionality according to their permissions.

### Secure Administrator Configuration

The administrator password is **not stored in source code**.

It can be provided through:

```text
Admin:Password
```

or:

```text
Admin__Password
```

Environment variables and secure configuration should be used for production credentials.

---

# 📤 Schedule Export

Schedules can be exported into several formats:

| Format | Support |
| ------ | :-----: |
| PDF    |    ✅    |
| Excel  |    ✅    |
| CSV    |    ✅    |

The system supports exporting:

* Complete schedules
* Group schedules
* Teacher schedules

---

# 🏗️ Architecture

ScheduleSystem is built using the **ASP.NET Core MVC architecture**.

```text
                         ScheduleSystem
                               │
              ┌────────────────┼────────────────┐
              │                │                │
              ▼                ▼                ▼
        Controllers          Models           Views
              │                │                │
              │                │                │
              └────────────────┼────────────────┘
                               │
                               ▼
                         ApplicationDbContext
                               │
                               ▼
                           PostgreSQL
```

### Project Structure

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
│   ├── ScheduleGenerationResult.cs
│   ├── ScheduleGenerationSettings.cs
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
│
├── Migrations/
│
├── wwwroot/
│   ├── css/
│   └── js/
│
├── Program.cs
├── appsettings.json
└── ScheduleSystem.csproj
```

---

# 🛠️ Tech Stack

<div align="center">

| Technology                | Purpose                        |
| ------------------------- | ------------------------------ |
| **C#**                    | Main programming language      |
| **ASP.NET Core MVC**      | Web framework                  |
| **.NET 8**                | Application platform           |
| **Entity Framework Core** | ORM                            |
| **PostgreSQL**            | Database                       |
| **Npgsql**                | PostgreSQL provider            |
| **ASP.NET Core Identity** | Authentication & authorization |
| **Razor**                 | Server-side views              |
| **HTML5**                 | Application markup             |
| **CSS3**                  | UI styling                     |
| **JavaScript**            | Client-side logic              |
| **Git / GitHub**          | Version control                |

</div>

---

# 🎨 UI / Design

The interface follows a modern **Liquid Glass / Glassmorphism-inspired** design.

```text
┌──────────────────────────────────────────┐
│                                          │
│          ScheduleSystem                  │
│                                          │
│   ┌──────────────┐   ┌──────────────┐   │
│   │   Teachers   │   │    Groups    │   │
│   └──────────────┘   └──────────────┘   │
│                                          │
│   ┌──────────────┐   ┌──────────────┐   │
│   │   Subjects   │   │  Classrooms  │   │
│   └──────────────┘   └──────────────┘   │
│                                          │
│             Schedule                     │
│                                          │
└──────────────────────────────────────────┘
```

### Design principles

* Clean white interface
* Blue accent colors
* Glass-style cards
* Rounded components
* Soft shadows
* Responsive layouts
* Consistent spacing
* Clear navigation
* Modern forms
* Interactive filters
* Minimal visual noise

The goal is to make a complex scheduling system feel simple and intuitive.

---

# 🗄️ Database

ScheduleSystem uses **PostgreSQL** with **Entity Framework Core**.

Database:

```text
schedulesystem
```

### Main entities

```text
┌───────────────────────────┐
│          Users            │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│         Teachers          │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│      TeacherSubjects      │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│         Subjects          │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│     GroupSubjects         │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│          Groups           │
└───────────────────────────┘


┌───────────────────────────┐
│         Schedules         │
├───────────────────────────┤
│ Teacher                   │
│ Group                     │
│ Subject                   │
│ Classroom                 │
│ Time                      │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│        Classrooms         │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│    ClassroomCategories    │
└───────────────────────────┘
```

Entity Framework Core migrations are included in the repository.

---

# ⚙️ Installation

## 1. Clone the repository

```bash
git clone https://github.com/Artem56x/ScheduleSystem.git
cd ScheduleSystem
```

## 2. Install dependencies

Make sure you have:

* .NET 8 SDK
* PostgreSQL
* Git
* Entity Framework Core CLI

Check the .NET version:

```bash
dotnet --version
```

---

## 3. Create PostgreSQL database

Create a database named:

```text
schedulesystem
```

Configure the PostgreSQL connection string using your local configuration.

---

## 4. Restore dependencies

```bash
dotnet restore
```

---

## 5. Apply migrations

```bash
dotnet ef database update
```

---

## 6. Configure administrator password

PowerShell:

```powershell
$env:Admin__Password="YOUR_ADMIN_PASSWORD"
```

Windows Command Prompt:

```cmd
set Admin__Password=YOUR_ADMIN_PASSWORD
```

The password is intentionally not stored in the source code.

---

## 7. Build

```bash
dotnet build
```

---

## 8. Run

```bash
dotnet run
```

ASP.NET Core will display the local application URL in the terminal.

---

# 🧰 Entity Framework Core

Create a migration:

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

# 🔒 Security

The project follows several basic security practices:

* ASP.NET Core Identity authentication
* Role-based authorization
* Password hashing through Identity
* No hardcoded administrator password
* Server-side validation
* Entity Framework Core database access
* Protected administrative functionality
* Secrets excluded from source control

For production deployment, additional security measures should be configured, including:

* HTTPS
* Secure cookies
* Production secret management
* Database access restrictions
* Production logging
* Environment-specific configuration

---

# 📊 Project Status

<div align="center">

## 🟢 Completed

</div>

```text
Authentication             ████████████████████ 100%
Authorization              ████████████████████ 100%
Database                   ████████████████████ 100%
CRUD                       ████████████████████ 100%
Schedule Management        ████████████████████ 100%
Validation                 ████████████████████ 100%
Generation                 ████████████████████ 100%
Recommendations            ████████████████████ 100%
Notifications              ████████████████████ 100%
Audit Logs                 ████████████████████ 100%
Export                     ████████████████████ 100%
UI / UX                    ████████████████████ 100%
```

### Implemented

* [x] ASP.NET Core MVC architecture
* [x] .NET 8
* [x] PostgreSQL database
* [x] Entity Framework Core
* [x] ASP.NET Core Identity
* [x] Role-based authorization
* [x] Teacher management
* [x] Multiple subjects per teacher
* [x] Group management
* [x] Subject management
* [x] Classroom management
* [x] Classroom categories
* [x] Group-subject relationships
* [x] Schedule management
* [x] Schedule validation
* [x] Conflict detection
* [x] Automatic schedule generation
* [x] Classroom recommendations
* [x] Notifications
* [x] Audit logs
* [x] Schedule filtering
* [x] PDF export
* [x] Excel export
* [x] CSV export
* [x] User profile
* [x] User settings
* [x] Responsive Liquid Glass interface
* [x] Entity Framework Core migrations

---

# 🚀 Possible Future Improvements

The current version is complete for its intended college practice scope.

Possible future extensions include:

* [ ] Advanced schedule optimization algorithms
* [ ] More detailed analytics
* [ ] Calendar integrations
* [ ] Email notifications
* [ ] Mobile application
* [ ] REST API
* [ ] Docker deployment
* [ ] Cloud deployment
* [ ] Advanced timetable visualization

---

# 📌 Project Information

```text
Project:       ScheduleSystem
Version:       1.0
Platform:      Web
Framework:     ASP.NET Core MVC
Runtime:       .NET 8
Language:      C#
Database:      PostgreSQL
ORM:           Entity Framework Core
Architecture:  MVC
Status:        Completed
```

---

# 🎓 Academic Project

ScheduleSystem was developed as a **college practice project** focused on applying software development skills to a real-world educational scheduling problem.

The project combines:

```text
                 ScheduleSystem
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
   Web Development  Database Design  UI / UX
        │              │              │
        └──────────────┼──────────────┘
                       │
                       ▼
              Business Logic
                       │
                       ▼
             Schedule Generation
                       │
                       ▼
              Conflict Validation
```

The project demonstrates practical experience with:

* Backend development
* Database design
* MVC architecture
* Entity relationships
* Authentication
* Authorization
* Business logic
* Validation
* Scheduling algorithms
* UI/UX design
* Git and GitHub

---

# 👨‍💻 Author

<div align="center">

### Artem Dudnik

**Software Development Student · Kazakhstan**

<br>

<a href="https://github.com/Artem56x">
<img src="https://img.shields.io/badge/GitHub-Artem56x-181717?style=for-the-badge&logo=github&logoColor=white" alt="GitHub"/>
</a>

<br><br>

⭐ If you like the project, consider giving it a star.

</div>

---

# 💙 Built with C# & ASP.NET Core

<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0A84FF,50:5E5CE6,100:AF52DE&height=120&section=footer" width="100%"/>

</div>
