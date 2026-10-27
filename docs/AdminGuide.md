# ScheduleSystem — Administrator Guide

## 1. Overview

This guide describes the administrative functionality of ScheduleSystem, a web application for managing college timetables.

The application provides tools for managing reference data, creating and validating schedule entries, generating timetables, recommending suitable classrooms, reviewing notifications, and monitoring system activity.

The administrator is responsible for:

- Managing teachers, subjects, student groups, and classrooms.
- Maintaining teacher–subject relationships.
- Configuring schedule generation.
- Reviewing generated timetables and validation messages.
- Managing schedule data and exports.
- Monitoring notifications and audit information.
- Maintaining secure application configuration.

The exact functionality available depends on the current application version and the permissions assigned to the authenticated user.

---

## 2. Administrator Account and Roles

ScheduleSystem uses ASP.NET Core Identity for authentication and authorization.

The application defines two main roles:

- `Admin` — access to administrative operations.
- `User` — access to functions permitted for regular users.

Administrative operations are protected by role-based authorization.

### Default administrator account

The application uses the following email address when initializing the administrator account:

```text
admin@schedulesystem.local
```

The application does not store the administrator password directly in the source code.

The password must be supplied through application configuration using:

```text
Admin__Password
```

This environment variable corresponds to the configuration key `Admin:Password`.

If the administrator account does not exist and the password is not configured, the initialization process reports an error.

**Security recommendation:** configure a strong password before starting the application. Never commit real passwords, connection strings containing credentials, or other secrets to the repository.

---

## 3. Recommended Administrative Workflow

Before generating a timetable, verify that the required reference data is configured correctly.

Recommended workflow:

```text
Classroom Categories
        ↓
Classrooms
        ↓
Teachers and Subjects
        ↓
Teacher–Subject Assignments
        ↓
Student Groups
        ↓
Schedule Configuration
        ↓
Schedule Generation
        ↓
Validation and Review
        ↓
Corrections and Export
```

The quality of the generated timetable depends on the accuracy and completeness of the source data.

---

## 4. Classroom Categories

Classroom categories describe the type of room and help determine whether a classroom is appropriate for a particular subject.

Examples include:

- Computer Classroom
- Standard Classroom
- Specialized Classroom

The available categories depend on the data configured in the application.

### Recommendations

Administrators should:

- Use clear and consistent category names.
- Avoid creating duplicate categories.
- Assign the appropriate category to each classroom.
- Verify subject requirements before generating a timetable.

Categories should reflect the actual facilities and requirements of the educational institution.

---

## 5. Classroom Management

Open **Аудитории** to view the classroom list.

Depending on the user's permissions, the application allows administrative operations such as creating, editing, and deleting classrooms.

A classroom contains the following important information:

- Name or room number.
- Classroom category.
- Capacity.

Example:

```text
Room: 305
Category: Computer Classroom
Capacity: 25
```

The capacity should represent the number of students the classroom can accommodate.

Before changing or deleting a classroom, review whether it is used by existing schedule entries.

---

## 6. Teacher Management

Open **Преподаватели** to manage teacher records.

Teacher information includes the teacher's name and, where configured, email address.

Administrative functionality includes:

- Creating teacher records.
- Editing teacher information.
- Viewing teacher details.
- Managing teacher–subject assignments.
- Removing records where permitted.

A teacher can be associated with multiple subjects through the `TeacherSubject` relationship.

Example:

```text
Teacher: Ivanov I.I.

Subjects:
- Programming
- Databases
- Web Development
```

Only assign subjects that the teacher is qualified and authorized to teach.

---

## 7. Teacher–Subject Assignments

Teacher–subject assignments are used when selecting teachers for schedule entries and when generating timetables.

When creating or editing a teacher, select the subjects that the teacher can teach.

Before schedule generation, verify that:

- The required subjects exist.
- Each teacher has the correct subject assignments.
- Assignments reflect the institution's actual teaching responsibilities.

