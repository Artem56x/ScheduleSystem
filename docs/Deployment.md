# ScheduleSystem — Deployment Guide

## 1. Overview

This guide describes how to install, configure, run, and update ScheduleSystem on a Windows computer.

ScheduleSystem is an ASP.NET Core MVC web application built with .NET 8 and PostgreSQL.

### Technology Stack

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- PostgreSQL
- Npgsql
- ASP.NET Core Identity
- Git

### Requirements

- Windows 10 or later
- .NET 8 SDK
- PostgreSQL
- Git
- Internet access for downloading dependencies
- Sufficient disk space and memory for the application and database

The exact PostgreSQL version should be selected according to the deployment environment and compatibility requirements.

---

## 2. Install Required Software

### .NET 8 SDK

Install the .NET 8 SDK from the official website:

https://dotnet.microsoft.com/download/dotnet/8.0

Verify the installation in PowerShell:

```powershell
dotnet --version
dotnet --list-sdks
```

The installed SDK list should contain a version from the .NET 8 family.

### PostgreSQL

Install PostgreSQL using the official website:

https://www.postgresql.org/download/

Make sure the PostgreSQL service is running and that you know the database username and password.

The default PostgreSQL port is usually `5432`.

### Git

Install Git from:

https://git-scm.com/downloads

Verify the installation:

```powershell
git --version
```

---

## 3. Obtain the Source Code

The source code is hosted on GitHub.

Repository: https://github.com/Artem56x/ScheduleSystem

Clone the repository:

```powershell
git clone https://github.com/Artem56x/ScheduleSystem.git
```

Navigate to the project directory:

```powershell
cd ScheduleSystem
```

Verify the downloaded files:

```powershell
dir
```

The project should contain files and directories similar to:

```text
Controllers/
Data/
Models/
Services/
Views/
ViewModels/
Migrations/
wwwroot/
Program.cs
appsettings.json
ScheduleSystem.csproj
```

The exact directory structure may change between versions.

---

## 4. Restore Dependencies

Restore the NuGet packages required by the project:

```powershell
dotnet restore
```

This command downloads the dependencies defined in the project file.

If the restore operation fails, check the internet connection, NuGet configuration, and any reported package errors.

---

## 5. Configure the Database

ScheduleSystem uses PostgreSQL through Entity Framework Core and the Npgsql provider.

The application reads its connection string from the `DefaultConnection` configuration setting.

A typical local connection string is:

```text
Host=localhost;Port=5432;Database=schedulesystem;Username=postgres;Password=YOUR_PASSWORD
```

Replace `YOUR_PASSWORD` with the password configured for your PostgreSQL account.

The required database name is:

```text
schedulesystem
```

### Configure the connection string

For local development, the connection string can be supplied through ASP.NET Core configuration or an environment variable.

