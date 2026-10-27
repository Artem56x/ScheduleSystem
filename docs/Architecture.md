# ScheduleSystem — Architecture Documentation

## 1. Overview

ScheduleSystem is a web application built with ASP.NET Core MVC and .NET 8 for managing educational schedules.

The application provides functionality for managing teachers, subjects, student groups, classrooms and lessons. It also includes automatic schedule generation, schedule validation, classroom recommendations, authentication and schedule export.

The application uses a layered structure based on the Model-View-Controller (MVC) pattern.

The main architectural components are:

- ASP.NET Core MVC;
- Controllers and Razor Views;
- Models and ViewModels;
- Application services;
- Entity Framework Core;
- PostgreSQL;
- ASP.NET Core Identity;
- Dependency injection.

The architecture separates HTTP request handling, application logic, presentation and persistent data access.

---

## 2. High-Level Architecture

The application follows a server-side MVC architecture.

```text
                       Browser
                          |
                          | HTTP Request
                          v
                ASP.NET Core Middleware
                          |
                          v
                       Routing
                          |
                          v
                     Controllers
                          |
               +----------+----------+
               |                     |
               v                     v
            Services          ApplicationDbContext
               |                     |
               |                     v
               |              Entity Framework Core
               |                     |
               +---------------------+
                                     |
                                     v
                                 PostgreSQL

                     Controller Result
                            |
                            v
                       Razor Views
                            |
                            v
                         Browser
```

Controllers coordinate requests and responses. Services implement reusable application operations, while Entity Framework Core provides access to the PostgreSQL database.

ASP.NET Core Identity is integrated with the application's Entity Framework Core context.

---

## 3. MVC Pattern

ScheduleSystem uses the Model-View-Controller pattern.

### 3.1. Models

Models represent application entities and their relationships.

Examples include:

- `Teacher`;
- `Subject`;
- `Group`;
- `Classroom`;
- `ClassroomCategory`;
- `Schedule`;
- `TeacherSubject`;
- `GroupSubject`;
- `Notification`;
- `AuditLog`.

Models are mapped to database tables through Entity Framework Core.

Some supporting models represent schedule-generation settings, time slots and other application-specific data.

### 3.2. Views

Views are Razor `.cshtml` files responsible for rendering pages and collecting user input.

Feature-related views are organized into directories such as:

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

The actual directories depend on the current project files.

Views are responsible for presentation. Server-side controllers and services remain responsible for processing requests and enforcing application rules.

### 3.3. Controllers

Controllers receive HTTP requests, coordinate application operations and return views, redirects or other responses.

Examples include:

```text
AccountController
AnalyticsController
AuditLogsController
ClassroomsController
GroupsController
GroupSubjectsController
HomeController
NotificationsController
ScheduleGeneratorController
SchedulesController
SubjectsController
TeachersController
```

Controllers can interact with `ApplicationDbContext` directly or use registered application services, depending on the operation.

The schedule-generation workflow is handled separately by `ScheduleGeneratorController`.

---

## 4. Layered Architecture

The application can be logically divided into the following layers.

```text
+--------------------------------------+
|          Presentation Layer          |
|          Razor Views / UI            |
+-------------------+------------------+
                    |
                    v
+--------------------------------------+
|          Request Handling            |
|             Controllers              |
+-------------------+------------------+
                    |
                    v
+--------------------------------------+
|          Application Logic           |
|               Services               |
+-------------------+------------------+
                    |
                    v
+--------------------------------------+
|             Data Access               |
|       ApplicationDbContext / EF Core |
+-------------------+------------------+
                    |
                    v
+--------------------------------------+
|               Database                |
|              PostgreSQL               |
+--------------------------------------+
```

This is a logical representation rather than a strict physical separation into independent projects. Controllers may use the database context directly where the current implementation requires it.

---

## 5. Project Structure

The following tree illustrates the main areas of the project. It is representative rather than an exhaustive inventory of every source file.

