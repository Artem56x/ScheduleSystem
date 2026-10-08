# ScheduleSystem — Administrator Guide

## 1. Overview

This guide describes the administrative functionality of ScheduleSystem.

The administrator is responsible for:

* managing system data;
* configuring teachers and subjects;
* managing student groups;
* managing classrooms;
* generating schedules;
* reviewing schedule conflicts;
* managing notifications and system settings;
* monitoring system activity.

---

## 2. Administrator Account

ScheduleSystem uses ASP.NET Core Identity for authentication and authorization.

The application supports two main roles:

* `User`
* `Admin`

The administrator has access to functions that are not available to regular users.

The default administrator email configured by the application is:

```text
admin@schedulesystem.local
```

The administrator password is not stored directly in the source code.

It must be provided through configuration or an environment variable:

```text
Admin__Password
```

---

## 3. Administrator Responsibilities

Before creating a timetable, the administrator should make sure that all required reference data is correct.

The recommended order is:

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
Schedule Generation
        ↓
Schedule Validation
        ↓
Export
```

Incorrect reference data can result in an invalid or incomplete timetable.

---

## 4. Classroom Categories

Classroom categories define the type of classroom.

Categories are used when determining whether a classroom is suitable for a subject.

For example:

```text
Computer Classroom
Standard Classroom
Specialized Classroom
```

The exact categories depend on the institution's requirements.

### Recommendations

Administrators should:

* avoid duplicate categories;
* use clear names;
* assign the correct category to each classroom;
* ensure that subject requirements use existing categories.

---

## 5. Classroom Management

Open:

**Аудитории**

Administrators can create, edit and remove classrooms.

Each classroom contains:

* name;
* category;
* capacity.

### Example

```text
Room: 305
Category: Computer Classroom
Capacity: 25
```

The classroom capacity should represent the actual number of students that can comfortably use the room.

---

## 6. Teacher Management

Open:

**Преподаватели**

Administrators can:

* add teachers;
* edit teachers;
* view teacher details;
* delete teachers;
* assign multiple subjects to teachers.

A teacher can teach multiple subjects.

The relationship is implemented as a many-to-many relationship:

```text
Teacher
   │
   ├── Subject A
   ├── Subject B
   └── Subject C
```

This allows the system to represent real teaching assignments more accurately.

---

## 7. Assigning Subjects to Teachers

When creating or editing a teacher, select all subjects that the teacher can teach.

For example:

```text
Teacher: Иванов И.И.

Subjects:
[x] Programming
[x] Databases
[ ] Mathematics
[x] Web Development
```

Only subjects actually taught by the teacher should normally be selected.

The relationship is used by schedule validation and automatic schedule generation.

---

## 8. Teacher–Subject Confirmation

ScheduleSystem contains an additional confirmation mechanism when a teacher is selected for a subject they are not currently assigned to.

The system can display a warning before saving the schedule.

This behavior is intentional.

It allows the administrator to:

1. identify a potentially incorrect assignment;
2. review the warning;
3. explicitly confirm the assignment when necessary.

The confirmation mechanism should not be bypassed without a valid administrative reason.

---

## 9. Student Group Management

Open:

**Группы**

Administrators can manage student groups.

Each group contains:

* group name;
* course;
* number of students.

The number of students is particularly important because it affects classroom recommendations.

### Example

```text
Group: 25БД-1
Course: 2
Students: 24
```

A classroom with a capacity below 24 should not be selected for this group.

---

## 10. Subject Management

Open:

**Предметы**

Administrators can manage subjects.

A subject can contain:

* name;
* course;
* type;
* classroom category requirements;
* assigned teachers.

### Classroom Requirements

If a subject requires a special classroom, configure the appropriate classroom category.

For example:

```text
Subject: Computer Graphics
Required category:
Computer Classroom
```

This information is used by the classroom recommendation system.

---

## 11. Subject Types

Subjects can be classified by their type.

The type should correspond to the institution's curriculum and help administrators distinguish different kinds of academic subjects.

When creating a subject, select the appropriate type available in the system.

---

## 12. Schedule Management

Open:

**Расписание**

Administrators can create, edit and review schedule entries.

A schedule entry connects:

```text
Group
   +
Teacher
   +
Subject
   +
Classroom
   +
Day
   +