Incorrect assignments may prevent the generator from finding a suitable timetable or may require manual correction.

---

## 8. Teacher–Subject Confirmation

ScheduleSystem includes a confirmation mechanism for certain situations where a teacher is selected for a subject that is not currently assigned to them.

When the application displays a confirmation warning:

1. Review the selected teacher and subject.
2. Verify the teacher's assignment.
3. Confirm the operation only if the assignment is intentional.
4. Correct the reference data if the selection is incorrect.

A confirmation warning should not be treated as proof that an assignment is valid. Administrators remain responsible for checking the teaching assignment.

---

## 9. Student Group Management

Open **Группы** to manage student groups.

Group information includes:

- Group name.
- Course.
- Number of students.

Example:

```text
Group: 25БД-1
Course: 2
Students: 24
```

The number of students is important when choosing a classroom.

For example, a classroom with a capacity of 20 is not suitable for a group of 24 students.

Keep group information up to date before generating or modifying a timetable.

---

## 10. Subject Management

Open **Предметы** to manage academic subjects.

Depending on the configured model, subject information may include:

- Subject name.
- Course.
- Subject type.
- Required classroom category.
- Assigned teachers.

Subject configuration affects the selection of teachers and classrooms.

Example:

```text
Subject: Computer Graphics
Required classroom category: Computer Classroom
```

Verify subject requirements before creating schedule entries or running automatic generation.

---

## 11. Subject Types

Subject types help distinguish subjects according to the institution's curriculum.

Select the appropriate type from the options available in the application.

The available values and their meaning depend on the current implementation and institutional configuration.

Do not assume that a subject type automatically enforces a scheduling restriction unless that behavior is implemented by the application.

---

## 12. Schedule Management

Open **Расписание** to view and manage timetable entries.

A schedule entry connects the following elements:

```text
Student Group
      +
Teacher
      +
Subject
      +
Classroom
      +
Day of Week
      +
Start and End Time
```

These values should be consistent with the institution's requirements.

The schedule page supports filtering by:

- Group.
- Teacher.
- Classroom.
- Subject.
- Day of the week.
- Time range.

Filters can be combined to find particular lessons or investigate scheduling issues.

---

## 13. Creating a Schedule Entry Manually

To create or modify a lesson:

1. Open **Расписание**.
2. Select the appropriate create or edit operation.
3. Select the student group.
4. Select the subject.
5. Select the teacher.
6. Select the classroom.
7. Specify the day of the week.
8. Set the start and end times.
9. Review any validation messages or confirmation warnings.
10. Save the entry if the application permits the operation.

Manual editing is useful for correcting individual lessons and handling exceptional situations.

Available fields and validation behavior depend on the current application version.

---

## 14. Schedule Validation

ScheduleSystem includes schedule validation functionality intended to help identify incompatible timetable entries.

The relevant checks may include:

### Time Validation

The start time must be earlier than the end time.

Invalid example:

```text
10:30 — 09:30
```

Valid example:

```text
09:30 — 10:30
```

### Teacher Conflicts

A teacher should not be assigned to overlapping lessons.

### Group Conflicts

A student group should not have two overlapping lessons.

### Classroom Conflicts

A classroom should not be assigned to multiple simultaneous lessons.

### Classroom Capacity

The classroom capacity should be sufficient for the selected group.

### Classroom Category

The classroom category should meet the subject's requirements.

**Important:** the presence of a validation rule, recommendation, or warning does not necessarily mean that every operation is blocked automatically. Review the messages displayed by the application and correct any identified issues before publishing the timetable.

---

## 15. Schedule Filters

The schedule page supports filtering by group, teacher, classroom, subject, day of the week, and time range.

Example: to review the lessons assigned to a particular teacher:

1. Open **Расписание**.
2. Select or enter the teacher.
3. Apply the filter.
4. Review the resulting lessons.

Combined filters can help identify conflicts, inspect individual groups, and review classroom usage.

---

## 16. Automatic Schedule Generation

ScheduleSystem includes an automatic timetable generator.

