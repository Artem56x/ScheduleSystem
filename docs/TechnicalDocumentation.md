# ScheduleSystem — Technical Documentation

## 1. Overview

ScheduleSystem is a web-based college timetable management system developed with ASP.NET Core MVC on .NET 8.

The application provides tools for managing academic data, creating and validating schedules, generating timetables, recommending classrooms, and exporting schedules.

The system uses PostgreSQL for data storage, Entity Framework Core for data access, and ASP.NET Core Identity for authentication and authorization.

## 2. Main Objectives

The main objectives of ScheduleSystem are:

- Managing teachers, student groups, subjects, and classrooms.
- Managing teacher-subject and group-subject relationships.
- Creating, editing, deleting, and filtering schedules.
- Validating timetable constraints and detecting conflicts.
- Generating schedules automatically.
- Recommending suitable classrooms.
- Managing user accounts and access roles.
- Displaying user notifications.
- Recording application activity through audit logs.
- Exporting schedules to PDF and Excel.
- Providing an interface for administrators and users.

## 3. Technology Stack

| Technology | Purpose |
|---|---|
| C# | Main programming language |
| .NET 8 | Application platform |
| ASP.NET Core MVC | Web application framework |
| Entity Framework Core | ORM and database access |
| PostgreSQL | Relational database |
| Npgsql | PostgreSQL provider for EF Core |
| ASP.NET Core Identity | Authentication and authorization |
| Razor Views | Server-side rendering |
| HTML5 | Page structure |
| CSS3 | Interface styling |
| JavaScript | Client-side interactions |
| ClosedXML | Excel export |
| QuestPDF | PDF generation |
| Git and GitHub | Version control and source hosting |

## 4. Application Architecture

ScheduleSystem follows an ASP.NET Core MVC architecture with supporting services and an Entity Framework Core data layer.

```text
User Browser
     |
     v
ASP.NET Core Middleware
     |
     v
MVC Controllers and Razor Views
     |
     +--------------------+
     |                    |
     v                    v
  ViewModels           Services
                          |
                          +-- ScheduleGeneratorService
                          +-- ScheduleValidationService
                          +-- ClassroomRecommendationService
                          +-- ScheduleExportService
                          +-- TeacherRecommendationService
     |
     v
ApplicationDbContext
     |
     v
Entity Framework Core
     |
     v
Npgsql
     |
     v
PostgreSQL
```

### 4.1 Controllers

Controllers process HTTP requests, coordinate application workflows, and return views or other HTTP responses.

The project includes controllers for major application areas, including:

- Account and authentication
- Home and dashboard
- Schedules
- Teachers
- Groups
- Subjects
- Classrooms
- Schedule generation
- Notifications
- Audit logs
- Analytics

The exact set of controllers may change as the project evolves.

### 4.2 Services

Services encapsulate reusable business logic where appropriate.

| Service | Responsibility |
|---|---|
| `ScheduleGeneratorService` | Generates schedules using available academic data and constraints |
| `ScheduleValidationService` | Checks timetable rules and scheduling conflicts |
| `ClassroomRecommendationService` | Recommends suitable classrooms |
| `TeacherRecommendationService` | Provides teacher-related recommendations |
| `ScheduleExportService` | Generates supported schedule export files |

The application is not a strictly layered architecture: some controllers also interact directly with `ApplicationDbContext`.

## 5. Project Structure