Time
```

All of these elements must be compatible.

---

## 13. Creating a Schedule Manually

To create a lesson manually:

1. Open **Расписание**.
2. Select the create option.
3. Select the group.
4. Select the teacher.
5. Select the subject.
6. Select the classroom.
7. Select the day.
8. Specify the start time.
9. Specify the end time.
10. Review validation messages.
11. Save the lesson.

Manual creation is useful for individual corrections or special cases.

---

## 14. Schedule Validation

Before saving a schedule entry, the system validates the available information.

Validation includes:

### Time Validation

The start time must be earlier than the end time.

Invalid:

```text
10:30 — 09:30
```

Valid:

```text
09:30 — 10:30
```

### Teacher Conflicts

A teacher cannot normally be assigned to overlapping lessons.

### Group Conflicts

A group cannot normally attend two lessons at the same time.

### Classroom Conflicts

A classroom cannot normally be occupied by multiple lessons at the same time.

### Classroom Capacity

The classroom must have sufficient capacity for the selected group.

### Classroom Category

The classroom should satisfy the subject's classroom requirements.

---

## 15. Schedule Filters

The schedule page provides filtering functionality.

Administrators can filter by:

* group;
* teacher;
* classroom;
* subject;
* day;
* time.

Combined filters can be used to investigate specific timetable situations.

### Example

To find all lessons for a particular teacher:

1. Open **Расписание**.
2. Select the teacher.
3. Apply the filter.
4. Review the resulting lessons.

---

## 16. Automatic Schedule Generation

ScheduleSystem provides automatic timetable generation.

The generator uses the available system data and configured restrictions to produce a timetable.

The generation process considers:

* groups;
* subjects;
* teachers;
* classrooms;
* classroom categories;
* classroom capacities;
* teaching days;
* maximum daily lessons;
* gaps.

---

## 17. Generator Default Settings

The default generator settings are:

| Parameter               | Value |
| ----------------------- | ----: |
| Lessons per week        |     3 |
| Maximum lessons per day |     4 |
| Teaching days per week  |     5 |
| Maximum gaps per day    |     2 |
| Allow gaps              |   Yes |

These settings provide a starting point and can be adjusted according to institutional requirements where supported by the application.

---

## 18. Recommended Generation Workflow

Before starting automatic generation:

### Step 1

Verify all classrooms.

### Step 2

Verify classroom categories.

### Step 3

Verify teachers.

### Step 4

Verify teacher-subject assignments.

### Step 5

Verify subjects.

### Step 6

Verify groups and student counts.

### Step 7

Review existing schedule entries.

### Step 8

Start schedule generation.

### Step 9

Review the generated timetable.

### Step 10

Correct individual entries if necessary.

---

## 19. Classroom Recommendations

The classroom recommendation system helps select suitable rooms.

The recommendation logic considers:

```text
Subject requirements
        +
Group size
        +
Classroom capacity
        +
Classroom category
        +
Availability
```

For example:

```text
Group size: 28

Classroom A:
Capacity = 20
→ Not suitable

