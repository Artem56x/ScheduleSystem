<div align="center">

<img src="https://capsule-render.vercel.app/api?type=blur&color=0:0A84FF,50:5E5CE6,100:AF52DE&height=220&section=header&text=ScheduleSystem&fontSize=58&fontColor=FFFFFF&animation=fadeIn&fontAlignY=38&desc=Smart%20College%20Schedule%20Management%20System&descAlignY=62&descSize=18&descColor=FFFFFF" width="100%"/>

<br>

<img src="https://readme-typing-svg.demolab.com?font=JetBrains+Mono&weight=600&size=20&duration=2800&pause=900&color=0A84FF&center=true&vCenter=true&width=700&lines=Smart+Schedule+Management;ASP.NET+Core+MVC+%2B+PostgreSQL;Conflict+Detection+%2B+Smart+Recommendations;Modern+Glassmorphism+UI;Built+with+C%23+%F0%9F%92%99" alt="Typing SVG" />

<br><br>

<img src="https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white"/>
<img src="https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white"/>
<img src="https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>

<br>

<img src="https://img.shields.io/badge/MVC-0A84FF?style=for-the-badge"/>
<img src="https://img.shields.io/badge/Razor-0A84FF?style=for-the-badge"/>
<img src="https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white"/>
<img src="https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white"/>
<img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black"/>

<br><br>

<a href="https://github.com/Artem56x/ScheduleSystem">
<img src="https://img.shields.io/github/stars/Artem56x/ScheduleSystem?style=flat-square&logo=github&label=Stars"/>
</a>

<a href="https://github.com/Artem56x/ScheduleSystem/network/members">
<img src="https://img.shields.io/github/forks/Artem56x/ScheduleSystem?style=flat-square&logo=github&label=Forks"/>
</a>

<a href="https://github.com/Artem56x/ScheduleSystem">
<img src="https://img.shields.io/github/repo-size/Artem56x/ScheduleSystem?style=flat-square&label=Size"/>
</a>

</div>

---

# 🗓️ ScheduleSystem

**ScheduleSystem** is a web application for creating, managing, validating, and viewing educational schedules.

The project is built with **ASP.NET Core MVC**, **Entity Framework Core**, and **PostgreSQL**.

The main goal of the system is not only to store schedules, but also to help administrators create schedules while reducing **time conflicts, resource conflicts, and classroom assignment errors**.

---

## ✨ Features

### 📅 Schedule Management

* Create lessons
* Edit lessons
* Delete lessons
* View the complete schedule
* Sort lessons by time
* Group schedules by groups and weekdays
* View an individual group schedule
* View an individual teacher schedule
* View an individual classroom schedule

---

### 🔎 Smart Search & Filtering

The schedule can be filtered by:

* 👥 Group
* 👨‍🏫 Teacher
* 📚 Subject
* 🏫 Classroom
* 📆 Day of the week
* ⏱️ Time

This makes it easier to find specific lessons and review schedules for individual resources.

---

## 🧠 Smart Schedule Validation

One of the core features of ScheduleSystem is automatic schedule validation.

Before a lesson is created or modified, the system checks whether the selected resources can be used at the specified time.

### ⏱️ Time Conflict Detection

For example:

```text
10:00 ───────── 11:30

          11:00 ───────── 12:30
```

The system detects overlapping lessons and checks several resources:

```text
👨‍🏫 Teacher
👥 Group
🏫 Classroom
```

This helps prevent situations where the same teacher, group, or classroom is assigned to multiple lessons at the same time.

---

## 🏫 Smart Classroom Recommendations

Classroom selection is not based only on availability.

The system considers several requirements:

* Classroom capacity
* Number of students in the group
* Classroom category
* Classroom availability
* Subject requirements

For example:

```text
Classroom: 404

Capacity: 10
Students: 12

❌ Classroom capacity is insufficient.
```

The system can then identify suitable available classrooms:

```text
Available classrooms:

102 · 105 · 107

Remaining capacity:

+8 · +15 · +23
```

This allows administrators to select classrooms that satisfy the requirements of a particular lesson.

---

## 💻 Classroom Categories

Different subjects may require different types of classrooms.

For example:

```text
💻 Computer Science
        ↓
Computer classroom

🔬 Laboratory Work
        ↓
Laboratory

🚗 Practical Training
        ↓
Specialized classroom
```

Classroom categories are stored in the database, allowing new categories to be added without changing the application structure.

---

## 👨‍🏫 Teachers

The system provides teacher management functionality:

* Create teachers
* Edit teachers
* Delete teachers
* View teacher information
* Assign subjects
* View individual teacher schedules

The system can also validate whether the selected subject corresponds to the teacher's assigned subject.

---

## 👥 Groups

Each educational group can contain:

* Group name
* Specialty
* Course
* Number of students
* Description

The number of students is used during classroom validation to ensure that the selected classroom has sufficient capacity.

---

## 📚 Subjects

Subjects are stored as separate database entities.

This allows the system to maintain a centralized list of subjects and connect them with:

* Teachers
* Groups
* Schedules
* Classroom requirements

---

## 🏫 Classrooms

Each classroom contains information such as:

```text
Name
Category
Capacity
```

Classroom categories allow the system to determine whether a particular room is suitable for a subject or lesson.

---

# 🏗️ Architecture

ScheduleSystem follows the **ASP.NET Core MVC** architecture.

```text
ScheduleSystem
│
├── Controllers
│   ├── HomeController
│   ├── SchedulesController
│   ├── TeachersController
│   ├── GroupsController
│   ├── SubjectsController
│   ├── ClassroomsController
│   └── ClassroomCategoriesController
│
├── Data
│   └── ApplicationDbContext
│
├── Models
│   ├── Schedule
│   ├── Teacher
│   ├── Group
│   ├── Subject
│   ├── Classroom
│   └── ClassroomCategory
│
├── Views
│   ├── Home
│   ├── Schedules
│   ├── Teachers
│   ├── Groups
│   ├── Subjects
│   ├── Classrooms
│   └── ClassroomCategories
│
├── Migrations
│
└── wwwroot
    ├── css
    ├── js
    └── lib
```

---

# 🛠️ Tech Stack

<div align="center">

| Technology                | Purpose                   |
| ------------------------- | ------------------------- |
| **C#**                    | Main programming language |
| **ASP.NET Core MVC**      | Web framework             |
| **.NET 8**                | Application platform      |
| **Entity Framework Core** | ORM                       |
| **PostgreSQL**            | Database                  |
| **Razor**                 | Server-side views         |
| **HTML5 / CSS3**          | User interface            |
| **JavaScript**            | Client-side functionality |
| **Bootstrap**             | UI components             |
| **jQuery**                | Client-side utilities     |

</div>

---

# 🎨 UI / Design

ScheduleSystem uses a modern interface inspired by **glassmorphism** and contemporary web application design.

The interface focuses on:

```text
┌─────────────────────────────────────────┐
│                                         │
│        Clean Interface                  │
│                                         │
│        Glass Cards                      │
│                                         │
│        Soft Shadows                     │
│                                         │
│        Blue Accent Colors               │
│                                         │
│        Smooth Interactions              │
│                                         │
│        Minimal Navigation               │
│                                         │
└─────────────────────────────────────────┘
```

The goal is to make a complex scheduling system clear and comfortable to use.

---

# ⚙️ Installation

## 1. Clone the repository

```bash
git clone https://github.com/Artem56x/ScheduleSystem.git
cd ScheduleSystem
```

## 2. Restore dependencies

```bash
dotnet restore
```

## 3. Configure PostgreSQL

Create a PostgreSQL database:

```text
schedulesystem
```

Configure the application's database connection.

For local development, sensitive connection information should preferably be stored using **.NET User Secrets** instead of being committed to the repository.

## 4. Apply migrations

```bash
dotnet ef database update
```

## 5. Run the application

```bash
dotnet run
```

After the application starts, ASP.NET Core will display the local address in the terminal.

---

# 🗄️ Database

ScheduleSystem uses **PostgreSQL** with **Entity Framework Core**.

The main relationships can be represented as follows:

```text
                    ┌──────────────┐
                    │   Schedule   │
                    └──────┬───────┘
                           │
             ┌─────────────┼─────────────┐
             │             │             │
             ▼             ▼             ▼
       ┌──────────┐  ┌──────────┐  ┌──────────┐
       │ Teacher  │  │  Group   │  │ Subject  │
       └──────────┘  └──────────┘  └──────────┘
                           │
                           │
                           ▼
                    ┌──────────────┐
                    │  Classroom   │
                    └──────┬───────┘
                           │
                           ▼
                ┌────────────────────┐
                │ ClassroomCategory  │
                └────────────────────┘
```

---

# 📦 Database Migrations

Entity Framework Core migrations are used to manage database schema changes.

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

# 🔐 Authentication & Authorization

The application includes user authentication and role-based access control.

The system is designed around different access levels, including:

```text
Administrator
      │
      ├── Manage teachers
      ├── Manage groups
      ├── Manage subjects
      ├── Manage classrooms
      ├── Manage schedules
      └── Configure system settings

User
      │
      └── View schedule information
```

Administrative functionality is protected from regular users.

---

# 📤 Schedule Export

ScheduleSystem supports exporting schedule information into external formats.

Available export functionality includes:

* 📄 PDF
* 📊 Excel
* 📋 CSV

Schedules can be exported for different views, including the complete schedule and individual schedules.

---

# ⚙️ Schedule Generation

The system includes configuration options for automatic schedule generation.

Generation settings can control parameters such as:

* Default lessons per week
* Maximum lessons per day
* Teaching days per week
* Maximum gaps per day
* Whether gaps are allowed

These settings provide a foundation for generating schedules according to the requirements of an educational institution.

---

# 📊 Schedule Statistics

The application provides schedule-related statistics and overview information to help administrators monitor the current state of the system.

The system can work with statistics related to:

* Teachers
* Groups
* Subjects
* Classrooms
* Scheduled lessons

---

# 📝 Audit Logs

Administrative actions can be tracked through an audit log system.

This provides a record of important changes made within the application and helps maintain transparency when managing schedule data.

---

# 🚀 Roadmap

### ✅ Completed

* [x] ASP.NET Core MVC architecture
* [x] PostgreSQL database
* [x] Entity Framework Core
* [x] CRUD operations
* [x] Schedule management
* [x] Group schedules
* [x] Teacher schedules
* [x] Classroom schedules
* [x] Search and filtering
* [x] Teacher conflict detection
* [x] Group conflict detection
* [x] Classroom conflict detection
* [x] Classroom capacity validation
* [x] Classroom categories
* [x] Smart classroom recommendations
* [x] Authentication and authorization
* [x] Administrator functionality
* [x] User roles
* [x] Schedule export
* [x] Schedule generation settings
* [x] Schedule statistics
* [x] Audit logs
* [x] Modern responsive UI

### 🔄 In Progress

* [ ] More advanced recommendation logic
* [ ] Improved schedule optimization
* [ ] More detailed statistics
* [ ] Improved mobile experience

### 💡 Future

* [ ] Advanced automatic schedule optimization
* [ ] Extended notifications
* [ ] REST API
* [ ] Docker deployment

---

# 📈 Project Focus

```text
Backend          ████████████████████  100%

Database         ████████████████████  100%

CRUD             ████████████████████  100%

Validation       ███████████████████░   95%

UI / UX          ████████████████████  100%

Recommendations  █████████████████░░░   85%

Optimization     ████████████░░░░░░░░   60%
```

---

# 🔐 Security

Sensitive database credentials and other private configuration values should **not** be stored directly in Git.

For local development, use:

```bash
dotnet user-secrets
```

Sensitive files and local configuration should remain excluded through `.gitignore`.

---

# 📌 Project Status

```text
Status:        🟢 Active Development
Version:       1.0
Platform:      Web
Framework:     ASP.NET Core
Runtime:       .NET 8
Database:      PostgreSQL
Architecture:  MVC
Language:      C#
```

---

<div align="center">

## 💙 Built with C# & ASP.NET Core

<br>

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0A84FF,50:5E5CE6,100:AF52DE&height=120&section=footer"/>

### 👨‍💻 Artem56x

<a href="https://github.com/Artem56x">
<img src="https://img.shields.io/badge/GitHub-Artem56x-181717?style=for-the-badge&logo=github"/>
</a>

<br><br>

⭐ If you like the project, consider giving it a star.

</div>
