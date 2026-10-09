# ScheduleSystem — Architecture Documentation

## 1. Overview

ScheduleSystem is an ASP.NET Core MVC web application built with .NET 8.

The application follows a layered architecture based on the Model-View-Controller (MVC) pattern.

The main architectural components are:

* Models;
* Controllers;
* Services;
* ViewModels;
* Razor Views;
* Entity Framework Core;
* PostgreSQL;
* ASP.NET Core Identity.

The architecture separates presentation, application logic and data access.

---

## 2. High-Level Architecture

The general application architecture can be represented as:

```text
                         User
                           │
                           ▼
                    ┌─────────────┐
                    │ Razor Views │
                    │    (.cshtml)│
                    └──────┬──────┘
                           │
                           ▼
                    ┌─────────────┐
                    │ Controllers │
                    └──────┬──────┘
                           │
              ┌────────────┴────────────┐
              │                         │
              ▼                         ▼
       ┌──────────────┐        ┌────────────────┐
       │   Services   │        │   ViewModels   │
       └──────┬───────┘        └────────────────┘
              │
              ▼
       ┌──────────────────┐
       │ Entity Framework │
       │       Core       │
       └────────┬─────────┘
                │
                ▼
       ┌──────────────────┐
       │    PostgreSQL    │
       └──────────────────┘
```

ASP.NET Core Identity is integrated into the application and uses the same Entity Framework Core database context.

---

## 3. MVC Pattern

The application uses the Model-View-Controller pattern.

### Model

Models represent application data and domain entities.

Examples include:

* `Teacher`;
* `Subject`;
* `Group`;
* `Classroom`;
* `Schedule`;
* `TeacherSubject`;
* `Notification`;
* `AuditLog`.

Models are mapped to database tables through Entity Framework Core.

---

### View

Views are Razor `.cshtml` files.

They are responsible for displaying information and collecting user input.

Examples include:

```text
Views/Teachers/
Views/Subjects/
Views/Groups/
Views/Classrooms/
Views/Schedules/
Views/Account/
Views/Home/
```

The views do not contain the main business logic of the application.

---

### Controller

Controllers process HTTP requests and coordinate application operations.

Examples include:

```text
TeachersController
SubjectsController
GroupsController
ClassroomsController
SchedulesController
ScheduleGeneratorController
NotificationsController
AccountController
AnalyticsController
AuditLogsController
HomeController
```

Controllers receive user requests, validate input, call services or the database layer and return the appropriate view or response.

---

## 4. Layered Architecture

ScheduleSystem can be logically divided into several layers.

```text
┌──────────────────────────────────────┐
│          Presentation Layer          │
│                                      │
│        Razor Views / ViewModels      │
└──────────────────┬───────────────────┘
                   │
┌──────────────────▼───────────────────┐
│          Application Layer            │
│                                      │
│ Controllers / Services / Validation  │
└──────────────────┬───────────────────┘
                   │
┌──────────────────▼───────────────────┐
│             Data Layer                │
│                                      │
│ ApplicationDbContext / EF Core       │
└──────────────────┬───────────────────┘
                   │
┌──────────────────▼───────────────────┐
│             Database                  │
│                                      │
│              PostgreSQL               │
└──────────────────────────────────────┘
```

This separation makes the application easier to maintain and extend.

---

## 5. Project Structure

The main project structure is organized approximately as follows:

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
│   ├── ScheduleTimeSlot.cs
│   ├── Subject.cs
│   ├── Teacher.cs
│   ├── TeacherSubject.cs
│   └── ...
│
├── Services/
│   ├── ClassroomRecommendationService.cs
│   ├── ScheduleExportService.cs
│   ├── ScheduleGeneratorService.cs
│   └── ScheduleValidationService.cs
│
├── ViewComponents/
│   └── NotificationBell/
│
├── ViewModels/
│   └── ...
│
├── Views/
│   ├── Account/
│   ├── Analytics/
│   ├── AuditLogs/
│   ├── Classrooms/
│   ├── Groups/
│   ├── Home/
│   ├── Notifications/
│   ├── ScheduleGenerator/
│   ├── Schedules/
│   ├── Subjects/
│   └── Teachers/
│
├── Migrations/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── ...
│
├── Program.cs
├── appsettings.json
└── ScheduleSystem.csproj
```

---

## 6. Controllers

Controllers are responsible for handling incoming requests.

A typical request flow is:

```text
HTTP Request
     ↓
