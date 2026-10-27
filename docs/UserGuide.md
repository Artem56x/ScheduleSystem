# ScheduleSystem — User Guide

## 1. Overview

ScheduleSystem is a web application for managing and generating college class schedules.

The application provides functionality for working with:

- Class schedules
- Teachers and their subjects
- Student groups
- Subjects and classroom requirements
- Classrooms and classroom categories
- Schedule validation
- Automatic schedule generation
- Notifications and account management
- Schedule exports

The system is designed to centralize timetable management, reduce manual scheduling work, and help identify scheduling conflicts.

Some functions are available only to users with the appropriate permissions.

---

## 2. Getting Started

Start the application and open it in a web browser.

After opening the application, use the main navigation to access the available sections:

- **Главная** — dashboard and system overview.
- **Расписание** — view, filter, and manage schedule entries.
- **Преподаватели** — manage teacher records and subject assignments.
- **Группы** — manage student groups.
- **Предметы** — manage subjects and their requirements.
- **Аудитории** — manage classrooms, categories, and capacities.

Additional account functions are available through the account or profile interface.

The exact actions available on each page depend on the user's permissions and the application's current configuration.

---

## 3. Dashboard

The **Главная** page provides an overview of the application.

Depending on the current implementation and available data, the dashboard may display summary information about:

- Teachers
- Student groups
- Subjects
- Classrooms
- Scheduled lessons

Use the dashboard to get a quick overview before opening a specific management section.

If expected information is missing, check that the application is connected to the correct database and that the required records have been created.

---

## 4. Working with the Schedule

Open **Расписание** to view the timetable.

The schedule page provides a list of schedule entries and filtering options.

### 4.1 Schedule Filters

Available filters include:

- Student group
- Teacher
- Classroom
- Subject
- Day of the week
- Start and end time

Filters can be combined to narrow the results.

For example, selecting a group and a day of the week helps display the lessons scheduled for that group on that day.

To review the complete timetable, clear any filters that are no longer needed.

### 4.2 Reviewing Schedule Entries

When reviewing a lesson, check the following information:

- Group
- Teacher
- Subject
- Classroom
- Day of the week
- Start time
- End time

Verify the selected values before saving changes or exporting the timetable.

---

## 5. Creating a Schedule Entry

If your account has permission to manage schedule entries, follow these steps:

1. Open **Расписание**.
2. Select the action for adding a schedule entry.
3. Select a student group.
4. Select a teacher.
5. Select a subject.
6. Select a classroom.
7. Choose the day of the week.
8. Enter the start time.
9. Enter the end time.
10. Review the information and save the entry.

The application validates schedule data and checks for relevant conflicts.

If a validation message appears, read it before proceeding. Correct the invalid information or follow the application's confirmation process when an explicitly confirmable warning is shown.

The exact behavior depends on the validation result and the operation being performed.

---

## 6. Schedule Validation

ScheduleSystem includes a schedule validation service that checks schedule entries for common problems.

Validation may cover:

- Valid day and time values
- Start and end time consistency
- Teacher scheduling conflicts
- Group scheduling conflicts
- Classroom scheduling conflicts
- Overlapping lessons
- Teacher-subject assignments
- Classroom capacity
- Classroom category requirements
- Related group, teacher, subject, and classroom records

### Example: Teacher Conflict

Suppose a teacher already has a lesson from `08:30` to `09:30`.

Assigning the same teacher to another lesson during an overlapping period can create a scheduling conflict.

The validation service checks relevant constraints and reports problems according to the implemented validation rules.

Some warnings may allow an authorized user to confirm an operation. Do not assume that every warning can be ignored or that every invalid entry can be saved.

---

## 7. Automatic Schedule Generation

ScheduleSystem includes an automatic schedule generator.

The generator attempts to build a timetable using available data and configured restrictions, including:

- Student groups
- Subjects and required lesson counts
- Teacher-subject assignments
- Classrooms and their categories
- Teaching days and time slots
- Scheduling constraints
- Classroom recommendations, when enabled

### 7.1 Preparing for Generation

Before starting generation:

1. Add the required classrooms and classroom categories.
2. Create teacher records.
3. Assign subjects to teachers.
4. Create the required subjects and their relationships with groups.
5. Add student groups and enter their student counts.
6. Review the available scheduling settings.
7. Open the schedule generation interface.

