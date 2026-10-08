# ScheduleSystem — Technical Documentation

## 1. Overview

ScheduleSystem is a web-based college timetable management system developed with ASP.NET Core MVC on .NET 8.

The system is designed to manage academic scheduling data and provide tools for creating, validating, generating, filtering, and exporting class schedules.

The application uses PostgreSQL as the database and Entity Framework Core as the data access layer.

## 2. Main Objectives

The system provides:

* management of teachers;
* management of student groups;
* management of subjects;
* management of classrooms;
* creation and editing of schedules;
* automatic schedule generation;
* schedule conflict validation;
* classroom recommendations;
* classroom category requirements;
* teacher-subject relationships;
* group-subject relationships;
* authentication and role-based authorization;
* user notifications;
* audit logging;
* schedule export to PDF, Excel, and CSV;
* configurable schedule generation parameters.

## 3. Technology Stack

| Technology            | Purpose                            |
| --------------------- | ---------------------------------- |
| C#                    | Main programming language          |
| .NET 8                | Application platform               |
| ASP.NET Core MVC      | Web application framework          |
| Entity Framework Core | ORM and database access            |
| PostgreSQL            | Relational database                |
| Npgsql                | PostgreSQL provider for EF Core    |
| ASP.NET Core Identity | Authentication and authorization   |
| Razor Views           | Server-side UI rendering           |
| HTML5                 | Page structure                     |
| CSS3                  | Interface styling                  |
| JavaScript            | Client-side interactions           |
| Git / GitHub          | Version control and source hosting |

## 4. Architecture

ScheduleSystem follows an ASP.NET Core MVC architecture with a separate service layer.

```text
User Browser
    |
    v
ASP.NET Core MVC
    |
    +-- Controllers
    |      |
    |      +-- ViewModels
    |      +-- Models
    |
    +-- Services
    |      +-- ScheduleGeneratorService
    |      +-- ScheduleValidationService
    |      +-- ClassroomRecommendationService
    |      +-- ScheduleExportService
    |
    v
Entity Framework Core
    |
    v
PostgreSQL
```

### Controllers

Controllers handle HTTP requests, application flow, validation, and interaction with services and the database.

The project contains controllers for:

* account and authentication;
* home/dashboard;
* schedules;
* teachers;
* groups;
* subjects;
* classrooms;
* schedule generation;
* notifications;
* audit logs;
* analytics.

### Services

Business logic is separated from controllers where appropriate.

Important services include:

* `ScheduleGeneratorService` — automatic schedule generation;
* `ScheduleValidationService` — schedule conflict and rule validation;
* `ClassroomRecommendationService` — classroom selection and recommendations;
* `ScheduleExportService` — export of schedules to supported formats.

## 5. Project Structure

```text
ScheduleSystem/
├── Controllers/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── IdentitySeeder.cs
├── Migrations/
├── Models/
├── Services/
├── ViewComponents/
├── Views/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── ...
├── Program.cs
├── appsettings.json
├── README.md
└── ScheduleSystem.csproj
```

## 6. Data Model

The main domain entities are:

* `Teacher`
* `TeacherSubject`
* `Subject`
* `Group`
* `GroupSubject`
* `Classroom`
* `ClassroomCategory`
* `SubjectClassroomCategory`
* `Schedule`
* `ScheduleTimeSlot`
* `ScheduleGenerationSettings`
* `Notification`
* `AuditLog`
* `ApplicationUser`

### Teacher and Subject

A teacher can teach multiple subjects, and a subject can be taught by multiple teachers.

This relationship is implemented through the junction entity `TeacherSubject`.

```text
Teacher
   |
   | 1..*
   v
TeacherSubject
   ^
   | *..1
   |
Subject
```

The junction table uses a composite primary key:

```text
(TeacherId, SubjectId)
```

### Group and Subject

Groups and subjects are connected through `GroupSubject`.

This allows the system to determine which subjects belong to a particular student group.

### Classroom Requirements

Subjects can define required classroom categories through `SubjectClassroomCategory`.

Classroom recommendations also consider the group's student count.

## 7. Database

The application uses PostgreSQL with Entity Framework Core.

The PostgreSQL provider is Npgsql.

The application's `ApplicationDbContext` inherits from:

```csharp
IdentityDbContext<ApplicationUser>
```

This allows application data and ASP.NET Core Identity data to use the same EF Core context.

The project contains migrations for the evolution of the database schema, including Identity, notifications, audit logs, schedule time slots, generation settings, classroom categories, subject requirements, and the teacher-subject relationship.

## 8. Schedule Management

The schedule module supports:

* creating schedules;
* editing schedules;
* deleting schedules;
* filtering schedules;
* viewing schedules by group;
* viewing schedules by teacher;
* viewing schedules by classroom;
* viewing schedules by subject;
* filtering by day and time;
* validation of schedule constraints.

A schedule is associated with the relevant group, teacher, subject, and classroom.

## 9. Schedule Validation

Schedule validation prevents invalid timetable configurations.

The validation layer checks, among other rules:

### Time validity

The lesson start time must be earlier than the lesson end time.

### Overlapping lessons

The system detects conflicting schedule intervals for the same relevant resource.

### Classroom capacity

A classroom must have sufficient capacity for the selected student group.

```text
Classroom.Capacity >= Group.StudentCount
```

### Classroom category

If a subject requires a particular classroom category, the selected classroom must satisfy that requirement.

### Course compatibility

Subjects and groups are checked for compatible course information where applicable.

## 10. Automatic Schedule Generation

The system contains an automatic schedule generation service.

The generation process uses configured parameters and available academic data to create timetable entries while applying scheduling constraints.