The generator uses the available application data and the supplied generation settings to search for a timetable.

Relevant inputs include:

- Selected student groups.
- Required teaching loads.
- Available teaching days.
- Available time slots.
- Maximum lessons per day.
- Maximum consecutive lessons.
- Lesson distribution preferences.
- Classroom recommendation settings.

The actual result depends on the available teachers, subjects, classrooms, group requirements, and scheduling constraints.

Automatic generation does not guarantee a complete timetable when the configured requirements cannot all be satisfied.

---

## 17. Schedule Generation Settings

The generator provides settings for controlling the generation process.

The `ScheduleGenerationRequest` model defines the following default values:

| Parameter | Default value |
|---|---:|
| Maximum lessons per day | 4 |
| Maximum consecutive lessons | 2 |
| Distribute lessons evenly | Enabled |
| Use classroom recommendations | Enabled |

The request model also contains collections for selected groups, teaching days, teaching loads, and time slots.

These values describe the defaults defined by the request model. The final settings used during generation may differ if the user changes them in the interface or if the application applies additional configuration.

Do not assume that every default is enforced independently of the supplied generation request.

---

## 18. Recommended Generation Workflow

Before starting automatic generation:

1. Verify classroom categories and capacities.
2. Verify classroom records.
3. Verify teacher records.
4. Check teacher–subject assignments.
5. Verify subject requirements.
6. Check student group names and student counts.
7. Select the required groups and teaching loads.
8. Configure teaching days and time slots.
9. Review the generation settings.
10. Start generation.
11. Inspect the generated timetable.
12. Correct any remaining issues and verify the final schedule.

If generation fails or produces an incomplete result, review the configured requirements and available resources before changing the data.

---

## 19. Classroom Recommendations

ScheduleSystem includes a classroom recommendation service.

The purpose of this service is to help identify classrooms that may be appropriate for a lesson.

Relevant factors include:

- Subject requirements.
- Group size.
- Classroom capacity.
- Classroom category.
- Classroom availability.

Example:

```text
Group size: 28

Classroom A:
Capacity: 20
Result: Insufficient capacity

Classroom B:
Capacity: 30
Category: Matches the requirement
Result: Potentially suitable
```

A recommendation should be reviewed in the context of the complete timetable and the institution's requirements.

A recommended classroom should not be assumed to be reserved until the corresponding schedule entry has been successfully saved.

---

## 20. Investigating Schedule Conflicts

When a conflict or validation warning appears, identify the affected resource and review the relevant schedule entries.

### Teacher conflict

Check whether the teacher has another lesson during the same period.

### Group conflict

Check whether the group has another lesson during the same period.

### Classroom conflict

Check whether the classroom is already assigned to another lesson.

### Capacity issue

Compare the group size with the classroom capacity.

### Category issue

Verify the subject's classroom requirements and the classroom category.

### Generation issue

Review teaching loads, teacher–subject assignments, available time slots, selected groups, and generation settings.

After making corrections, review the resulting timetable again.

---

## 21. Notifications

ScheduleSystem includes a notification system for communicating relevant application events.

Users should review their notifications after important timetable changes.

Depending on the event and the current implementation, notifications may provide information about schedule-related activity.

The notification system is intended to make relevant changes easier to notice without repeatedly reviewing every schedule entry.

---

## 22. Audit Logs

ScheduleSystem includes audit logging functionality for monitoring important application activity.

Authorized administrators can use audit information to help:

- Investigate unexpected changes.
- Review recorded administrative activity.
- Troubleshoot application issues.
- Improve accountability.

Audit logs should be treated as a diagnostic resource. Their exact contents depend on which operations are recorded by the current implementation.

---

## 23. Schedule Export

ScheduleSystem includes a schedule export service using the libraries `QuestPDF` and `ClosedXML`.

These libraries support PDF and Excel document generation, respectively.

Use the export options available in the application interface.

### PDF

Suitable for printing and distributing a formatted timetable when the PDF export option is available.