The generator may fail to produce a complete timetable when the source data is incomplete or the constraints cannot be satisfied.

### 7.2 Generation Settings

Generation settings depend on the values configured in the generation interface.

They may include:

- Teaching days
- Available time slots
- Maximum lessons per day
- Maximum consecutive lessons
- Lesson distribution preferences
- Classroom recommendation options

Do not assume that every setting has the same default value across all parts of the application. Review the values displayed by the generation form before starting the process.

### 7.3 Preview and Confirmation

The generation workflow supports previewing the proposed timetable before saving it.

Recommended procedure:

1. Configure the generation settings.
2. Start generation.
3. Review the generated timetable.
4. Check the proposed entries for conflicts and unsuitable assignments.
5. Confirm and save the result if it meets the requirements.
6. Open **Расписание** to review the saved timetable.

Generating a preview does not necessarily mean that the entries have already been saved. Complete the confirmation step when it is available.

If generation fails, review the input data, available time slots, teacher-subject assignments, classroom requirements, and scheduling restrictions.

---

## 8. Teachers

Open **Преподаватели** to manage teacher records.

Teachers can be associated with multiple subjects. These relationships help the application determine whether a teacher is suitable for a particular lesson.

### 8.1 Adding a Teacher

1. Open **Преподаватели**.
2. Select the action for adding a teacher.
3. Enter the required teacher information.
4. Assign the subjects taught by the teacher, if the form provides this option.
5. Review the information.
6. Save the record.

### 8.2 Editing a Teacher

1. Find the required teacher.
2. Open the edit action.
3. Update the relevant information.
4. Review the subject assignments.
5. Save the changes.

### 8.3 Teacher-Subject Validation

When a schedule entry is created or edited, the application checks the relationship between the selected teacher and subject.

If the teacher is not assigned to the selected subject, the application may report a warning or require confirmation, depending on the operation and validation rules.

Check the teacher's subject assignments before changing the schedule.

---

## 9. Student Groups

Open **Группы** to manage student groups.

A group record can include information such as:

- Group name
- Course
- Number of students
- Other descriptive information supported by the form

Use the official group name and enter the correct student count.

The course must fall within the range supported by the application.

Student counts are important because classroom recommendations use group size to help identify suitable rooms.

Before generating a timetable, verify that the group records are complete and accurate.

---

## 10. Subjects

Open **Предметы** to manage subjects.

Depending on the available fields, a subject may include:

- Subject name
- Course
- Subject type
- Required classroom categories
- Teacher assignments or related teacher records

Some subjects require specialized classrooms.

For example, a subject that requires computers can be associated with the **Компьютерный класс** category.

Review the subject requirements before generating a timetable. Incorrect requirements can prevent suitable classrooms from being selected.

---

## 11. Classrooms

Open **Аудитории** to manage classrooms.

A classroom record can contain:

- Classroom name or number
- Classroom category
- Capacity

The classroom category describes the room's intended use. Examples include standard classrooms, computer classrooms, and specialized rooms, depending on the categories configured in the application.

Enter the actual classroom identifier and its correct capacity.

Classroom information affects both manual scheduling and automatic classroom recommendations.

---

## 12. Classroom Recommendations

ScheduleSystem includes a classroom recommendation service.

Recommendations consider relevant information such as:

1. Classroom category requirements of the subject.
2. Number of students in the group.
3. Classroom capacity.
4. Classroom availability and scheduling conflicts.

### Example: Classroom Capacity

If a group has 25 students, a classroom with a capacity of 20 students is unsuitable for that group.

### Example: Classroom Category

If a subject requires a computer classroom, a room from an incompatible category should not be treated as suitable.

Recommendations are intended to assist scheduling decisions. Review the selected classroom and any validation messages before saving a timetable entry.

---

## 13. Notifications

ScheduleSystem includes a notification interface for schedule-related information.

Depending on the application's current implementation, notifications may contain details about a subject, group, scheduled time, or relevant changes.

Example notification:

> Компьютерная графика — 25БД-1  
> Время: 08:30–09:30

Open the notification interface to review available messages.

Notification availability and visibility may depend on the current implementation and the user's permissions. If a notification appears unrelated to your account, report the issue to the system administrator.