```text
ScheduleSystem/
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
│   └── Generator/
│       └── Schedule-generation-related models
│
├── Services/
│   ├── ClassroomRecommendationService.cs
│   ├── ScheduleExportService.cs
│   ├── ScheduleGeneratorService.cs
│   ├── ScheduleValidationService.cs
│   └── TeacherRecommendationService.cs
│
├── ViewComponents/
│   └── NotificationBell/
│
├── ViewModels/
│   └── Presentation-specific models
│
├── Views/
│   └── Feature-specific Razor Views
│
├── Migrations/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── ...
│
├── Program.cs
├── appsettings.json
└── ScheduleSystem.csproj
```

Some model types are located in subdirectories and namespaces. The actual source tree is authoritative when checking individual file locations.

---

## 6. Controller Responsibilities

Controllers handle incoming requests and coordinate the work required to produce a response.

A typical MVC request follows this sequence:

```text
HTTP Request
     |
     v
Controller Action
     |
     v
Input and Model Validation
     |
     v
Service and/or DbContext
     |
     v
Result Processing
     |
     v
Razor View, Redirect or File Response
```

Controllers may perform query construction and straightforward data operations. More complex reusable operations are delegated to services.

The implementation does not require every controller to follow an identical internal sequence.

---

## 7. Application Services

Application services encapsulate reusable operations and domain-related processing.

The application registers the following services through dependency injection:

- `ScheduleExportService`;
- `ScheduleValidationService`;
- `ScheduleGeneratorService`;
- `ClassroomRecommendationService`;
- `TeacherRecommendationService`.

### 7.1. ScheduleValidationService

`ScheduleValidationService` checks schedule data and reports validation errors or warnings.

The implementation includes checks related to:

- valid days and time ranges;
- the existence of referenced teachers, groups, subjects and classrooms;
- compatibility between a group's course and a subject's course;
- teacher-subject assignments;
- classroom capacity;
- scheduling conflicts and classroom requirements.

The exact validation result depends on the submitted schedule and the relevant validation conditions.

### 7.2. ScheduleGeneratorService

`ScheduleGeneratorService` performs automatic schedule generation.

It receives a `ScheduleGenerationRequest`, validates the request and processes the selected groups, days, time slots and generation settings.

The service evaluates possible assignments against scheduling constraints and returns a generation result.

The generation process is separate from the HTTP request-handling logic in `ScheduleGeneratorController`.

### 7.3. ClassroomRecommendationService

`ClassroomRecommendationService` supports classroom selection by evaluating classroom suitability.

Relevant factors include classroom capacity, category requirements, availability and scheduling conflicts.

Recommendations assist the user in selecting a suitable classroom. They should not be interpreted as a guarantee that every possible scheduling constraint has been satisfied.

### 7.4. TeacherRecommendationService

`TeacherRecommendationService` provides teacher-related recommendation functionality used by the schedule-management workflow.

Its exact selection rules are defined by the service implementation.

### 7.5. ScheduleExportService

`ScheduleExportService` generates downloadable schedule documents.

The implementation uses:

- ClosedXML for Excel workbook generation;
- QuestPDF for PDF document generation.

Supported export options should be determined from the actual controller actions and public methods. An export format should not be considered available merely because it is mentioned in documentation.

---

## 8. Why Services Are Used

Services help separate complex or reusable operations from HTTP request handling.

For example, schedule generation involves input processing, loading related data, evaluating possible assignments and applying scheduling constraints.

Without a dedicated service, the controller would need to coordinate all these details directly.

The current separation can be represented as:

```text
ScheduleGeneratorController
             |
             v
ScheduleGeneratorService
             |
             v
    ApplicationDbContext
             |
             v
         PostgreSQL
```

This structure improves readability and creates a clearer place for future changes to generation rules.

It does not, by itself, guarantee automated test coverage or complete separation of all business logic.

---

## 9. Domain Models

Domain models represent the entities and relationships used by the application.

Important entities include:

| Model | Purpose |
|---|---|
| `Teacher` | Stores teacher information |
| `Subject` | Represents an academic subject |
| `Group` | Represents a student group |
| `Classroom` | Represents a classroom |
| `ClassroomCategory` | Classifies classrooms |
| `Schedule` | Represents a scheduled lesson |
| `TeacherSubject` | Links teachers and subjects |
| `GroupSubject` | Represents a group-subject relationship |
| `Notification` | Represents a user notification |
| `AuditLog` | Represents an audit-log entry |
| `ScheduleTimeSlot` | Represents a reusable schedule time slot |
| `ScheduleGenerationSettings` | Stores schedule-generation settings |