To use an environment variable in the current PowerShell session:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=schedulesystem;Username=postgres;Password=YOUR_PASSWORD"
```

Replace the example credentials before running the application.

ASP.NET Core maps double underscores (`__`) in environment variable names to configuration hierarchy separators. Therefore, `ConnectionStrings__DefaultConnection` corresponds to `ConnectionStrings:DefaultConnection`.

Do not commit real database credentials to Git.

---

## 6. Create the Database

Create a PostgreSQL database named `schedulesystem` if it does not already exist.

You can use pgAdmin or the PostgreSQL command-line tool.

Example using `psql`:

```sql
CREATE DATABASE schedulesystem;
```

Run this command using a PostgreSQL account with permission to create databases.

If the database already exists, do not create it again.

---

## 7. Configure the Administrator Password

ScheduleSystem uses an environment variable named `Admin__Password` to provide the administrator password to the application's initialization logic.

Set it in the current PowerShell session:

```powershell
$env:Admin__Password = "YOUR_STRONG_PASSWORD"
```

Verify that the variable is set without printing the password:

```powershell
if ($env:Admin__Password) {
    Write-Host "Administrator password is configured."
} else {
    Write-Host "Administrator password is not configured."
}
```

Keep the password private. Do not include it in screenshots, Git commits, or documentation.

**Important:** Configure this variable in the same environment from which you start the application. A variable set in one PowerShell session is not automatically available in every other session.

---

## 8. Administrator Account

The application initializes the administrator role and account through its Identity seeding logic when required.

The configured administrator email is:

```text
admin@schedulesystem.local
```

The password must be supplied through the secure configuration mechanism described in the previous section.

If the application fails during startup, verify that `Admin__Password` is configured and review the terminal output.

The administrator email is a local application account identifier, not a public email service.

---

## 9. Install the Entity Framework Core CLI

ScheduleSystem uses Entity Framework Core migrations to manage the database schema.

Check whether the EF Core CLI is available:

```powershell
dotnet ef
```

If the command is unavailable, install the tool:

```powershell
dotnet tool install --global dotnet-ef
```

If it is already installed and needs updating:

```powershell
dotnet tool update --global dotnet-ef
```

If PowerShell cannot find `dotnet-ef` after installation, restart the terminal and verify that the .NET global tools directory is included in `PATH`.

---

## 10. Apply Database Migrations

From the directory containing `ScheduleSystem.csproj`, run:

```powershell
dotnet ef database update
```

Entity Framework Core applies the existing migrations to the database configured by the application.

List available migrations:

```powershell
dotnet ef migrations list
```

Check whether the current model contains changes that have not been added to a migration:

```powershell
dotnet ef migrations has-pending-model-changes
```

A model change does not necessarily mean that an existing migration has failed. If changes are reported, review the model and migration history before creating a new migration.

For a production database, back up the data and review migrations before applying schema changes.

---

## 11. Build the Application

Build the project:

```powershell
dotnet build
```

A successful build should complete without errors.

If compilation fails, review the reported errors and resolve them before continuing.

For a deployment check, you can also use:

```powershell
dotnet build --configuration Release
```

---

## 12. Run the Application

Ensure that the database connection and administrator password are configured in the current PowerShell session.

Start the application:

```powershell
dotnet run
```

The terminal displays the local addresses on which the application is listening.

Open the address shown in the terminal. It may resemble:

```text
https://localhost:7000
```

or:

```text
http://localhost:5000
```

These are examples only. Use the actual address printed by the application.

If HTTPS displays a local development certificate warning, configure and trust the ASP.NET Core development certificate as appropriate for your environment. Do not disable HTTPS security checks in production.

---

## 13. First Login

After the application starts:

1. Open the local application address in a browser.
2. Navigate to the login page.
3. Enter `admin@schedulesystem.local`.
4. Enter the administrator password configured through `Admin__Password`.
5. Sign in.
6. Verify that administrator functionality is available.

If login fails, check the password configuration, database connection, Identity initialization, and application logs.

---

## 14. Initial Data Configuration

Before generating a timetable, configure the reference data required by the application.

A recommended sequence is:

```text
Classroom Categories
        ↓
Classrooms
        ↓
Teachers
        ↓
Subjects
        ↓
Teacher–Subject Assignments
        ↓
Student Groups
        ↓
Group–Subject Assignments
        ↓
Schedule Configuration
        ↓