Default generation settings include:

| Parameter               | Default |
| ----------------------- | ------: |
| Lessons per week        |       3 |
| Maximum lessons per day |       4 |
| Teaching days per week  |       5 |
| Maximum gaps per day    |       2 |
| Allow gaps              |     Yes |

The generator works together with schedule validation and classroom recommendation logic to reduce invalid combinations.

## 11. Classroom Recommendations

The classroom recommendation service selects suitable classrooms based on available data.

The main constraints include:

1. classroom category requirements;
2. classroom capacity;
3. group size;
4. availability and schedule conflicts.

This allows the system to recommend classrooms instead of requiring the administrator to search through every classroom manually.

## 12. Teachers

The teacher module supports CRUD operations and subject assignment.

A teacher may be associated with multiple subjects.

When creating or editing a schedule, the application can check whether the selected teacher is associated with the selected subject.

The interface contains an explicit confirmation mechanism for cases where an administrator intentionally wants to continue despite a teacher-subject mismatch.

## 13. Groups

Groups contain academic information such as:

* group name;
* course;
* student count.

The student count is also used during classroom recommendation and schedule validation.

## 14. Subjects

Subjects contain information such as:

* name;
* course;
* subject type;
* classroom category requirements.

Subjects can be associated with multiple teachers and groups through the corresponding relationship entities.

## 15. Classrooms

Classrooms contain:

* classroom name;
* classroom category;
* capacity.

The application uses classroom categories instead of a separate computer-equipment flag.

This allows classroom requirements to be represented through the category system.

## 16. Authentication and Authorization

The project uses ASP.NET Core Identity.

The application supports role-based access, including:

* `Admin`;
* `User`.

Authentication-related functionality includes:

* registration;
* login;
* logout;
* password management;
* profile information;
* account settings.

Administrative functionality is protected by authorization rules.

The administrator password is not stored as a hardcoded value in the source code. Initial administrator provisioning uses configuration or the `Admin__Password` environment variable.

## 17. Notifications

The application contains a notification system for users.

Notifications can contain schedule-related information such as:

* subject;
* group;
* lesson time;
* schedule changes or relevant events.

The interface provides a notification area through the notification bell component.

## 18. Audit Logs

Important application actions can be recorded in the audit log system.

Audit logs provide a trace of administrative or system activity and can be viewed through the corresponding administrative interface.

## 19. Schedule Export

Schedules can be exported to:

* PDF;
* Excel;
* CSV.

The export functionality is implemented through `ScheduleExportService`.

The application supports overall schedule exports and schedule exports for specific academic entities where supported by the interface.

## 20. User Interface

The interface uses a modern white and blue visual style inspired by contemporary glass-based UI patterns.

The application includes dedicated styling for major sections and reusable visual components.

The main navigation includes:

* Home;
* Schedule;
* Teachers;
* Groups;
* Subjects;
* Classrooms.

The interface is implemented with Razor views, HTML, CSS, and JavaScript.

## 21. Configuration

Application configuration is handled through ASP.NET Core configuration mechanisms.

Sensitive administrator credentials must be supplied through secure configuration, for example:

```text
Admin__Password
```

Sensitive values should not be committed to source control.

## 22. Database Migrations

Database schema changes are managed through Entity Framework Core migrations.

Create a migration:

```powershell
dotnet ef migrations add MigrationName
```

Apply migrations:

```powershell
dotnet ef database update
```

Check for pending model changes:

```powershell
dotnet ef migrations has-pending-model-changes
```

## 23. Build and Verification

The project should be verified before deployment or demonstration.

Recommended commands:

```powershell
dotnet restore
dotnet build
dotnet ef database update
```

The final project audit confirmed:

* successful project build;
* 0 build errors;
* 0 build warnings;
* no pending EF Core model changes;
* implemented `TeacherSubject` relationship;
* no active dependency on the previous single `Teacher.Subject` relationship;
* removal of the hardcoded administrator password;
* schedule generation and validation functionality;
* export functionality.

## 24. Security Considerations

The application uses:

* ASP.NET Core Identity;
* role-based authorization;
* secure configuration for administrator credentials;
* EF Core database access;
* server-side validation;
* controlled access to administrative functionality.

Production deployments should additionally use HTTPS, secure PostgreSQL credentials, protected configuration storage, and regular database backups.

## 25. Development Workflow

The project uses Git for version control.

The main branch is:

```text
main
```

The source repository is hosted on GitHub:

https://github.com/Artem56x/ScheduleSystem

## 26. Project Status

The main implementation is complete.

Implemented areas include:

* core MVC architecture;
* PostgreSQL database;
* Entity Framework Core;
* CRUD modules;
* teacher-subject many-to-many relationship;
* group-subject relationships;
* schedule management;
* schedule validation;
* automatic schedule generation;
* classroom recommendations;
* authentication and authorization;
* notifications;
* audit logs;
* export;
* modernized UI;
* secure administrator password configuration.

## 27. Future Improvements

Possible future improvements include:

* automated integration and UI tests;
* deployment to a production server;
* additional reporting and analytics;
* more advanced schedule generation algorithms;
* granular permissions beyond the current roles;
* localization for additional languages.

These improvements are not required for the current project functionality.

## 28. Conclusion

ScheduleSystem provides a complete web-based solution for managing college schedules.

The project combines ASP.NET Core MVC, Entity Framework Core, PostgreSQL, ASP.NET Core Identity, business services, schedule validation, automatic generation, classroom recommendations, notifications, audit logging, and export functionality in a single application.

The architecture separates presentation, application logic, and data access, making the project suitable for further development and maintenance.
