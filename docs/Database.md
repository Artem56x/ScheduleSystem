# ScheduleSystem — Database Documentation

## 1. Overview

ScheduleSystem uses PostgreSQL as its relational database management system.

Entity Framework Core is used as the Object-Relational Mapping (ORM) framework.

The application uses the Code First approach:

```text
C# Models
    ↓
Entity Framework Core
    ↓
Migrations
    ↓
PostgreSQL Database
```

The database stores the main educational data, schedule information, user accounts, notifications and audit records.

---

## 2. Database

The main application database is:

```text
schedulesystem
```

The application connects to PostgreSQL through the Npgsql Entity Framework Core provider.

---

## 3. Main Entities

The database contains the following major application entities:

* Teachers
* Subjects
* TeacherSubjects
* Groups
* Classrooms
* ClassroomCategories
* Schedules
* ScheduleTimeSlots
* ScheduleGenerationSettings
* Notifications
* AuditLogs
* Identity tables

The exact physical schema is managed by Entity Framework Core migrations.

---

## 4. Entity Relationship Overview

The main application relationships can be represented as:

```text
                    ┌──────────────────┐
                    │     Teachers     │
                    └────────┬─────────┘
                             │
                             │ 1:N
                             ↓
                    ┌──────────────────┐
                    │ TeacherSubjects  │
                    └────────┬─────────┘
                             │
                             │ N:1
                             ↓
                    ┌──────────────────┐
                    │     Subjects     │
                    └──────────────────┘


┌──────────────┐
│    Groups    │
└──────┬───────┘
       │
       │
       ↓
┌──────────────────┐
│    Schedules     │
└───┬────┬────┬────┘
    │    │    │
    │    │    └───────────────┐
    │    │                    │
    ↓    ↓                    ↓
Teacher Subject           Classroom


┌─────────────────────┐
│ ClassroomCategories │
└──────────┬──────────┘
           │
           ↓
     ┌────────────┐
     │ Classrooms │
     └────────────┘
```

---

## 5. Teachers

The `Teachers` entity stores information about teachers.

A teacher can teach multiple subjects.

The relationship with subjects is implemented through the `TeacherSubjects` junction table.

Conceptually:

```text
Teacher
  │
  ├── Subject 1
  ├── Subject 2
  └── Subject 3
```

This allows the system to represent many-to-many teacher-subject relationships.

---

## 6. Subjects

The `Subjects` entity stores information about academic subjects.

A subject contains information such as:

* name;
* course;
* subject type;
* classroom requirements.

Subjects can be associated with multiple teachers through `TeacherSubjects`.

---

## 7. TeacherSubjects

`TeacherSubjects` is a junction table between teachers and subjects.

Its main fields are:

```text
TeacherId
SubjectId
```

The table uses a composite primary key:

```text
(TeacherId, SubjectId)
```

This prevents the same teacher-subject relationship from being stored more than once.

### Relationship

```text
Teachers
   │
   │ 1:N
   ↓
TeacherSubjects
   ↑
   │ N:1
   │
Subjects
```

This design is preferable to storing a single `SubjectId` directly in the teacher record because one teacher can teach multiple subjects.

---

## 8. Groups

The `Groups` entity stores student group information.

Main information includes:

* group name;
* course;
* student count.

The student count is used by the schedule and classroom recommendation systems.

For example:

```text
Group: 25БД-1
Course: 2
Students: 24
```

The system can use the value `24` when selecting a suitable classroom.

---

## 9. Classrooms

The `Classrooms` entity stores information about available classrooms.

Main fields include:

* classroom name;
* category;
* capacity.

A classroom belongs to a classroom category.

Conceptually:

```text
Classroom
    │
    │ N:1
    ↓
ClassroomCategory
```

---

## 10. ClassroomCategories

`ClassroomCategories` defines the type or purpose of a classroom.

Examples include:

```text
Computer Classroom
Standard Classroom
Specialized Classroom
```

The exact categories are determined by the institution.

Subjects can define classroom category requirements.

This allows the system to recommend classrooms based on their purpose rather than relying on a simple boolean property.

---

## 11. Classroom Requirements

The application uses classroom categories to determine whether a classroom is appropriate for a subject.

The general logic is:

```text
Subject
   ↓
Required Classroom Category
   ↓
Available Classrooms
   ↓
Capacity Check
   ↓
Suitable Classrooms
```

For example:

```text
Subject:
Computer Graphics

Requirement:
Computer Classroom

Group:
25 students

Classroom:
Capacity = 30
Category = Computer Classroom

Result:
Suitable
```

---

## 12. Schedules

The `Schedules` entity stores individual timetable entries.

A schedule entry connects the main scheduling entities:

```text
Group
Teacher
Subject
Classroom
Day
Time
```

Conceptually:

```text
              ┌──────────┐
              │  Group   │
              └────┬─────┘
                   │
                   ↓
┌──────────┐  ┌──────────┐  ┌──────────┐
│ Teacher  │→ │ Schedule │ ←│ Subject  │
└──────────┘  └─────┬─────┘  └──────────┘
                    │
                    ↓
              ┌───────────┐
              │ Classroom │
              └───────────┘
```

Schedule data is used by:

* schedule views;
* validation;
* automatic generation;
* classroom recommendations;
* notifications;
* exports.

---

## 13. ScheduleTimeSlots

`ScheduleTimeSlots` stores reusable schedule time information.

Time slots allow the application to work with standardized lesson periods.

A time slot can represent a period such as:

```text
08:30 — 09:30
09:40 — 10:40
10:50 — 11:50
```

The exact schedule depends on the configured educational timetable.

---

## 14. Schedule Generation Settings

`ScheduleGenerationSettings` stores configuration used by the automatic schedule generator.

Important parameters include:

* lessons per week;
* maximum lessons per day;
* teaching days per week;
* maximum gaps per day;
* whether gaps are allowed.

Default values are:

| Parameter               | Default |
| ----------------------- | ------: |
| Lessons per week        |       3 |
| Maximum lessons per day |       4 |
| Teaching days per week  |       5 |
| Maximum gaps per day    |       2 |
| Allow gaps              |     Yes |

These values provide the generator with basic scheduling constraints.

---

## 15. Notifications

The `Notifications` entity stores user notifications.

Notifications can contain information related to schedule changes or other important system events.

A notification can be associated with a user.

Typical information may include:

```text
Title
Message
Created time
Read state
User
```

The notification system allows users to see important changes without manually checking every schedule entry.

---

## 16. AuditLogs

The `AuditLogs` entity stores information about important system activity.

Audit logs are useful for:

* monitoring administrative actions;
* investigating changes;
* troubleshooting;
* maintaining accountability.

An audit record may contain information such as:

```text
User
Action
Entity
Entity identifier
Timestamp
Additional information
```

The exact stored fields are defined by the current application model.

---

## 17. Identity Database

ScheduleSystem uses ASP.NET Core Identity.

Identity manages authentication and user accounts.

The database contains Identity-related tables for functionality such as:

* users;
* roles;
* claims;
* logins;
* tokens;
* user-role relationships.

The Identity subsystem is integrated with the application's `ApplicationDbContext`.

---

## 18. ApplicationDbContext

The application's database context is:

```text
ApplicationDbContext
```

It inherits from:

```text
IdentityDbContext<ApplicationUser>
```

This allows application entities and ASP.NET Core Identity entities to use the same database context.

The context contains `DbSet` definitions for the application's main entities.

Examples include:

```text
Teachers
Groups
Subjects
Classrooms
Schedules
ScheduleTimeSlots
ScheduleGenerationSettings
ClassroomCategories
Notifications
AuditLogs
TeacherSubjects
```

---

## 19. Primary Keys

Application entities use primary keys to uniquely identify records.

Most individual entities use an integer identifier:

```text
Id
```

The `TeacherSubjects` entity uses a composite primary key:

```text
TeacherId + SubjectId
```

This is appropriate for a many-to-many junction table.

---

## 20. Foreign Keys

Foreign keys maintain relationships between entities.

Examples:

```text
TeacherSubjects.TeacherId
        ↓
Teachers.Id
```

```text
TeacherSubjects.SubjectId
        ↓
Subjects.Id
```

```text
Classrooms.CategoryId
        ↓
ClassroomCategories.Id
```

Schedule records also contain relationships to the entities required to describe a lesson.

---

## 21. Referential Integrity

Foreign keys are used to maintain database consistency.

The database and Entity Framework Core configuration prevent invalid relationships from being stored where the configured constraints prohibit them.

This reduces the possibility of orphaned or inconsistent records.

---

## 22. Many-to-Many Teacher–Subject Relationship

The teacher-subject relationship is one of the most important database design decisions in ScheduleSystem.

A simple one-to-many design would allow a teacher to have only one subject:

```text
Teacher → Subject
```

This would not represent real teaching assignments correctly.

The implemented design uses:

```text
Teacher
   ↓
TeacherSubjects
   ↑
Subject
```

Therefore:

```text
One teacher → many subjects
One subject → many teachers
```

This is a standard relational many-to-many design.

---

## 23. Schedule Validation and the Database

The database stores the data required to validate schedule entries.

The validation layer can use:

* teacher;
* group;
* classroom;
* date/day;
* start time;
* end time;
* subject;
* classroom category;
* group size.

The database therefore provides the source data required by the scheduling rules.

---

## 24. Database Migrations

Entity Framework Core migrations are used to manage schema changes.

The project contains migrations for major database changes, including:

* Identity;
* initial application entities;
* notifications;
* audit logs;
* schedule time slots;
* generation settings;
* user notification settings;
* teacher-subject many-to-many relationships.

A migration represents a controlled change from one database schema version to another.

---

## 25. Applying Migrations