Schedule Generation
```

The exact order can vary depending on which records already exist and which relationships the interface requires.

### Classroom Categories

Create the classroom categories required by the institution.

Examples:

- Standard Classroom
- Computer Classroom
- Specialized Classroom

Use categories that match the actual requirements of the institution.

### Classrooms

For each classroom, configure:

- Name or room number
- Classroom category
- Capacity

Example:

```text
Room: 305
Category: Computer Classroom
Capacity: 25
```

Ensure that the capacity reflects the actual number of students the room can accommodate.

### Teachers

Create teacher records and assign the subjects they are qualified to teach.

A teacher can be associated with multiple subjects.

### Subjects

Create the subjects used by the institution.

Configure the available subject information, such as name, course, type, and classroom-category requirements, according to the fields provided by the application.

### Student Groups

Create the student groups and configure their names, courses, and student counts.

Student counts are important when evaluating classroom capacity and recommendations.

### Verify Reference Data

Before generating a timetable, verify:

- Teacher and subject assignments
- Group and subject assignments
- Classroom categories and capacities
- Subject classroom requirements
- Group student counts
- Schedule days and time slots

Incorrect reference data can lead to invalid or unexpected timetable assignments.

---

## 15. Generate and Validate a Schedule

Once the reference data is ready:

1. Open the schedule generation functionality.
2. Review the generation settings.
3. Start the generation process.
4. Review the generated preview.
5. Check for conflicts and unexpected assignments.
6. Confirm and save the generated schedule through the application.

Schedule generation uses the available academic data and configured constraints.

The application includes validation functionality for relevant scheduling issues, including teacher, group, and classroom conflicts, time ranges, classroom capacity, and classroom-category requirements.

Review validation warnings carefully. Not every warning necessarily means that the schedule cannot be saved; the behavior depends on the particular validation rule.

---

## 16. Review and Export the Schedule

Use the schedule filters to review the timetable by available criteria, such as group, teacher, classroom, subject, day, and time.

Before distributing the timetable, verify that:

- Teachers are assigned correctly.
- Groups have the expected lessons.
- Classrooms meet the requirements.
- Time slots do not conflict.
- The generated schedule matches the institution's requirements.

ScheduleSystem supports schedule export in the following formats:

| Format | Typical use |
|---|---|
| Excel | Further editing and analysis |
| PDF | Printing and sharing |

Export availability may depend on the current application interface and the data selected for export.

---

## 17. Update the Application

Before updating an existing installation, back up the database.

Navigate to the project directory and retrieve the latest changes:

```powershell
git pull origin main
```

If you have local source-code changes, commit or back them up before pulling updates.

Restore dependencies:

```powershell
dotnet restore
```

Build the application:

```powershell
dotnet build --configuration Release
```

Apply migrations if the new version introduces database schema changes:

```powershell
dotnet ef database update
```

Restart the application and verify the main features.

The update workflow is:

```text
Back Up Database
        ↓
Retrieve New Version
        ↓
Restore Dependencies
        ↓
Build Application
        ↓
Apply Required Migrations
        ↓
Start Application
        ↓
