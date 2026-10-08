# ScheduleSystem — Deployment Guide

## 1. Overview

This guide describes how to install and run ScheduleSystem on a Windows computer.

The deployment requires:

* Windows;
* .NET 8 SDK;
* PostgreSQL;
* Git;
* ScheduleSystem source code.

The application uses:

* ASP.NET Core MVC;
* .NET 8;
* Entity Framework Core;
* PostgreSQL;
* Npgsql;
* ASP.NET Core Identity.

---

## 2. System Requirements

### Minimum Requirements

The application is designed to run on a modern Windows computer with:

* Windows 10 or later;
* .NET 8 SDK;
* PostgreSQL 14 or later;
* Git;
* at least 4 GB RAM;
* available disk space for the project and database.

The exact PostgreSQL version may vary depending on the deployment environment.

---

## 3. Required Software

Install the following components before starting deployment.

### .NET 8 SDK

The .NET SDK is required to build and run the application.

Verify the installation:

```powershell
dotnet --version
```

The command should return a version belonging to the .NET 8 SDK family.

Verify installed SDKs:

```powershell
dotnet --list-sdks
```

---

## 4. PostgreSQL

PostgreSQL is the database management system used by ScheduleSystem.

After installation, verify that the PostgreSQL service is running.

The application requires a PostgreSQL database named:

```text
schedulesystem
```

The actual connection credentials depend on the deployment environment.

---

## 5. Git

Git is recommended for obtaining and updating the project source code.

Verify Git:

```powershell
git --version
```

---

## 6. Obtaining the Project

The project repository is hosted on GitHub.

Repository:

[ScheduleSystem GitHub Repository](https://github.com/Artem56x/ScheduleSystem.git?utm_source=chatgpt.com)

Clone the repository:

```powershell
git clone https://github.com/Artem56x/ScheduleSystem.git
```

Enter the project directory:

```powershell
cd ScheduleSystem
```

---

## 7. Checking the Project

After cloning, verify the project files:

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

---

## 8. Restoring Dependencies

Restore the .NET dependencies:

```powershell
dotnet restore
```

This downloads the required NuGet packages defined by the project.

---

## 9. Database Configuration

The application uses an ASP.NET Core configuration system for database settings.

The PostgreSQL connection string must point to the target database.

A typical connection string has the following structure:

```text
Host=localhost;Port=5432;Database=schedulesystem;Username=postgres;Password=YOUR_PASSWORD
```

The actual credentials must be configured for the local environment.

Do not commit real production database passwords to Git.

---

## 10. Creating the Database

If the database does not already exist, create a PostgreSQL database named:

```text
schedulesystem
```

The database can be created using:

* pgAdmin;
* PostgreSQL command-line tools;
* another PostgreSQL administration application.

After creating the database, configure the application's connection string.

---

## 11. Entity Framework Core

ScheduleSystem uses Entity Framework Core migrations to create and update the database schema.

If the EF Core CLI tool is not installed, install it with:

```powershell
dotnet tool install --global dotnet-ef
```

If it is already installed, update it when necessary:

```powershell
dotnet tool update --global dotnet-ef
```

Verify the installation:

```powershell
dotnet ef
```

---

## 12. Applying Database Migrations

After configuring the database connection, apply the existing migrations:

```powershell
dotnet ef database update
```

Entity Framework Core will apply the migrations in the correct order.

The database will be updated to the schema expected by the current version of ScheduleSystem.

---

## 13. Verifying Migrations

To check whether the project has unexpected model changes:

```powershell
dotnet ef migrations has-pending-model-changes
```

A clean deployment should not have unexpected pending model changes.

To view the available migrations:

```powershell
dotnet ef migrations list
```

---

## 14. Administrator Password Configuration

The administrator password is intentionally not stored directly in the source code.

ScheduleSystem expects the administrator password through configuration or an environment variable.

The environment variable name is:

```text
Admin__Password
```

On Windows PowerShell, it can be configured for the current session:

```powershell
$env:Admin__Password="YOUR_STRONG_PASSWORD"
```

Verify that it is available:

```powershell
$env:Admin__Password
```

Do not publish the password in documentation, Git commits or screenshots.

---

## 15. Administrator Account

The administrator account uses the configured email:

```text
admin@schedulesystem.local
```

The password must be supplied through the secure configuration mechanism described above.

The administrator account is created or initialized by the application's identity seeding logic when required.

---

## 16. Building the Application

Before starting the application, build the project:

```powershell
dotnet build
```

A successful build should complete without errors.

If the build fails, resolve the reported errors before continuing with deployment.

---

## 17. Running the Application

Start the application using:

```powershell
dotnet run
```

ASP.NET Core will start the web server and display the local application address in the terminal.

The address may look similar to:

```text
https://localhost:xxxx
```

or:

```text
http://localhost:xxxx
```

Use the address shown by the application rather than assuming a specific port.

---

## 18. Development Environment

For development, the application can be launched directly with:

```powershell
dotnet run
```

Changes can then be made to the source code and tested locally.

A typical development workflow is:

```text
Edit
  ↓
Build
  ↓
Run
  ↓
Test
  ↓
Review
  ↓
Commit
```

---

## 19. First Login

After the application starts:

1. Open the local application address.
2. Navigate to the login page.
3. Enter the administrator email.
4. Enter the configured administrator password.
5. Sign in.
6. Verify that administrator functionality is available.

Administrator email:

```text
admin@schedulesystem.local
```

---

## 20. Initial Data Configuration

After the first successful login, configure the application data.

Recommended order:

```text
Classroom Categories
        ↓
Classrooms
        ↓
Teachers
        ↓
Subjects
        ↓
Teacher–Subject Relations
        ↓
Groups
        ↓
Schedule
```

This order ensures that dependent entities have the required reference data.

---

## 21. Creating Classroom Categories

Create the categories required by the institution.

For example:

```text
Standard Classroom
Computer Classroom
Specialized Classroom
```

The names should match the actual organizational requirements.

---

## 22. Creating Classrooms

Add available classrooms.

For each classroom specify:

* name;
* category;
* capacity.

Example:

```text
Room: 305
Category: Computer Classroom
Capacity: 25
```

Verify that the capacity corresponds to the real classroom.

---

## 23. Creating Teachers

Add the teachers who participate in the timetable.

For each teacher:

1. Enter the teacher's information.
2. Select the subjects they teach.
3. Save the record.

A teacher can have multiple subjects.

---

## 24. Creating Subjects

Add the subjects used by the institution.

Configure:

* name;
* course;
* type;
* classroom requirements.

If a subject requires a specific classroom category, configure that requirement.

---

## 25. Creating Groups

Add the student groups.

Configure:

* group name;
* course;
* number of students.

The student count is important because it affects classroom recommendations.

---

## 26. Verifying Reference Data

Before generating a timetable, verify:

* teacher names;
* teacher-subject assignments;
* subject names;
* subject requirements;
* group names;
* student counts;
* classroom categories;
* classroom capacities.

Incorrect reference data can result in incorrect schedule generation.

---

## 27. Schedule Generation

Once the reference data is ready:

1. Open the schedule generation functionality.
2. Review the generation settings.
3. Start generation.
4. Wait for the generation process to complete.
5. Review the generated schedule.

The generator uses available teachers, groups, subjects and classrooms.

---

## 28. Reviewing the Generated Schedule

After generation, review the timetable using the schedule filters.

Check:

* teachers;
* groups;
* classrooms;
* subjects;
* days;
* times.

Pay particular attention to conflicts and unexpected assignments.

---

## 29. Manual Schedule Corrections

Individual schedule entries can be edited when required.

Before saving a correction, the application validates the schedule.

Possible validation issues include:

* teacher overlap;
* group overlap;
* classroom overlap;
* invalid time range;
* insufficient classroom capacity;
* incompatible classroom category.

---

## 30. Exporting the Schedule

After reviewing the timetable, export the final schedule.

Available formats include:

```text
PDF
Excel
CSV
```

Recommended use:

| Format | Recommended purpose  |
| ------ | -------------------- |
| PDF    | Printing and sharing |
| Excel  | Editing and analysis |
| CSV    | Data processing      |

---

## 31. Production Configuration

For production deployment, configuration should be separated from development settings.

Production configuration should include:

* production database connection;
* secure administrator password;
* appropriate logging;
* HTTPS;
* secure PostgreSQL credentials.

Secrets must not be committed to the Git repository.

---

## 32. Environment Variables

Environment variables can be used for sensitive configuration.

For example:

```powershell
$env:Admin__Password="YOUR_STRONG_PASSWORD"
```

The application can then read the value through the ASP.NET Core configuration system.

For a persistent Windows environment variable, configure it through the operating system rather than storing the secret in the source repository.

---

## 33. Database Backup Before Deployment

Before applying major migrations or deploying a new version, create a PostgreSQL backup.

A typical PostgreSQL backup can be created using:

```powershell
pg_dump -U postgres -d schedulesystem -F c -f schedulesystem_backup.dump
```

The exact command may need to be adjusted according to the local PostgreSQL installation.

Store backups securely.

---

## 34. Database Restore

If a database needs to be restored, use PostgreSQL's restore tools appropriate for the backup format.

For a custom-format backup created with `pg_dump -F c`, a typical restore command is:

```powershell
pg_restore -U postgres -d schedulesystem schedulesystem_backup.dump
```

Before restoring a production database, verify the backup and target environment.

---

## 35. Updating the Application

When a new version is available:

```powershell
git pull origin main
```

Then restore dependencies:

```powershell
dotnet restore
```

Build the application:

```powershell
dotnet build
```

Apply migrations if required:

```powershell
dotnet ef database update
```

Finally start the application:

```powershell
dotnet run
```

---

## 36. Recommended Update Workflow

The complete update workflow is:

```text
Backup Database
      ↓
Pull New Version
      ↓
Restore Dependencies
      ↓
Build
      ↓
Apply Migrations
      ↓
Start Application
      ↓
Test
      ↓
Verify Data
```

Do not skip the database backup before major schema changes.

---

## 37. Deployment Verification

After deployment, verify the following:

### Application

* [ ] Application starts successfully.
* [ ] No startup errors occur.
* [ ] Main page opens.

### Authentication

* [ ] Login works.
* [ ] Administrator account works.
* [ ] Authorization works.

### Database

* [ ] PostgreSQL is accessible.
* [ ] Database connection works.
* [ ] Migrations are applied.

### Main Features

* [ ] Teachers open correctly.
* [ ] Groups open correctly.
* [ ] Subjects open correctly.
* [ ] Classrooms open correctly.
* [ ] Schedule opens correctly.
* [ ] Schedule validation works.
* [ ] Schedule generation works.
* [ ] Notifications work.
* [ ] Export works.

---

## 38. Build Verification

Run:

```powershell
dotnet build
```

The expected result is:

```text
Build succeeded.
```

The deployment should not proceed if the project contains build errors.

---

## 39. Troubleshooting

### `dotnet` command is not recognized

Install the .NET 8 SDK and restart the terminal.

Verify:

```powershell
dotnet --version
```

---

### `dotnet ef` command is not recognized

Install the Entity Framework Core CLI:

```powershell
dotnet tool install --global dotnet-ef
```

Then restart the terminal if necessary.

---

### PostgreSQL connection error

Check:

* PostgreSQL service;
* host;
* port;
* database name;
* username;
* password;
* connection string.

The default PostgreSQL port is commonly:

```text
5432
```

---

### Database does not exist

Create:

```text
schedulesystem
```

Then run:

```powershell
dotnet ef database update
```

---

### Migration fails

Check:

1. Database connection.
2. PostgreSQL permissions.
3. Current migration state.
4. Application configuration.
5. Database backup.

Do not manually modify migration history unless you fully understand the consequences.

---

### Administrator account is not initialized

Check that:

```text
Admin__Password
```

is available to the application.

For the current PowerShell session:

```powershell
$env:Admin__Password="YOUR_STRONG_PASSWORD"
```

Then restart the application.

---

### Application starts but pages fail

Check:

* database connection;
* migrations;
* application logs;
* browser developer console;
* server terminal output.

---

## 40. Security Checklist

Before exposing the application to other users:

* [ ] Use a strong administrator password.
* [ ] Do not store passwords in source code.
* [ ] Do not commit secrets to Git.
* [ ] Protect PostgreSQL credentials.
* [ ] Use HTTPS.
* [ ] Restrict database access.
* [ ] Back up the database.
* [ ] Limit administrator access.
* [ ] Keep dependencies updated.

---

## 41. Development vs Production

### Development

Typical command:

```powershell
dotnet run
```

Development commonly uses:

* local PostgreSQL;
* local configuration;
* developer tools;
* local testing.

### Production

Production should use:

* secure database credentials;
* protected configuration;
* HTTPS;
* database backups;
* controlled access;
* monitoring and logging.

The exact production hosting configuration depends on the target environment.

---

## 42. Deployment Architecture

A basic deployment can be represented as:

```text
┌───────────────────────────────┐
│          Client PC            │
│                               │
│       Web Browser             │
└───────────────┬───────────────┘
                │
                │ HTTP / HTTPS
                ▼
┌───────────────────────────────┐
│       ASP.NET Core App        │
│                               │
│       ScheduleSystem          │
└───────────────┬───────────────┘
                │
                │ Npgsql
                ▼
┌───────────────────────────────┐
│          PostgreSQL           │
│                               │
│        schedulesystem         │
└───────────────────────────────┘
```

---

## 43. Recommended Deployment Sequence

The complete installation sequence is:

```text
Install Windows
      ↓
Install .NET 8 SDK
      ↓
Install PostgreSQL
      ↓
Install Git
      ↓
Clone ScheduleSystem
      ↓
Configure Database
      ↓
Configure Admin__Password
      ↓
Restore Dependencies
      ↓
Apply EF Core Migrations
      ↓
Build Application
      ↓
Run Application
      ↓
Login
      ↓
Configure Initial Data
      ↓
Generate Schedule
      ↓
Verify
```

---

## 44. Maintenance

Regular maintenance should include:

* database backups;
* dependency updates;
* application updates;
* migration checks;
* build verification;
* review of audit logs;
* verification of schedule data.

Before major updates, create a database backup.

---

## 45. Final Deployment Checklist

### Software

* [ ] Windows installed.
* [ ] .NET 8 SDK installed.
* [ ] PostgreSQL installed.
* [ ] Git installed.

### Application

* [ ] Repository cloned.
* [ ] Dependencies restored.
* [ ] Configuration completed.
* [ ] Administrator password configured.
* [ ] Application builds successfully.

### Database

* [ ] `schedulesystem` database created.
* [ ] Connection configured.
* [ ] EF Core migrations applied.
* [ ] Database backup created.

### Verification

* [ ] Application starts.
* [ ] Administrator can log in.
* [ ] Dashboard works.
* [ ] Reference data can be managed.
* [ ] Schedule works.
* [ ] Validation works.
* [ ] Generation works.
* [ ] Notifications work.
* [ ] Export works.

---

## 46. Conclusion

ScheduleSystem can be deployed on a Windows computer using a standard ASP.NET Core and PostgreSQL setup.

The essential deployment process is:

```text
.NET 8
  +
PostgreSQL
  +
ScheduleSystem
  +
EF Core Migrations
  +
Secure Configuration
      ↓
Working ScheduleSystem Installation
```

The application uses Entity Framework Core migrations for database management and ASP.NET Core Identity for authentication and authorization.

Following this guide provides a repeatable deployment process while keeping application credentials and database information outside the source code.