### Excel

Suitable for spreadsheet-based review and further processing when the Excel export option is available.

### CSV

CSV export should only be documented as an available feature if a corresponding export action is implemented and accessible in the current application version.

Do not assume that every export format supports the same filters or data scope.

After exporting, open the resulting file and verify that the expected lessons and timetable information are present.

---

## 24. Data Management Recommendations

Periodically verify the accuracy of:

- Teacher names and contact details.
- Subject names and requirements.
- Student group names.
- Student counts.
- Classroom names.
- Classroom categories.
- Classroom capacities.
- Teacher–subject assignments.
- Existing schedule entries.

Accurate reference data improves the reliability of schedule validation, classroom recommendations, and automatic generation.

---

## 25. Deleting Data

Before deleting a teacher, subject, group, or classroom, review whether the record is referenced by existing schedule entries or related records.

Depending on the entity and its relationships, the application may reject the operation or require related data to be addressed first.

Recommended procedure:

1. Identify the record to be deleted.
2. Review its related schedule entries.
3. Determine whether the dependent data must be reassigned or removed.
4. Perform the operation only when the resulting data state is acceptable.
5. Verify the affected timetable after the change.

Do not assume that deleting a reference record automatically removes all related records.

---

## 26. Authentication and Security

ScheduleSystem uses ASP.NET Core Identity and role-based authorization.

Administrators should follow these practices:

- Use a strong, unique administrator password.
- Configure secrets outside the source code.
- Do not commit passwords or production credentials to Git.
- Restrict administrator access to authorized users.
- Use HTTPS in deployed environments.
- Protect the PostgreSQL database and its credentials.
- Keep application dependencies and the hosting environment maintained.

The environment variable for the initial administrator password is:

```text
Admin__Password
```

Production deployments should use an appropriate secrets-management mechanism and environment-specific configuration.

---

## 27. Database Migrations

ScheduleSystem uses Entity Framework Core migrations to manage database schema changes.

Before applying migrations to an important environment, back up the database and verify the target connection.

To apply available migrations:

```powershell
dotnet ef database update
```

To create a migration during development:

```powershell
dotnet ef migrations add MigrationName
```

The commands should be executed from the appropriate project directory with the required Entity Framework tooling and database configuration.

Do not create or apply migrations blindly against a production database.

---

## 28. Application Configuration

The application uses the ASP.NET Core configuration system.

Configuration includes database connection settings and the administrator password.

The administrator password is read using the configuration key:

```text
Admin:Password
```

The corresponding environment variable is:

```text
Admin__Password
```

The database connection must be configured for the target environment before the application starts.

Never commit real credentials to the repository. Use environment variables, user secrets during development, or an appropriate secrets-management service.

---

## 29. Build Verification

Before deploying or submitting a new version, verify that the project builds successfully.

Run:

```powershell
dotnet restore
dotnet build
```

If the project uses Entity Framework Core tooling, the following command can be used to check for pending model changes:

```powershell
dotnet ef migrations has-pending-model-changes
```

The last command requires a compatible Entity Framework Core version and the required tooling.

A successful build does not replace runtime testing. Verify the database connection and the main application workflows separately.

---

## 30. Deployment Checklist

Before deploying a new version:

- [ ] Back up the database.
- [ ] Retrieve the intended application version.
- [ ] Configure the database connection.
- [ ] Configure the administrator password and other secrets.
- [ ] Verify database connectivity.
- [ ] Apply required migrations.
- [ ] Restore dependencies.
- [ ] Build the application.
- [ ] Start the application and inspect logs.
- [ ] Test authentication and authorization.
- [ ] Test the dashboard and reference-data pages.
- [ ] Test schedule viewing and filtering.
- [ ] Test manual schedule creation and editing.
- [ ] Test schedule validation.
- [ ] Test automatic generation.
- [ ] Test the export formats available in the application.
- [ ] Verify administrative operations.
- [ ] Confirm that the deployed environment uses secure configuration.

---