---

## 14. Schedule Export

ScheduleSystem provides schedule export functionality in **PDF and Excel formats**.

The available export actions depend on the current schedule page and the export options implemented by the application.

Typical file names may resemble:

```text
Общее_расписание.pdf
Общее_расписание.xlsx
```

An individual export may use a name similar to:

```text
Расписание_Название.xlsx
```

The actual file name depends on the export operation.

### 14.1 Choosing a Format

- **PDF** — suitable for viewing, sharing, and printing.
- **Excel** — suitable for spreadsheet-based review and further analysis.

CSV export should only be used if a CSV option is present in the current application version.

### 14.2 Recommended Export Procedure

1. Open **Расписание**.
2. Apply the required filters, if applicable.
3. Review the displayed schedule.
4. Select the appropriate export action.
5. Save the generated file.
6. Open the file and verify that the expected records are included.

Export the timetable after reviewing the final changes.

---

## 15. Account and Authentication

ScheduleSystem uses ASP.NET Core Identity for authentication and account management.

Depending on the application's configuration, users can access account-related pages to sign in, register, or manage supported account information.

Some pages and operations require authentication.

If sign-in fails, check the entered credentials and contact the administrator if the problem persists.

Never share account passwords or store them in public repositories.

---

## 16. Profile

The profile interface provides access to information associated with the current account.

Depending on the implemented profile page, it may display details such as the user's display name and email address.

The available profile actions depend on the current application version and account permissions.

If you need to change information that is not editable through the interface, contact the system administrator.

---

## 17. Settings

Available settings depend on the user's role and the application's implementation.

Administrative settings may include program-related configuration or other system management functions.

Before changing system-wide settings, verify what the setting controls and whether the change affects other users.

If a setting is unavailable, your account may not have the required permissions, or the functionality may not be implemented in the current version.

---

## 18. Roles and Permissions

ScheduleSystem supports role-based access control.

The main roles are:

- **User** — regular application user.
- **Admin** — administrator with access to protected management and configuration functions.

Administrative access is required for certain operations, including access to the schedule generation interface.

The exact permissions depend on the authorization rules implemented for each page and action.

Use administrator privileges only when necessary. Do not attempt to bypass access restrictions.

---

## 19. Working with Data

Consistent data entry improves searching, filtering, validation, and exporting.

### Group Names

Use the official group name used by the educational institution.

### Teacher Names

Enter teacher names in a consistent format.

### Subject Names

Use the official subject names from the curriculum.

### Classroom Names

Use the actual classroom number or identifier.

### Capacity and Categories

Enter accurate classroom capacities and select the appropriate classroom categories.

Review related records when editing or removing data. Existing schedule entries may depend on teachers, groups, subjects, or classrooms.

---

## 20. Recommended Setup Workflow

For a new installation, prepare reference data before generating a timetable.

A recommended order is:

```text
Classroom Categories
        ↓
Classrooms
        ↓
Teachers
        ↓
Subjects
        ↓
Groups
        ↓
Group-Subject and Teacher-Subject Relationships
        ↓
Generation Settings
        ↓
Schedule Generation
        ↓
Validation and Review
        ↓
Schedule Export
```

The exact order may vary depending on the required relationships and the available management forms.

The important requirement is that the generator has complete and consistent data before it starts.

---

## 21. Creating a Complete Timetable

Use the following workflow to create and review a timetable.

### Step 1 — Configure Classroom Categories

Check that the categories required by the institution are available.

### Step 2 — Add Classrooms

Create the classrooms and specify their categories and capacities.

### Step 3 — Add Teachers

Create teacher records and configure their subject assignments.

### Step 4 — Add Subjects

Create the subjects and specify their relevant course and classroom requirements.

### Step 5 — Add Groups

Create student groups and enter accurate student counts.

### Step 6 — Check Relationships

Verify that the required teacher-subject and group-subject relationships are configured.

### Step 7 — Generate the Schedule

Open the schedule generation interface, configure the available settings, and start generation.

### Step 8 — Review the Preview

Check the proposed timetable and address any reported issues.

### Step 9 — Confirm and Save

Confirm the result when the preview meets the requirements.

### Step 10 — Review and Export

Open the schedule page, verify the saved entries, and export the timetable in the required format.

---

## 22. Avoiding Common Problems