The main project structure includes:

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
├── ViewModels/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── ...
├── Program.cs
├── appsettings.json
├── README.md
└── ScheduleSystem.csproj
```

### Main directories

- `Controllers/` — HTTP request handling and application workflows.
- `Data/` — Entity Framework Core context and identity initialization.
- `Migrations/` — database schema migrations.
- `Models/` — domain and application models.
- `Services/` — business services.
- `Views/` — Razor views.
- `ViewModels/` — data prepared for presentation and forms.
- `ViewComponents/` — reusable view components.
- `wwwroot/` — static CSS, JavaScript, images, and other assets.

## 6. Data Model

The main domain entities include:

- `ApplicationUser`
- `Teacher`
- `TeacherSubject`
- `Subject`
- `Group`
- `GroupSubject`
- `Classroom`
- `ClassroomCategory`
- `SubjectClassroomCategory`
- `Schedule`
- `ScheduleTimeSlot`
- `ScheduleGenerationSettings`
- `Notification`
- `AuditLog`

ASP.NET Core Identity also maintains its associated identity and role entities.

### 6.1 Teacher–Subject Relationship

A teacher can teach multiple subjects, and a subject can be taught by multiple teachers.

The relationship is implemented through `TeacherSubject`.

```text
Teacher
   |
   v
TeacherSubject
   ^
   |
Subject
```

The junction entity uses the composite primary key:

```text
(TeacherId, SubjectId)
```

This relationship supports teacher assignment and schedule validation.

### 6.2 Group–Subject Relationship

The `GroupSubject` entity represents the many-to-many relationship between student groups and subjects.

It allows the application to associate subjects with the groups that study them.

### 6.3 Classroom Requirements

The `SubjectClassroomCategory` entity associates subjects with classroom categories.

This enables the system to represent requirements such as computer classrooms or other specialized facilities.

Classroom suitability can also depend on capacity and schedule availability.

## 7. Database Layer

ScheduleSystem uses PostgreSQL and Entity Framework Core with the Npgsql provider.

The primary database context is:

```csharp
ApplicationDbContext : IdentityDbContext<ApplicationUser>
```

This context integrates application data with ASP.NET Core Identity.

The context defines entity sets and relationships for academic data, schedules, classroom requirements, notifications, audit logs, and generation settings.

### Database configuration

The connection string is read from the `DefaultConnection` configuration setting.

The database name used in the standard local setup is:

```text
schedulesystem
```

Sensitive credentials should be provided through protected configuration or environment variables rather than committed to source control.

### Database migrations

EF Core migrations manage schema changes over time.

Create a migration:

```powershell
dotnet ef migrations add MigrationName
```

Apply migrations:

```powershell
dotnet ef database update
```

List migrations:

```powershell
dotnet ef migrations list
```

Check for pending model changes:

```powershell
dotnet ef migrations has-pending-model-changes
```

## 8. Schedule Management

The schedule module supports the creation and management of timetable entries.

The main capabilities include:

- Creating schedule entries.
- Editing and deleting entries.
- Filtering schedules by group.
- Filtering by teacher.
- Filtering by classroom.
- Filtering by subject.
- Filtering by day of the week.
- Filtering by time range.
- Viewing and reviewing the resulting timetable.

Each schedule entry connects the relevant academic group, teacher, subject, and classroom.

## 9. Schedule Validation

The schedule validation service checks timetable data against configured rules.

### 9.1 Time Validation

The start time must be earlier than the end time.

### 9.2 Scheduling Conflicts

The application checks for overlapping assignments involving relevant resources, including teachers, groups, and classrooms.

### 9.3 Classroom Capacity

The selected classroom should have sufficient capacity for the student group.

```text
Classroom.Capacity >= Group.StudentCount
```

### 9.4 Classroom Category Requirements

If a subject requires a particular classroom category, the selected classroom must satisfy the configured requirement.

### 9.5 Course Compatibility

The application checks course compatibility where applicable.

### 9.6 Teacher–Subject Assignment

The application checks whether the selected teacher is associated with the selected subject. The interface supports an explicit confirmation flow for certain teacher-subject mismatches.

Validation behavior depends on the specific rule; not every warning necessarily prevents an operation from continuing.

## 10. Automatic Schedule Generation

The application contains a schedule generation workflow implemented through `ScheduleGeneratorController` and `ScheduleGeneratorService`.

The generator considers available academic data, time slots, and scheduling constraints.

The workflow is:

```text
Review Generation Settings
           |
           v
Prepare Generation Request
           |
           v