Classroom B:
Capacity = 30
Category = required
→ Suitable
```

---

## 20. Schedule Conflicts

If a conflict occurs, administrators should identify which resource is involved.

### Teacher conflict

Check whether the teacher has another lesson at the same time.

### Group conflict

Check whether the group has another lesson at the same time.

### Classroom conflict

Check whether the classroom is already occupied.

### Capacity conflict

Check the number of students and classroom capacity.

### Category conflict

Check the subject's classroom requirements and classroom category.

---

## 21. Notifications

ScheduleSystem provides notifications related to important schedule events.

Administrators should review notifications after significant schedule changes.

Notifications can contain information about:

* subject;
* group;
* scheduled time;
* changes to schedule information.

The notification system helps users notice timetable changes without manually checking every schedule entry.

---

## 22. Audit Logs

ScheduleSystem contains audit logging functionality.

Audit logs can be used to monitor important system activity.

They are useful for:

* reviewing administrative actions;
* investigating changes;
* troubleshooting;
* maintaining accountability.

Administrators should use audit information when investigating unexpected changes to system data.

---

## 23. Schedule Export

Administrators can export schedules into:

* PDF;
* Excel;
* CSV.

### PDF

Recommended for:

* printing;
* sharing;
* final timetable presentation.

### Excel

Recommended for:

* further analysis;
* manual processing;
* spreadsheet workflows.

### CSV

Recommended for:

* data exchange;
* automated processing;
* importing data into compatible systems.

---

## 24. Data Management Recommendations

Administrators should periodically verify:

* teacher names;
* subject names;
* group names;
* student counts;
* classroom capacities;
* classroom categories;
* teacher-subject assignments.

Reference data should be updated when academic information changes.

---

## 25. Deleting Data

Before deleting a teacher, subject, group or classroom, verify whether it is used by existing schedule data.

Deleting reference information that is still required can affect related records or prevent normal schedule management.

When possible, review dependent schedule entries before performing destructive operations.

---

## 26. Authentication and Security

Administrators should follow basic security practices.

### Passwords

Administrator passwords must not be stored in source code.

The application uses configuration/environment variables for the administrator password.

### Recommended configuration

Use:

```text
Admin__Password
```

for the administrator password.

### Production

For a production environment:

* use a strong unique password;
* do not commit secrets to Git;
* protect configuration files;
* restrict administrator access;
* use HTTPS;
* keep the database protected.

---

## 27. Database Migrations

ScheduleSystem uses Entity Framework Core migrations.

After receiving a new version of the project, administrators/developers should verify that the database schema matches the application model.

To apply migrations:

```powershell
dotnet ef database update
```

To create a new migration during development:

```powershell
dotnet ef migrations add MigrationName
```

Before applying structural database changes to an important environment, create a database backup.

---

## 28. Application Configuration

Important application configuration is stored through the ASP.NET Core configuration system.

The database connection is configured through the application's connection settings.

Sensitive values should be supplied through secure configuration mechanisms rather than committed to the repository.

The administrator password is provided through:

```text
Admin__Password
```

---

## 29. Build Verification

Before deploying or submitting a new version, verify that the project builds successfully.

Run:

```powershell
dotnet build
```

The build should complete without errors.

For Entity Framework model verification:

```powershell
dotnet ef migrations has-pending-model-changes
```

The application should not have unexpected pending model changes before deployment.

---

## 30. Deployment Checklist

Before deploying a new version:

* [ ] Back up the database.
* [ ] Pull the latest project version.
* [ ] Verify configuration.
* [ ] Verify the database connection.
* [ ] Apply required migrations.
* [ ] Build the application.
* [ ] Start the application.
* [ ] Test authentication.
* [ ] Test the dashboard.
* [ ] Test schedule viewing.
* [ ] Test schedule creation.
* [ ] Test schedule validation.
* [ ] Test exports.
* [ ] Verify administrator functions.

---

## 31. Troubleshooting

### Application fails to start

Check:

* .NET installation;
* configuration;
* database connection;
* application logs.

### Database connection fails

Check:

* PostgreSQL status;
* database name;
* server address;
* port;
* username;
* password;
* connection string.

### Migration fails

Check:

* current database state;
* migration history;
* database permissions;
* connection settings.

### Schedule generation does not produce the expected result

Check:

* teacher-subject assignments;
* group sizes;
* classroom capacities;
* classroom categories;
* subject requirements;
* existing schedule entries;
* generator settings.

### A teacher cannot be assigned to a subject

Verify that the subject is assigned to the teacher.

If the assignment is intentional, review the confirmation warning provided by the application.

---

## 32. Recommended Administrator Workflow

The recommended daily workflow is:

```text
Login
  ↓
Review notifications
  ↓
Check reference data
  ↓
Review schedule
  ↓
Make required corrections
  ↓
Validate schedule
  ↓
Export final version
  ↓
Review audit information when necessary
```

---

## 33. Administrator Best Practices

Administrators should:

1. Keep reference data accurate.
2. Maintain correct teacher-subject relationships.
3. Keep group student counts up to date.
4. Keep classroom capacities accurate.
5. Use classroom categories consistently.
6. Review generated schedules before publishing them.
7. Maintain database backups.
8. Protect administrator credentials.
9. Avoid committing secrets to Git.
10. Test important changes before deployment.

---

## 34. System Maintenance

Regular maintenance should include:

* reviewing outdated reference data;
* checking database backups;
* verifying application builds;
* reviewing important audit events;
* checking schedule consistency;
* updating dependencies when necessary;
* keeping the production environment secure.

---

## 35. Final Checklist

Before publishing a timetable, verify:

* [ ] All teachers are present.
* [ ] Teacher-subject assignments are correct.
* [ ] All groups are present.
* [ ] Student counts are correct.
* [ ] All subjects are configured.
* [ ] Classroom categories are correct.
* [ ] Classroom capacities are correct.
* [ ] No teacher conflicts exist.
* [ ] No group conflicts exist.
* [ ] No classroom conflicts exist.
* [ ] Classroom requirements are satisfied.
* [ ] The final timetable has been reviewed.
* [ ] A final export has been created.

---

## 36. Conclusion

The administrator is responsible for maintaining accurate reference data and ensuring that generated schedules comply with the institution's requirements.

ScheduleSystem provides tools for:

* centralized data management;
* teacher-subject assignment;
* classroom management;
* automatic schedule generation;
* schedule validation;
* classroom recommendations;
* notifications;
* audit logging;
* schedule export.

Following the recommended administrative workflow helps maintain a reliable and consistent timetable.