Controller
     ↓
Validation
     ↓
Service / DbContext
     ↓
ViewModel
     ↓
Razor View
```

Controllers should coordinate operations rather than contain large amounts of complex business logic.

---

## 7. Services

The application uses dedicated services for complex operations.

Important services include:

### ScheduleValidationService

Responsible for checking schedule consistency and detecting conflicts.

It can validate:

* teacher conflicts;
* group conflicts;
* classroom conflicts;
* time ranges;
* classroom capacity;
* classroom category requirements.

---

### ScheduleGeneratorService

Responsible for automatic schedule generation.

It uses:

* groups;
* subjects;
* teachers;
* classrooms;
* schedule restrictions;
* generation settings.

---

### ClassroomRecommendationService

Responsible for finding suitable classrooms.

The service considers:

* classroom category;
* group size;
* classroom capacity;
* classroom availability;
* subject requirements.

---

### ScheduleExportService

Responsible for converting schedule data into export formats.

Supported formats include:

* PDF;
* Excel;
* CSV.

---

## 8. Why Services Are Used

Complex operations are separated into services instead of being implemented directly inside controllers.

For example, automatic schedule generation is a complex process.

Without a dedicated service, the controller would contain:

```text
Request handling
+
Validation
+
Generation algorithm
+
Database operations
+
Error handling
```

Using a service separates these responsibilities:

```text
Controller
    ↓
ScheduleGeneratorService
    ↓
ApplicationDbContext
```

This makes the code easier to test and maintain.

---

## 9. Models

Models represent persistent domain entities.

Examples:

```text
Teacher
Subject
Group
Classroom
ClassroomCategory
Schedule
ScheduleTimeSlot
ScheduleGenerationSettings
Notification
AuditLog
```

Models can contain:

* properties;
* navigation properties;
* relationships;
* validation-related attributes where appropriate.

---

## 10. Teacher–Subject Architecture

Teachers and subjects use a many-to-many relationship.

```text
┌──────────────┐
│    Teacher   │
└──────┬───────┘
       │
       │ 1:N
       ▼
┌────────────────┐
│ TeacherSubject │
└───────┬────────┘
        │
        │ N:1
        ▼
┌──────────────┐
│   Subject    │
└──────────────┘
```

This allows:

```text
One Teacher → Multiple Subjects
One Subject → Multiple Teachers
```

The relationship is represented by `TeacherSubject`.

The composite key is:

```text
TeacherId + SubjectId
```

---

## 11. ViewModels

ViewModels are used when the data required by a page does not directly correspond to one database entity.

They allow the application to prepare exactly the data required by a view.

For example, teacher creation can require:

```text
Teacher information
+
Available subjects
+
Selected subjects
```

Instead of exposing database entities directly, a dedicated ViewModel can represent the form.

This improves separation between the database model and the presentation layer.

---

## 12. Data Access

Entity Framework Core is responsible for data access.

The application uses:

```text
ApplicationDbContext
```

which inherits from:

```text
IdentityDbContext<ApplicationUser>
```

The context provides access to application entities and Identity entities.

---

## 13. PostgreSQL

PostgreSQL is the persistent database system.

The application communicates with PostgreSQL through:

```text
Entity Framework Core
        +
Npgsql
```

The general flow is:

```text
C# Entity
   ↓
EF Core
   ↓
Npgsql
   ↓
PostgreSQL
```

---

## 14. Dependency Injection

ASP.NET Core dependency injection is used to provide application services and framework components.

Services can be registered in the application's startup configuration and injected where required.

For example:

```text
Controller
    ↓
Injected Service
    ↓
Service Logic
```

This reduces direct coupling between components.

---

## 15. Authentication

Authentication is implemented using ASP.NET Core Identity.

The authentication architecture is:

```text
User
 ↓
Login
 ↓
ASP.NET Core Identity
 ↓
Authentication
 ↓
Authorization
 ↓