Models may contain properties, navigation properties, validation attributes and relationship configuration.

The database schema is defined through Entity Framework Core mappings and migrations.

---

## 10. Teacher–Subject Relationship

Teachers and subjects are connected through the `TeacherSubject` entity.

The relationship is many-to-many:

```text
+-------------+       +----------------+       +-------------+
|   Teacher   | 1   N | TeacherSubject | N   1 |   Subject   |
+-------------+-------+----------------+-------+-------------+
```

This relationship allows:

- one teacher to be associated with multiple subjects;
- one subject to be associated with multiple teachers.

`ApplicationDbContext` configures a composite key consisting of:

```text
TeacherId + SubjectId
```

The relationship uses foreign keys to connect the join entity to teachers and subjects.

---

## 11. ViewModels

ViewModels represent data prepared specifically for a page or form.

They are useful when the information required by a view does not correspond to a single database entity.

For example, a form may require:

```text
Entity Information
        +
Available Options
        +
Selected Values
```

A ViewModel can combine these values into a presentation-specific structure.

ViewModels help keep page-specific data separate from persistent domain entities. Their use depends on the requirements of each feature.

---

## 12. Data Access

The application uses Entity Framework Core for database access.

The main context is:

```csharp
ApplicationDbContext
```

It inherits from:

```csharp
IdentityDbContext<ApplicationUser>
```

This enables the same context to manage application entities and ASP.NET Core Identity data.

The context exposes `DbSet` properties for entities including teachers, groups, subjects, classrooms, schedules, notifications and audit logs.

It also configures relationships, keys and delete behaviors through `OnModelCreating`.

---

## 13. PostgreSQL Integration

PostgreSQL is the application's persistent relational database.

The application uses the Npgsql Entity Framework Core provider.

The data-access flow is:

```text
C# Models
    |
    v
Entity Framework Core
    |
    v
Npgsql Provider
    |
    v
PostgreSQL
```

Application configuration supplies the database connection string through the ASP.NET Core configuration system.

The actual connection string and credentials depend on the deployment environment.

---

## 14. Dependency Injection

The application uses the built-in ASP.NET Core dependency injection container.

Services and framework components are registered during application startup in `Program.cs`.

For example:

```csharp
builder.Services.AddScoped<ScheduleExportService>();
builder.Services.AddScoped<ScheduleValidationService>();
builder.Services.AddScoped<ClassroomRecommendationService>();
builder.Services.AddScoped<ScheduleGeneratorService>();
builder.Services.AddScoped<TeacherRecommendationService>();
```

The scoped lifetime means a new service instance is created for a given dependency-injection scope.

Controllers receive required dependencies through their constructors.

This reduces the need to instantiate application services manually and makes dependencies explicit.

---

## 15. Authentication

Authentication is implemented using ASP.NET Core Identity.

The application configures Identity with `ApplicationUser` and `IdentityRole` and uses Entity Framework Core for identity storage.

The authentication flow is:

```text
User
  |
  v
Login Form
  |
  v
ASP.NET Core Identity
  |
  v
Authentication Cookie
  |
  v
Authenticated Request
```

The application configures login and access-denied paths.

Password policy and account lockout settings are defined in `Program.cs`. The current configuration requires a minimum password length of eight characters, a digit, lowercase and uppercase letters, and a non-alphanumeric character. It also configures a lockout after repeated failed login attempts.

---

## 16. Authorization

Authorization controls access to protected actions and resources.

The application supports role-based access control with the roles:

```text
User
Admin
```

For example, `ScheduleGeneratorController` is protected with:

```csharp
[Authorize(Roles = "Admin")]
```

This restricts the controller to authenticated users assigned the `Admin` role.

Other controllers and actions may use different authorization rules. Access requirements should be checked against the relevant controller and action attributes.

---

## 17. Schedule Management Architecture

Manual schedule management is handled by `SchedulesController`.

The controller uses:

- `ApplicationDbContext`;
- `ScheduleExportService`;
- `ScheduleValidationService`;
- `ClassroomRecommendationService`;
- `TeacherRecommendationService`.

The general flow is:

```text
Browser
   |
   v
SchedulesController
   |
   +----------------------------+
   |             |              |
   v             v              v
Validation   Classroom       Teacher
Service      Recommendations Recommendations
   |             |              |
   +-------------+--------------+
                 |
                 v
         ApplicationDbContext
                 |
                 v
             PostgreSQL
```

The controller also supports schedule queries and filtering.

The exact services used depend on the action being executed.

---

## 18. Automatic Schedule Generation

Automatic schedule generation uses a dedicated controller and service.

The workflow includes a preview before the generated schedule is saved.

```text
ScheduleGeneratorController
            |
            v
Validate Generation Request
            |
            v
ScheduleGeneratorService
            |
            v
Load Required Data
            |
            v
Evaluate Candidate Assignments
            |
            v
Apply Scheduling Constraints
            |
            v
Return Generation Result
            |
            v
Display Preview
            |
            v
User Confirms Save
            |
            v
Persist Schedule Changes
```

The generator works with selected groups, days, time slots and generation settings.

The request model defines its own default values. The controller may provide different initial values when constructing the form model. Consequently, model defaults and the defaults displayed by the user interface are not necessarily identical.

Generation success depends on the supplied data and the constraints that can be satisfied. The system should not be described as guaranteeing a valid complete timetable for every input.

---

## 19. Schedule Validation Flow

Schedule validation is handled by `ScheduleValidationService`.

A simplified flow is:

```text
Schedule Input
      |
      v
Check Basic Values
      |
      v
Load Related Entities
      |
      v
Check Course Compatibility
      |
      v
Check Teacher–Subject Assignment
      |
      v
Check Capacity and Requirements
      |
      v
Check Scheduling Conflicts
      |
      v
Return Validation Result
```

The service returns a validation result that may contain errors and additional warning information.

Some checks may require user confirmation, depending on the rule and the controller workflow. The exact behavior is defined by the validation result and the action that consumes it.

---

## 20. Classroom Recommendations

Classroom recommendations help identify rooms that may be suitable for a lesson.

The general process is:

```text
Schedule Requirements
          |
          v
Read Group and Subject Data
          |
          v
Find Candidate Classrooms
          |
          v
Evaluate Category Requirements
          |
          v
Evaluate Capacity
          |
          v
Check Availability and Conflicts
          |
          v
Return Suitable Candidates
```

The relevant requirements depend on the subject, group and classroom configuration.

Recommendations support the scheduling process but do not replace the final validation of a schedule.

---

## 21. Notification Architecture

The application includes notification-related models, a notification controller and a notification bell ViewComponent.

The general presentation flow is:

```text
Notification Data
       |
       v
Application Database
       |
       v
Notification Controller / ViewComponent
       |
       v
Notification Interface
```

The `Notification` entity is exposed through `ApplicationDbContext`.

The ViewComponent is responsible for rendering the notification-bell component. Notification creation, delivery and read-state behavior depend on the corresponding controller and service implementation.

The presence of a notification model does not imply the use of a separate message broker or real-time push infrastructure.

---

## 22. Audit Logging

The application includes an `AuditLog` model and an `AuditLogsController`.

The general data flow is:

```text
Application Action
        |
        v
Audit Log Entry
        |
        v
ApplicationDbContext
        |
        v
PostgreSQL
        |
        v
Audit Log Interface
```

Audit records can provide a history of selected application events when the relevant logging code records them.

The actual set of tracked events depends on the implementation. The existence of an audit-log entity alone does not mean that every application action is automatically recorded.

---

## 23. Export Architecture

Schedule export is separated into `ScheduleExportService`.

The general workflow is:

```text
Export Request
      |
      v
Controller Action
      |
      v
ScheduleExportService
      |
      v
Generate Document
      |
      v
Return Download Response
```

The project references ClosedXML and QuestPDF.

- ClosedXML is used for Excel workbook generation.
- QuestPDF is used for PDF document generation.

The exact export actions and document types should be verified in the controller and service implementations.

Additional formats should only be documented after their implementation has been confirmed.

---

## 24. Razor Views and Presentation

The presentation layer is implemented with Razor Views.

Views are organized by feature, including account management, analytics, audit logs, classrooms, groups, notifications, schedule management, subjects and teachers.