Verify Functionality
```

---

## 18. Database Backup and Restore

### Create a Backup

A PostgreSQL custom-format backup can be created with `pg_dump`:

```powershell
pg_dump -U postgres -d schedulesystem -F c -f schedulesystem_backup.dump
```

Depending on the installation, you may need to provide the PostgreSQL host, port, or executable path.

Enter credentials securely when prompted. Avoid putting passwords directly in shell commands or scripts.

### Restore a Backup

A custom-format backup can be restored with `pg_restore`.

For example, to restore into an existing, appropriately prepared database:

```powershell
pg_restore -U postgres -d schedulesystem schedulesystem_backup.dump
```

Restoring into a database that already contains application data may cause conflicts or duplicate records. When performing a full recovery, follow a deliberate restore procedure and verify the target database before proceeding.

Store backup files securely and test that backups can actually be restored.

---

## 19. Production Configuration

Running the application with `dotnet run` is suitable for development and basic local testing. It is not, by itself, a complete production hosting setup.

A production deployment should account for:

- A secure database connection and credentials
- A securely configured administrator password
- HTTPS and certificate management
- Appropriate application hosting and process management
- Logging and error monitoring
- Database backup and recovery procedures
- Restricted database and administrator access
- Protected configuration and secrets
- Dependency and security updates

Do not expose the PostgreSQL port or development server directly to the public internet without an appropriate network and security configuration.

The exact hosting procedure depends on whether the application is deployed to Windows Server, a virtual machine, a container platform, or another hosting environment.

---

## 20. Troubleshooting

### `dotnet` Is Not Recognized

Install the .NET 8 SDK and restart PowerShell.

Verify:

```powershell
dotnet --list-sdks
```

### `dotnet ef` Is Not Recognized

Install or update the global EF Core tool:

```powershell
dotnet tool install --global dotnet-ef
```

Restart the terminal if necessary.

### PostgreSQL Connection Fails

Check:

- Whether the PostgreSQL service is running
- Host and port
- Database name
- Username and password
- `ConnectionStrings__DefaultConnection`
- Database access permissions

The default PostgreSQL port is usually `5432`.

### The Database Does Not Exist

Create the `schedulesystem` database using pgAdmin or `psql`, then apply migrations:

```powershell
dotnet ef database update
```

### Migration Application Fails

Check the database connection, permissions, migration history, and application configuration.

Before attempting corrective schema changes, back up the database and inspect the error message.

### Administrator Initialization Fails

Verify that the application process has access to `Admin__Password`.

In the current PowerShell session:

```powershell
if (-not $env:Admin__Password) {
    Write-Host "Admin__Password is missing."
}
```

Configure the variable and restart the application.

### The Application Starts but Pages Fail

Review:

- Terminal output and application logs
- Database connectivity
- Migration status
- Browser developer console
- Authentication and authorization
- The specific HTTP status code

Use the error details to identify the problem rather than modifying database tables or migration history blindly.

---

## 21. Deployment Verification Checklist

### Software and Configuration

- [ ] .NET 8 SDK is installed.
- [ ] PostgreSQL is running.
- [ ] Git is installed.
- [ ] Source code has been retrieved.
- [ ] Dependencies have been restored.
- [ ] Database connection is configured.
- [ ] `Admin__Password` is configured securely.

### Database

- [ ] The `schedulesystem` database exists.
- [ ] EF Core migrations have been applied.
- [ ] The application can connect to PostgreSQL.
- [ ] A database backup has been created.

### Application

- [ ] The project builds successfully.
- [ ] The application starts without startup errors.
- [ ] The login page opens.
- [ ] The administrator can sign in.
- [ ] Authorization behaves as expected.
- [ ] Teachers, groups, subjects, and classrooms can be managed.
- [ ] Schedule filtering works.
- [ ] Schedule validation works.
- [ ] Schedule generation and confirmation work.
- [ ] Notifications and audit logging work as expected.
- [ ] Excel and PDF export work where available.

---

## 22. Deployment Architecture

A basic deployment consists of a browser, the ASP.NET Core application, and the PostgreSQL database.

```text
┌───────────────────────────────┐
│          Client PC            │
│                               │
│         Web Browser           │
└───────────────┬───────────────┘
                │
                │ HTTP / HTTPS
                ▼
┌───────────────────────────────┐
│       ASP.NET Core App        │
│                               │
│         ScheduleSystem        │
└───────────────┬───────────────┘
                │
                │ Npgsql
                ▼
┌───────────────────────────────┐
│          PostgreSQL           │
│                               │
│         schedulesystem        │
└───────────────────────────────┘
```

The application communicates with PostgreSQL through Entity Framework Core and Npgsql.

---

## 23. Conclusion

ScheduleSystem can be installed locally on Windows using the .NET 8 SDK, PostgreSQL, and Git.

The essential deployment steps are:

1. Install the required software.
2. Retrieve the source code.
3. Configure the PostgreSQL connection.
4. Create the database.
5. Configure `Admin__Password`.
6. Apply Entity Framework Core migrations.
7. Build and run the application.
8. Sign in and configure reference data.
9. Generate, validate, and export the schedule.
10. Verify the installation and establish a backup procedure.

For production use, additional hosting, security, monitoring, and recovery configuration is required.

## Related Documentation

- `Architecture.md` — application architecture.
- `ArchitectureDiagram.md` — architecture diagrams.
- `Database.md` — database documentation.
- `DatabaseDiagram.md` — database ER diagram.
- `TechnicalDocumentation.md` — technical implementation.
- `AdminGuide.md` — administrator instructions.
- `UserGuide.md` — user instructions.