## 31. Troubleshooting

### Application fails to start

Check:

- Installed .NET runtime and SDK requirements.
- Application configuration.
- Database availability.
- Startup and application logs.
- Required environment variables.

### Database connection fails

Check:

- PostgreSQL service status.
- Server address and port.
- Database name.
- Database credentials.
- Connection string configuration.
- Network access and database permissions.

### Migration fails

Check:

- The target database connection.
- Migration history.
- Database permissions.
- The current schema.
- Compatibility between the application model and existing database.

Back up important data before attempting corrective schema changes.

### Schedule generation does not produce the expected result

Check:

- Selected groups.
- Teaching loads.
- Teacher–subject assignments.
- Classroom capacities.
- Classroom categories.
- Subject requirements.
- Available teaching days and time slots.
- Maximum lessons per day.
- Maximum consecutive lessons.
- Existing schedule entries.
- Classroom recommendation settings.

The requested timetable may be impossible to generate if there are insufficient teachers, classrooms, or available time slots.

### A teacher cannot be assigned to a subject

Verify that the teacher–subject relationship is configured correctly.

If the application displays a confirmation warning, review the warning and confirm the operation only when the assignment is intentional.

### A schedule entry cannot be saved

Review validation messages and confirmation prompts.

Check the selected group, teacher, subject, classroom, and time range. Investigate overlapping entries and any unmet classroom requirements.

---

## 32. Recommended Daily Workflow

A recommended administrative workflow is:

```text
Login
  ↓
Review Notifications
  ↓
Check Reference Data
  ↓
Review the Timetable
  ↓
Make Required Corrections
  ↓
Review Validation Messages
  ↓
Verify the Final Timetable
  ↓
Export the Required Version
  ↓
Review Audit Information When Necessary
```

This workflow helps maintain consistent reference data and reduces the risk of publishing an incorrect timetable.

---

## 33. Administrator Best Practices

Administrators should:

1. Keep reference data accurate.
2. Maintain correct teacher–subject assignments.
3. Update group sizes when necessary.
4. Keep classroom capacities and categories accurate.
5. Review generation settings before running the generator.
6. Inspect generated timetables before publication.
7. Investigate validation warnings and conflicts.
8. Back up the database before important changes.
9. Protect administrator credentials.
10. Test important changes before deployment.

---

## 34. System Maintenance

Regular maintenance should include:

- Reviewing outdated reference data.
- Checking database backups.
- Verifying application builds.
- Reviewing relevant audit events.
- Checking timetable consistency.
- Updating dependencies when appropriate.
- Monitoring application logs.
- Maintaining secure production configuration.

Maintenance procedures should be adapted to the deployment environment and the institution's requirements.

---

## 35. Final Timetable Checklist

Before publishing a timetable, verify:

- [ ] All required teachers are present.
- [ ] Teacher–subject assignments are correct.
- [ ] All required student groups are present.
- [ ] Student counts are up to date.
- [ ] Subjects are configured correctly.
- [ ] Classroom categories are correct.
- [ ] Classroom capacities are sufficient.
- [ ] Teacher conflicts have been investigated.
- [ ] Group conflicts have been investigated.
- [ ] Classroom conflicts have been investigated.
- [ ] Classroom requirements have been reviewed.
- [ ] Validation messages have been addressed.
- [ ] The final timetable has been reviewed.
- [ ] The required export has been opened and checked.

---

## 36. Conclusion

ScheduleSystem provides administrative tools for managing timetable-related data and workflows.

Its documented functionality includes:

- Teacher and subject management.
- Student group management.
- Classroom and category management.
- Teacher–subject assignments.
- Manual schedule management.
- Schedule filtering and validation.
- Automatic schedule generation.
- Classroom recommendations.
- Notifications and audit logging.
- Schedule export functionality.

Administrators should maintain accurate reference data, review validation messages, protect credentials, and verify the final timetable before distributing it.

This guide should be kept synchronized with the application's current source code and the features exposed by the deployed version.