### A Teacher Is Not Available

Check whether the teacher already has a lesson during the selected period.

Also verify that the teacher is assigned to the selected subject when required.

### A Classroom Is Not Suitable

Check:

- Classroom category
- Classroom capacity
- Subject requirements
- Existing lessons in the same period

### A Group Has a Scheduling Conflict

Check whether the group already has another lesson at the selected time.

### A Teacher Is Not Assigned to the Subject

Review the teacher-subject relationship.

If the application presents a confirmation warning, follow the displayed instructions and confirm only when the assignment is intentional and authorized.

### Schedule Entries Overlap

Review the selected group, teacher, classroom, day, start time, and end time.

### Automatic Generation Does Not Produce a Complete Timetable

Check:

- Required subject and group relationships
- Teacher-subject assignments
- Classroom categories and capacities
- Available teaching days and time slots
- Daily and consecutive-lesson restrictions
- Existing schedule conflicts

The generator may be unable to find a complete solution when the constraints are incompatible.

---

## 23. Data Accuracy Checklist

Before generating or exporting a timetable, verify that:

- [ ] Required teachers have been added.
- [ ] Subjects are configured correctly.
- [ ] Teacher-subject assignments are accurate.
- [ ] Required group-subject relationships are configured.
- [ ] Group names and student counts are correct.
- [ ] Classroom names and capacities are correct.
- [ ] Classroom categories match their actual use.
- [ ] Subject classroom requirements are correct.
- [ ] Generation settings reflect the institution's requirements.
- [ ] The final timetable has been reviewed for conflicts.

Incorrect source data can lead to incomplete or unsuitable schedules.

---

## 24. Export Recommendations

Export the timetable after reviewing the saved entries.

Use:

- **PDF** for printing and viewing.
- **Excel** for spreadsheet-based review and analysis.

After exporting:

1. Open the file.
2. Check that the expected records are present.
3. Verify group, teacher, subject, classroom, day, and time information.
4. Keep a copy of the final export in an approved storage location.

An exported file is a snapshot of the schedule at the time of export. Exporting does not replace the application database or automatically update a previously downloaded file.

---

## 25. Troubleshooting

### The Application Does Not Open

Check that:

- The application is running.
- PostgreSQL is available.
- The database connection string is correct.
- The required .NET runtime or SDK is installed for the selected launch method.
- The application starts without build or configuration errors.

### Database Errors Appear

Check the database connection and verify that the required Entity Framework Core migrations have been applied.

### Expected Data Is Missing

Verify that the application is connected to the correct database and that the records exist in that database.

### Schedule Generation Produces Unexpected Results

Review the source data, teacher-subject assignments, group-subject relationships, classroom requirements, available time slots, and generation settings.

### An Access-Denied or Not-Found Page Appears

Verify that the signed-in account has the required role and that the requested page is available in the deployed application version.

If the problem persists, provide the administrator with the page URL, the action performed, and the error message. Do not include passwords or other secrets.

### Export Fails

Check that the schedule can be opened in the application and try the export again. If the error persists, record the selected export format and any displayed error message.

---

## 26. Best Practices

For reliable operation:

1. Keep reference data accurate and up to date.
2. Use consistent naming conventions.
3. Verify teacher-subject and group-subject relationships.
4. Check classroom capacities and categories.
5. Review generation settings before starting.
6. Inspect the generated timetable before confirming it.
7. Correct reported conflicts before publishing the timetable.
8. Export the final schedule after reviewing changes.
9. Protect administrator credentials.
10. Avoid deleting reference records that are still used by schedules.

---

## 27. Summary

ScheduleSystem provides a centralized environment for managing college schedules, validating schedule entries, recommending suitable classrooms, and generating timetables.

The recommended workflow is:

```text
Prepare reference data
        ↓
Configure teachers, subjects, and groups
        ↓
Configure classrooms and requirements
        ↓
Review generation settings
        ↓
Generate a timetable
        ↓
Validate and review the preview
        ↓
Confirm and save
        ↓
Review the saved schedule
        ↓
Export the final timetable
```

Following this workflow helps maintain consistent scheduling data and reduces manual scheduling errors.

For problems that cannot be resolved through the application interface, contact the system administrator and provide the relevant error message and steps to reproduce the issue.