Controller / Resource
```

Identity manages:

* users;
* passwords;
* roles;
* authentication state;
* claims;
* account-related information.

---

## 16. Authorization

Authorization determines which operations a user is allowed to perform.

The application supports role-based access control.

Main roles:

```text
User
Admin
```

Administrative controllers and actions can be protected using authorization rules.

---

## 17. Schedule Architecture

The scheduling subsystem is composed of several components:

```text
SchedulesController
       │
       ├───────────────┐
       │               │
       ▼               ▼
ScheduleValidation   ScheduleGenerator
Service              Service
       │               │
       └───────┬───────┘
               │
               ▼
       ApplicationDbContext
               │
               ▼
           PostgreSQL
```

This structure keeps scheduling logic separate from HTTP request handling.

---

## 18. Schedule Generation Flow

The automatic generation process can be represented as:

```text
Start Generation
       ↓
Load Groups
       ↓
Load Subjects
       ↓
Load Teachers
       ↓
Load Classrooms
       ↓
Load Generation Settings
       ↓
Build Possible Assignments
       ↓
Check Constraints
       ↓
Assign Time Slots
       ↓
Validate Generated Schedule
       ↓
Save Results
```

The generator attempts to satisfy the configured constraints.

---

## 19. Schedule Validation Flow

Validation follows a similar architecture:

```text
Schedule Input
      ↓
ScheduleValidationService
      ↓
Check Time
      ↓
Check Teacher
      ↓
Check Group
      ↓
Check Classroom
      ↓
Check Capacity
      ↓
Check Classroom Category
      ↓
Validation Result
```

This allows schedule-related rules to be centralized.

---

## 20. Classroom Recommendation Flow

The classroom recommendation process is:

```text
Subject
   +
Group
   ↓
Read Subject Requirements
   ↓
Read Group Size
   ↓
Find Classrooms
   ↓
Filter by Category
   ↓
Filter by Capacity
   ↓
Check Availability
   ↓
Return Recommendations
```

The recommendation service is used to reduce manual classroom selection.

---

## 21. Notification Architecture

Notifications are stored in the database and displayed through the application's notification interface.

The architecture is approximately:

```text
Schedule/Event
      ↓
Notification Creation
      ↓
PostgreSQL
      ↓
NotificationBell / Notifications Page
      ↓
User
```

The notification bell is implemented using a ViewComponent.

---

## 22. Audit Architecture

Important actions can be recorded in the audit log.

The general flow is:

```text
Administrative Action
        ↓
Audit Logging
        ↓
AuditLog Entity
        ↓
PostgreSQL
        ↓
Audit Logs Interface
```

Audit information can be used to investigate changes and administrative activity.

---

## 23. Export Architecture

Schedule export is separated from the main schedule controller logic.

The general flow is:

```text
Schedule Request
      ↓
SchedulesController
      ↓
ScheduleExportService
      ↓
Format Generation
      ↓
PDF / Excel / CSV
      ↓
File Download
```

This allows export functionality to be maintained independently from the schedule management interface.

---

## 24. Razor Views

The presentation layer uses Razor Views.

Views are organized by feature:

```text
Views/
├── Account/
├── Analytics/
├── AuditLogs/
├── Classrooms/
├── Groups/
├── Home/
├── Notifications/
├── ScheduleGenerator/
├── Schedules/
├── Subjects/
└── Teachers/
```

Each feature has its own UI components and forms.

---

## 25. Frontend Architecture

The frontend is based on:

* Razor;
* HTML5;
* CSS3;
* JavaScript.

The interface uses solid white surfaces, blue accents, shared controls and responsive page layouts. The existing site.css, liquid-glass.css and page styles retain their paths; presentation responsibilities and validation limits are documented in TechnicalDocumentation.md, section 29.

Styles are separated into dedicated CSS files where appropriate.

Examples include:

```text
Profile.css
Settings.css
ProgramSettings.css
StudyLoad.css
liquid-glass.css
```

The frontend is responsible for presentation and client-side interactions while server-side validation remains authoritative.

---

## 26. Request Lifecycle

A typical request follows this process:

```text
Browser
  │
  ▼
ASP.NET Core Middleware
  │
  ▼
Routing
  │
  ▼
Controller
  │
  ├── ViewModel
  │
  ├── Service
  │
  └── ApplicationDbContext
           │
           ▼
       PostgreSQL
           │
           ▼
       Controller
           │
           ▼
       Razor View
           │
           ▼
        Browser