Run Schedule Generator
           |
           v
Validate Generated Results
           |
           v
Display Preview
           |
           v
Administrator Confirmation
           |
           v
Save Generated Schedule
```

The preview is cached temporarily before confirmation. The generation controller restricts access to administrators.

### Generation parameters

The generation interface supports parameters such as:

- Teaching days
- Time slots
- Maximum lessons per day
- Maximum consecutive lessons
- Lesson distribution
- Classroom recommendations

**Important:** Model defaults, database settings, and values displayed in the generator interface may differ. Actual generation behavior depends on the request parameters and current implementation. Do not assume that one set of default values applies to every generation workflow.

The generator applies search and scheduling constraints to find a suitable timetable. A successful result depends on the availability and compatibility of the input data.

## 11. Classroom Recommendations

`ClassroomRecommendationService` helps identify suitable classrooms.

Relevant criteria include:

1. Classroom category requirements.
2. Classroom capacity.
3. Student group size.
4. Classroom availability.
5. Scheduling conflicts.

The recommendation functionality reduces the need to check every classroom manually.

Recommendations are based on available application data and do not replace verification of the final timetable.

## 12. Teacher Management

The teacher module supports teacher records and subject assignments.

A teacher may be associated with multiple subjects through `TeacherSubject`.

When creating or editing a schedule, the application can validate the teacher-subject relationship.

## 13. Group Management

The group module stores academic group information, including:

- Group name
- Course
- Student count

Groups are associated with subjects through `GroupSubject`.

The student count is used when validating classroom capacity and recommending rooms.

## 14. Subject Management

Subject records contain academic information such as:

- Subject name
- Course
- Subject type
- Required classroom categories

Subjects can be associated with multiple teachers and student groups through the corresponding relationship entities.

## 15. Classroom Management

Classroom records contain information such as:

- Classroom name
- Classroom category
- Capacity

The application represents classroom types through `ClassroomCategory` rather than relying on a separate computer-equipment flag.

Subject requirements are represented through `SubjectClassroomCategory`.

## 16. Authentication and Authorization

ScheduleSystem uses ASP.NET Core Identity for authentication and account management.

The application defines the roles:

- `Admin`
- `User`

Identity functionality includes registration, login, logout, profile information, and password-related operations.

Administrative functionality is protected by role-based authorization.

### Administrator initialization

The application initializes required roles and the administrator account through `IdentitySeeder`.

The configured administrator email is:

```text
admin@schedulesystem.local
```

The administrator password is supplied through configuration, including the `Admin__Password` environment variable.

Real passwords must not be stored in source code or public documentation.

## 17. Notifications

The application includes a notification system with a notification interface.

Notification records can contain a title, message, read status, user association, and creation timestamp.

The system provides a place to display relevant application information.

The existence of notification records does not, by itself, imply real-time push delivery.

## 18. Audit Logs

The application contains an audit log system for recording selected application actions.

Audit log records can include:

- User identifier
- Action name
- Entity name
- Entity identifier
- Timestamp

The administrative interface provides access to audit information.

The completeness of the audit history depends on which operations explicitly record audit entries.

## 19. Schedule Export

Schedule export is implemented through `ScheduleExportService`.

The confirmed export formats are:

| Format | Library | Purpose |
|---|---|---|
| Excel | ClosedXML | Spreadsheet export and further analysis |
| PDF | QuestPDF | Printing and sharing schedules |

CSV should not be documented as a supported export format unless the current implementation has been verified.

Export options depend on the functionality exposed by the relevant interface.

## 20. User Interface

The application uses Razor Views, HTML, CSS, and JavaScript.

The interface follows a modern white-and-blue visual style with glass-inspired elements.

The main application areas include:

- Home and dashboard
- Schedule
- Teachers
- Groups
- Subjects
- Classrooms

The interface includes reusable visual components and page-specific styles.

The frontend refresh described below focused on presentation and interaction while preserving the existing backend behavior.

## 21. Application Configuration

Configuration is handled through ASP.NET Core configuration mechanisms.

Important settings include:

- Database connection string
- Administrator password
- Identity settings
- Application behavior and service configuration

Environment variables can override hierarchical configuration values by using double underscores.

For example:

```powershell
$env:Admin__Password = "YOUR_STRONG_PASSWORD"
```

The application should be started from an environment where the required settings are available.

Secrets should not be committed to Git.

## 22. Dependency Injection and Middleware

Application services are registered through the ASP.NET Core dependency injection container.

The application configures MVC, database access, Identity, session support, caching, and application services.

The HTTP request pipeline includes middleware for functionality such as:

- HTTPS redirection
- Static files
- Routing
- Authentication
- Session
- Authorization

The precise order matters because middleware components depend on earlier stages of the request pipeline.

## 23. Build and Verification

The standard verification workflow includes:

```powershell
dotnet restore
dotnet build
dotnet ef database update
```

The build checks whether the application compiles. Database update applies pending migrations to the configured database.

A successful build does not, by itself, prove that all runtime workflows are correct.

Testing should also cover authentication, authorization, database operations, schedule validation, generation, notifications, and export.

## 24. Security Considerations

The application uses ASP.NET Core Identity, role-based authorization, server-side validation, and protected configuration for administrator credentials.

For production deployments, additional measures should include:

- HTTPS and appropriate certificate management.
- Secure PostgreSQL credentials.
- Restricted database network access.
- Protected environment variables or secret storage.
- Regular database backups.
- Controlled administrator access.
- Dependency updates and vulnerability review.
- Appropriate logging and monitoring.

Security behavior should be verified against the current application code and deployment configuration.

## 25. Development Workflow

The source repository is hosted on GitHub:

https://github.com/Artem56x/ScheduleSystem

The main branch is:

```text
main
```

A typical development workflow is:

```text
Edit
  |
  v