To apply existing migrations:

```powershell
dotnet ef database update
```

This updates the configured PostgreSQL database to the latest migration.

---

## 26. Creating a Migration

When the data model is intentionally changed during development:

```powershell
dotnet ef migrations add MigrationName
```

After creating the migration, apply it using:

```powershell
dotnet ef database update
```

A migration should only be created when the model has intentionally changed.

---

## 27. Checking for Pending Model Changes

The project can be checked for unexpected model changes using:

```powershell
dotnet ef migrations has-pending-model-changes
```

A clean project should not have unexpected pending model changes before a final build or deployment.

---

## 28. Database Backup

Before performing major database changes, administrators should create a PostgreSQL backup.

Backups are especially recommended before:

* applying structural migrations;
* deleting large amounts of data;
* deploying a major application version;
* changing production configuration.

---

## 29. Data Integrity Recommendations

To maintain database integrity:

1. Use official names for groups and subjects.
2. Keep student counts accurate.
3. Keep classroom capacities accurate.
4. Maintain correct classroom categories.
5. Keep teacher-subject assignments up to date.
6. Avoid unnecessary direct database modifications.
7. Use Entity Framework migrations for schema changes.
8. Back up important databases before structural changes.

---

## 30. Database Security

The database contains application and user information.

The following practices are recommended:

* use a strong PostgreSQL password;
* restrict database network access;
* do not commit database credentials to Git;
* use secure configuration;
* create regular backups;
* use separate credentials for production environments;
* limit database permissions where appropriate.

Administrator application credentials should also be kept outside the source code.

---

## 31. Database Architecture

The overall database architecture can be summarized as:

```text
                    PostgreSQL
                         │
                         │
                ApplicationDbContext
                         │
          ┌──────────────┴──────────────┐
          │                             │
     Application                    Identity
       Entities                     Entities
          │                             │
    ┌─────┼─────┐                 ┌────┴────┐
    │     │     │                 │         │
 Teachers Groups Subjects       Users      Roles
    │
 TeacherSubjects
    │
 Subjects
    │
 Schedules
    │
 Classrooms
    │
 ClassroomCategories
```

---

## 32. Data Flow

A typical scheduling operation follows this flow:

```text
User Input
    ↓
MVC Controller
    ↓
Validation / Business Logic
    ↓
Entity Framework Core
    ↓
ApplicationDbContext
    ↓
PostgreSQL
```

When data is retrieved:

```text
PostgreSQL
    ↓
ApplicationDbContext
    ↓
Entity Framework Core
    ↓
Controller / Service
    ↓
ViewModel
    ↓
Razor View
    ↓
User
```

---

## 33. Database and Business Logic

The database is responsible for persistent data storage and relationships.

Business rules are handled by the application layer.

Important services include:

* `ScheduleValidationService`
* `ScheduleGeneratorService`
* `ClassroomRecommendationService`
* `ScheduleExportService`

This separation prevents complex scheduling logic from being placed directly into database tables.

---

## 34. Database and Export

Schedule information stored in PostgreSQL can be retrieved by the application and converted into different formats.

The general process is:

```text
PostgreSQL
    ↓
Entity Framework Core
    ↓
Schedule Services
    ↓
Export Service
    ↓
PDF / Excel / CSV
```

The database remains the primary source of schedule information.

---

## 35. Database Verification

Before releasing a new version, verify:

```powershell
dotnet build
```

Then check the EF Core model:

```powershell
dotnet ef migrations has-pending-model-changes
```

If the database schema needs to be updated:

```powershell
dotnet ef database update
```

---

## 36. Current Database Design

The current design focuses on:

* normalized relational data;
* explicit foreign-key relationships;
* many-to-many teacher-subject assignments;
* classroom categorization;
* schedule validation;
* Identity integration;
* notification storage;
* audit logging;
* migration-based schema management.

The design is intended to support further expansion of the scheduling system without requiring fundamental restructuring.

---

## 37. Future Database Improvements

Possible future improvements include:

* additional schedule constraint entities;
* more detailed curriculum structures;
* academic year and semester entities;
* historical schedule versions;
* advanced timetable templates;
* improved reporting tables;
* additional audit information;
* database-level performance optimization for larger institutions.

These changes are not required for the current project but may be useful if the application is expanded.

---

## 38. Conclusion

The ScheduleSystem database provides a structured foundation for timetable management.

The main design principles are:

1. Relational data storage using PostgreSQL.
2. Entity Framework Core for data access.
3. Code First migrations for schema management.
4. Explicit foreign-key relationships.
5. Many-to-many teacher-subject relationships.
6. Classroom categories instead of simple hardware flags.
7. Database-backed schedule information.
8. Integrated ASP.NET Core Identity.
9. Notification and audit data storage.
10. Separation between database storage and business logic.

This structure allows ScheduleSystem to maintain consistent data while supporting automatic schedule generation, validation, recommendations and export.