Razor Views render server-provided data and generate HTML responses.

The presentation layer may use ViewModels, view data and model-binding mechanisms to transfer data between controller actions and views.

---

## 25. Frontend Architecture

The frontend uses:

- Razor;
- HTML5;
- CSS3;
- JavaScript.

CSS and JavaScript assets are served from the web root, including `wwwroot/css` and `wwwroot/js`.

The application interface combines shared styling with feature-specific presentation and client-side interactions.

Client-side validation and interaction improve usability, but server-side validation remains necessary because client-side checks can be bypassed.

The exact stylesheet names and their responsibilities should be maintained in accordance with the current contents of `wwwroot`.

---

## 26. Request Lifecycle

A typical page request follows the ASP.NET Core request pipeline.

```text
Browser
   |
   v
HTTPS Redirection
   |
   v
Static Files / Routing
   |
   v
Authentication Middleware
   |
   v
Session Middleware
   |
   v
Authorization Middleware
   |
   v
Controller Action
   |
   +------------------+
   |                  |
   v                  v
Application       ApplicationDbContext
Services               |
   |                   v
   |              Entity Framework Core
   |                   |
   +-------------------+
                       |
                       v
                   PostgreSQL
                       |
                       v
                Controller Result
                       |
                       v
                   Razor View
                       |
                       v
                    Browser
```

The exact middleware path depends on the requested resource and the configured pipeline. Static-file requests, for example, may be handled without executing a controller action.

---

## 27. Separation of Responsibilities

The main responsibilities are divided as follows.

### Controllers

Receive requests, coordinate operations and return responses.

### Services

Implement reusable application operations and complex processing.

### Models

Represent application entities and domain-related data.

### ViewModels

Represent data structures tailored to presentation requirements.

### Razor Views

Render HTML pages and forms.

### ApplicationDbContext

Provides Entity Framework Core access to application and Identity entities.

### Entity Framework Core

Maps objects to relational data and executes database operations.

### PostgreSQL

Stores persistent application data.

These responsibilities describe the logical architecture. Some straightforward data-access operations remain in controllers.

---

## 28. Configuration

Application configuration is managed through the ASP.NET Core configuration system.

Configuration includes values such as:

- database connection strings;
- administrator account settings;
- environment-specific behavior;
- application options.

The application uses the `DefaultConnection` connection-string setting for PostgreSQL.

The administrator seed process reads the administrator password from configuration. Environment variables can provide nested configuration values using double underscores, for example:

```text
Admin__Password
```

Sensitive credentials should be supplied through environment-specific configuration or an appropriate secrets-management mechanism rather than committed to source control.

The exact configuration requirements depend on the deployment environment.

---

## 29. Security Architecture

The security architecture combines authentication, authorization, validation and controlled data access.

```text
Incoming Request
       |
       v
Authentication
       |
       v
Authorization
       |
       v
Controller and Server-Side Validation
       |
       v
Application Logic
       |
       v
Entity Framework Core
       |
       v
PostgreSQL
```

Relevant security mechanisms include:

- ASP.NET Core Identity;
- role-based authorization;
- password policy;
- account lockout configuration;
- antiforgery validation on applicable form actions;
- server-side input validation;
- environment-based secret configuration.

The administrator password is read through configuration rather than hard-coded into the seeding logic.

Security depends on the full implementation and deployment configuration. This architecture description is not a substitute for a security audit.

---

## 30. Error Handling and Validation

The application uses input validation and operation-specific checks to identify invalid data and scheduling problems.

Examples include:

- invalid time ranges;
- missing related entities;
- incompatible group and subject courses;
- classroom capacity limitations;
- teacher-subject assignment warnings;
- scheduling conflicts.

Validation results are handled by the relevant controller and presented through the application interface.

The application also configures an exception handler for non-development environments.

The behavior of a specific error depends on the relevant controller, service and middleware configuration.

---

## 31. Database Migrations

Entity Framework Core migrations are used to manage changes to the database schema.

The general workflow is:

```text
Model or Mapping Change
          |
          v
Create Migration
          |
          v
Review Migration Files
          |
          v
Apply Migration
          |
          v
Updated Database Schema
```

Typical commands include:

```powershell
dotnet ef migrations add MigrationName
dotnet ef database update
```

Run these commands from the project directory with the required .NET tooling and database configuration available.

Migrations should be reviewed before being applied to a production database.

---

## 32. Build and Verification

The project can be built with the .NET CLI.

```powershell
dotnet restore
dotnet build
```

The build checks compilation and reports compiler errors and warnings.

When supported by the installed Entity Framework Core tooling, pending model changes can be checked with:

```powershell
dotnet ef migrations has-pending-model-changes
```

A successful build does not prove that all application workflows are correct. Database connectivity, authentication, generation, validation, export and other important features should also be tested.

---

## 33. Git Workflow

Git is used for source control, and GitHub hosts the remote repository.

A typical development workflow is:

```text
Modify Source Files
        |
        v
Review Changes
        |
        v
Run Build and Tests
        |
        v
Inspect Git Status
        |
        v
Commit Changes
        |
        v
Push to GitHub
```

Typical commands include:

```powershell
git status
git add .
git commit -m "Describe the changes"
git push
```

Changes should be reviewed before committing to avoid including credentials, generated artifacts or unrelated files.

---

## 34. Maintainability

The architecture provides several clear locations for future changes.

### New validation rule

A scheduling rule can be added to the relevant validation logic and integrated into the applicable workflow.

### New recommendation rule

Classroom or teacher recommendation behavior can be updated in the corresponding service.

### New export format

A new document format can be implemented in the export service and exposed through an appropriate controller action.

### New UI page

A new feature can use a controller action, Razor View and ViewModel where appropriate.

### New database entity

A new persistent entity may require a model, context configuration, relationships and a database migration.

The exact changes depend on the feature and its dependencies.

---

## 35. Scalability Considerations

The current architecture is appropriate for a college-level timetable management application.

If the workload or number of users grows significantly, possible improvements include:

- reviewing database indexes and query performance;
- introducing pagination for large datasets;
- adding caching where data can safely be reused;
- moving long-running generation operations to background processing;
- improving structured logging and monitoring;
- adding automated unit and integration tests;
- introducing API endpoints where required;
- optimizing database transactions and concurrency handling.

These are potential future improvements, not claims that the current application already implements them.

---

## 36. Current Architecture Summary

The application can be summarized as follows:

```text
                         Browser
                            |
                            v
                   ASP.NET Core MVC
                            |
                            v
                       Controllers
                            |
               +------------+------------+
               |                         |
               v                         v
           App Services            Razor Views
               |
               v
       ApplicationDbContext
               |
               v
       Entity Framework Core
               |
               v
           PostgreSQL
```

ASP.NET Core Identity integrates with the same Entity Framework Core context.

The application also includes dedicated components for schedule validation, automatic generation, recommendations, export, notifications and audit-related functionality.

---

## 37. Architectural Principles

The application is organized around the following principles:

1. MVC-based request handling and presentation.
2. Separation of presentation, application logic and data access.
3. Dedicated services for reusable and complex operations.
4. Entity Framework Core for relational data access.
5. PostgreSQL for persistent storage.
6. Dependency injection for application components.
7. ASP.NET Core Identity for authentication.
8. Role-based authorization for protected operations.
9. Entity Framework Core migrations for schema evolution.
10. ViewModels for presentation-specific data where appropriate.
11. Dedicated schedule-generation and validation components.
12. A separate service for schedule export.

These principles describe the current structure and intended organization of the application. They do not imply that every feature has independent projects or that all business logic resides exclusively in services.

---

## 38. Conclusion

ScheduleSystem is an ASP.NET Core MVC application built with .NET 8, Entity Framework Core and PostgreSQL.

Its architecture combines controllers, Razor Views, application services and a shared database context to support timetable management.

The main functional areas include:

- management of teachers, subjects, groups and classrooms;
- schedule creation and filtering;
- schedule validation;
- automatic schedule generation with preview and confirmation;
- classroom and teacher recommendation functionality;
- Excel and PDF export capabilities;
- Identity-based authentication and role-based authorization;
- notification and audit-related functionality.

The architecture is suitable for the current project scope and provides identifiable components for future maintenance and extension.

Further architectural improvements can be introduced as testing, performance, deployment and functional requirements evolve.