Restore Dependencies
  |
  v
Build
  |
  v
Test
  |
  v
Review Changes
  |
  v
Commit
  |
  v
Push
```

Database migrations should be reviewed before being applied to a shared or production database.

## 26. Project Status

The documented implementation includes:

- ASP.NET Core MVC architecture.
- PostgreSQL database integration.
- Entity Framework Core migrations.
- Academic CRUD modules.
- Teacher-subject and group-subject relationships.
- Schedule management and filtering.
- Schedule validation.
- Automatic schedule generation.
- Classroom recommendations.
- Authentication and role-based authorization.
- Notifications and audit logging.
- PDF and Excel export.
- Interface modernization.
- Administrator password configuration through protected settings.

The status of any particular feature should be verified against the current source code before a new release.

## 27. Future Improvements

Potential future improvements include:

- Automated integration and end-to-end tests.
- Additional reporting and analytics.
- More advanced scheduling algorithms.
- More granular authorization policies.
- Additional localization options.
- Production deployment automation.
- Expanded monitoring and operational diagnostics.

These items are possible directions for further development, not claims that the functionality is already implemented.

## 28. Conclusion

ScheduleSystem is a web-based timetable management application built with ASP.NET Core MVC, .NET 8, Entity Framework Core, PostgreSQL, and ASP.NET Core Identity.

It combines academic data management, timetable validation, automatic generation, classroom recommendations, user management, notifications, audit logging, and schedule export.

Its service layer, relational data model, and migration-based database management provide a foundation for maintenance and future development.

---

## 29. Interface Refresh Validation — 9 October 2026

This section documents the reported interface refresh and its validation results.

The refresh focused on existing Razor views, CSS, and JavaScript. It did not intentionally introduce new product screens, backend modules, database migrations, or a new frontend build system.

### 29.1 Styling and Interaction Responsibilities

The interface styling was organized around shared and page-specific responsibilities:

- `site.css` defines shared tokens and reusable components.
- `liquid-glass.css` handles the application shell, modal presentation, and shared print rules.
- Page-specific styles control section layouts.
- `site.js` supports shared interaction behavior, including modal focus management and navigation behavior.

The design work addressed typography, form controls, focus visibility, responsive layouts, table overflow, dialogs, navigation, keyboard interaction, and print presentation.

Schedule filtering and export scripts were reported as unchanged from the baseline.

### 29.2 Validation Environment

The reported comparisons used baseline commit:

```text
9a09ffb4d9a66095bc50c9cf2e2dc0aa590bf16b
```

The checks used the same disposable local PostgreSQL database for the before-and-after comparison.

Temporary fixtures, review scripts, database backups, and screenshots were kept outside the repository.

### 29.3 Reported Validation Results

| Check | Reported result |
|---|---|
| .NET 8 restore and build | Passed; final build had 0 warnings and 0 errors |
| HTTP regression comparison | 108 matching GET/POST records |
| Final rendered form comparison | 42 screens and 45 requests matched the baseline |
| JavaScript syntax | 8 external files and 10 rendered inline blocks passed syntax checks |
| CSS and JavaScript resources | 53 linked resources returned HTTP 200 |
| Source boundary review | Existing conditions, business attributes, and antiforgery controls retained |
| Responsive layout | Six principal pages checked at seven viewport sizes |
| Reachable-page narrow layout | 41 pages checked at 360 × 800 |
| Browser interactions | Filters, reset, no-results, navigation, tabs, dialogs, and generator preview flows checked |
| Short modal viewport | Preview confirmation checked at 844 × 390 |
| Print styling | Full and filtered schedule scenarios reviewed |
| Palette contrast | Reported text and surface combinations exceeded the documented thresholds |

These figures describe the reported review results at the time of testing. They should not be interpreted as proof of complete automated test coverage.

### 29.4 Responsive and Accessibility Checks

The review covered multiple viewport widths and heights, keyboard navigation, focus restoration, modal cancellation, visible focus indicators, form labels, and reduced-motion rules.

The review also checked schedule tables for local scrolling and semantic column headers.

However, the review did not fully exercise physical mobile keyboards, actual 200% browser zoom, or complete screen-reader acceptance with assistive software and hardware.

### 29.5 Print Validation

The print review used a full schedule containing 30 lessons across three groups and a filtered schedule containing one lesson for one group.

The reviewed styles hide the application shell and administrative actions, reset the sidebar offset, and attempt to avoid splitting individual lessons.

Actual browser pagination, long-group page breaks, and printed PDF output still require manual acceptance testing.

### 29.6 Remaining Manual Acceptance

The following checks were still identified as requiring further validation:

- Native browser print preview and PDF pagination.
- Actual 200% browser zoom.
- Physical mobile on-screen keyboard behavior.
- Full screen-reader and reduced-motion acceptance.
- Large-institution data sets with substantially more groups.
- Native browser submission behavior for the study-load workflow and its `beforeunload` interaction.

The study-load workflow was reported to create the expected indexed hidden fields in the browser. Independent HTTP regression checks verified server-side saving and redirection, although the in-app browser remained in a loading state during one interaction test.

### 29.7 Existing Issues Outside the Interface Refresh

Two existing backend issues were identified during the review.

**Access denied route:** `Views/Account/AccessDenied.cshtml` exists, but the reviewed `AccountController` did not contain a corresponding action. Forbidden administrative actions redirected to `/Account/AccessDenied`, which returned HTTP 404 in both the baseline and refreshed versions.

**Notification visibility:** `HomeController.Index` was reported to retrieve the latest notifications without filtering them by user. A notification associated with a test administrator was visible on the anonymous home page in the baseline.

The interface refresh did not resolve these issues. They require separate backend changes and regression testing.

### 29.8 Scope and Limitations

The reported validation is a point-in-time comparison, not a guarantee that all application behavior is correct.

The repository did not contain an existing automated test project at the time of the review. The review harness was kept outside the source tree and was not part of the normal build.

No publication or merge is implied by this documentation section.