```

This is the standard server-side MVC request lifecycle used by the application.

---

## 27. Separation of Responsibilities

The project follows a separation of responsibilities.

### Controllers

Handle requests and coordinate operations.

### Services

Contain reusable application and business logic.

### Models

Represent domain and database entities.

### ViewModels

Represent data required by presentation forms and pages.

### Views

Render the user interface.

### DbContext

Provides access to persistent data.

### PostgreSQL

Stores persistent information.

---

## 28. Configuration

Application configuration is handled through the ASP.NET Core configuration system.

Configuration can include:

* database connection;
* administrator password;
* environment-specific settings;
* application behavior.

Sensitive configuration values should not be committed to source control.

---

## 29. Security Architecture

The security model contains several layers:

```text
Authentication
      ↓
Authorization
      ↓
Server-side Validation
      ↓
Entity Framework / Database Constraints
      ↓
PostgreSQL
```

The application also avoids storing the administrator password directly in source code.

The administrator password is supplied through configuration/environment variables.

---

## 30. Error Handling

The application validates input and schedule-related operations before saving data.

Examples of validation include:

* invalid time ranges;
* schedule conflicts;
* incompatible classrooms;
* insufficient classroom capacity;
* invalid teacher-subject assignments.

Validation results are returned to the user through the application interface.

---

## 31. Database Migrations

Entity Framework Core migrations provide controlled database schema evolution.

The migration flow is:

```text
Model Change
    ↓
Migration Creation
    ↓
Migration File
    ↓
Database Update
    ↓
PostgreSQL Schema
```

Typical commands:

```powershell
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

## 32. Build and Verification

Before a release, the application should be verified using:

```powershell
dotnet build
```

The Entity Framework model can be checked using:

```powershell
dotnet ef migrations has-pending-model-changes
```

The project should build successfully without unexpected model changes.

---

## 33. Git Workflow

The project uses Git for version control.

The general workflow is:

```text
Modify Code
    ↓
Test
    ↓
dotnet build
    ↓
git status
    ↓
git add
    ↓
git commit
    ↓
git push
```

The remote repository is hosted on GitHub.

---

## 34. Maintainability

The architecture is designed to make future changes easier.

For example:

### New export format

Can be implemented within the export layer without redesigning the entire scheduling system.

### New validation rule

Can be added to the schedule validation service.

### New recommendation rule

Can be added to the classroom recommendation service.

### New UI page

Can be implemented through a controller, ViewModel and Razor View.

### New database entity

Can be added through a model, `DbSet`, Entity Framework configuration and migration.

---

## 35. Scalability Considerations

The current architecture is suitable for a college-level scheduling application.

If the system is expanded to support significantly larger organizations, possible improvements include:

* database indexing optimization;
* caching;
* asynchronous background schedule generation;
* distributed processing;
* stronger logging infrastructure;
* API separation;
* automated testing;
* pagination for large datasets.

These improvements are not required for the current project scope.

---

## 36. Current Architecture Summary

The current architecture can be summarized as:

```text
                    ┌─────────────────────┐
                    │       Browser       │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │    ASP.NET Core     │
                    │        MVC          │
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
              ▼                ▼                ▼
        Controllers        ViewModels         Views
              │
              ▼
          Services
              │
              ▼
     ApplicationDbContext
              │
              ▼
         Entity Framework
              │
              ▼
          PostgreSQL
```

---

## 37. Architectural Principles

ScheduleSystem follows several important principles:

1. Separation of concerns.
2. MVC-based presentation architecture.
3. Dedicated services for complex business operations.
4. Entity Framework Core for data access.
5. PostgreSQL for persistent storage.
6. Dependency injection.
7. Role-based authorization.
8. Migration-based database management.
9. ViewModels for presentation-specific data.
10. Centralized schedule validation.
11. Centralized schedule generation.
12. Separation of export functionality.

---

## 38. Conclusion

ScheduleSystem uses a structured ASP.NET Core MVC architecture that separates the user interface, request handling, business logic and persistent storage.

The architecture provides dedicated components for the most complex parts of the system:

* schedule validation;
* automatic schedule generation;
* classroom recommendations;
* schedule export;
* authentication;
* notifications;
* audit logging.

This structure makes the project easier to understand, maintain and extend while keeping the current implementation appropriate for a college timetable management system.
