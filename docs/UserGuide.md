# ScheduleSystem — User Guide

## 1. Overview

ScheduleSystem is a web application for managing and generating college class schedules.

The system allows users to work with:

* schedules;
* teachers;
* student groups;
* subjects;
* classrooms;
* notifications;
* schedule exports;
* personal account settings.

The application is designed for educational institutions where schedule management needs to be centralized and partially automated.

---

## 2. Getting Started

After launching the application, the user is presented with the main page.

The main navigation contains:

* **Главная** — dashboard and system overview;
* **Расписание** — schedule management;
* **Преподаватели** — teacher management;
* **Группы** — student group management;
* **Предметы** — subject management;
* **Аудитории** — classroom management.

Additional account and settings functions are available through the user profile.

---

## 3. Dashboard

The **Главная** page provides an overview of the system.

The dashboard contains information about the current amount of:

* teachers;
* groups;
* subjects;
* classrooms;
* scheduled lessons.

The dashboard is intended to provide a quick overview without opening individual sections.

---

## 4. Working with the Schedule

Open **Расписание** from the main navigation.

The schedule page contains the available lessons and filtering tools.

### 4.1 Schedule Filters

The schedule can be filtered by:

* group;
* teacher;
* classroom;
* subject;
* day of the week;
* time.

Filters can be combined to find a specific lesson or subset of the timetable.

For example, a user can select a group and a day to display only the lessons for that group on the selected day.

---

## 5. Creating a Schedule Entry

To create a new lesson:

1. Open **Расписание**.
2. Select the option to add a schedule entry.
3. Select a group.
4. Select a teacher.
5. Select a subject.
6. Select a classroom.
7. Select the day of the week.
8. Specify the start time.
9. Specify the end time.
10. Save the entry.

The system validates the entered data before saving.

---

## 6. Schedule Validation

ScheduleSystem checks schedule entries for conflicts.

The system validates:

* start and end time;
* teacher conflicts;
* group conflicts;
* classroom conflicts;
* overlapping lessons;
* compatibility of classroom requirements;
* classroom capacity.

A lesson cannot be saved when it creates an invalid schedule conflict unless the corresponding business logic explicitly allows the operation.

### Example

If a teacher already has a lesson from `08:30` to `09:30`, assigning the same teacher to another lesson during the same period creates a conflict.

The system detects the conflict and informs the user.

---

## 7. Automatic Schedule Generation

ScheduleSystem contains an automatic schedule generation system.

The generator can create a timetable based on the available:

* groups;
* subjects;
* teachers;
* classrooms;
* teaching days;
* schedule restrictions.

The default generation settings include:

| Setting                 | Default value |
| ----------------------- | ------------: |
| Lessons per week        |             3 |
| Maximum lessons per day |             4 |
| Teaching days per week  |             5 |
| Maximum gaps per day    |             2 |
| Allow gaps              |           Yes |

The generator attempts to create a valid timetable while respecting the configured restrictions.

Automatic generation is useful when a large number of groups and subjects need to be scheduled.

---

## 8. Teachers

Open **Преподаватели** to manage teachers.

A teacher can be associated with multiple subjects.

### 8.1 Adding a Teacher

To add a teacher:

1. Open **Преподаватели**.
2. Select **Добавить**.
3. Enter the teacher information.
4. Select the subjects taught by the teacher.
5. Save the teacher.

### 8.2 Editing a Teacher

Open the required teacher and select the edit option.

The assigned subjects can be changed at any time.

### 8.3 Teacher Validation

When creating or editing a schedule, the system checks the relationship between the selected teacher and subject.

If the teacher is not assigned to the selected subject, the system can display a confirmation warning.

This prevents accidental assignment while still allowing an administrator to explicitly confirm the operation when required.

---

## 9. Student Groups

Open **Группы** to manage student groups.

A group contains information such as:

* group name;
* course;
* number of students.

The course is limited to the supported college course range.

The number of students is used by the classroom recommendation system when selecting suitable classrooms.

---

## 10. Subjects

Open **Предметы** to manage subjects.

A subject can contain:

* subject name;
* course;
* subject type;
* classroom category requirements;
* assigned teachers.

Subjects can require specific types of classrooms.

For example, a subject requiring computers can be associated with the **Компьютерный класс** classroom category.

---

## 11. Classrooms

Open **Аудитории** to manage classrooms.

A classroom contains:

* classroom name;
* classroom category;
* capacity.

The classroom category describes the type of room.

Examples may include:

* standard classroom;
* computer classroom;
* specialized classroom.

The classroom capacity is used when selecting suitable rooms for student groups.

---

## 12. Classroom Recommendations

ScheduleSystem can recommend suitable classrooms for a lesson.

The recommendation system considers:

1. Classroom category requirements of the subject.
2. Number of students in the group.
3. Classroom capacity.
4. Classroom availability.

For example, if a group contains 25 students, a classroom with a capacity of 20 students will not be recommended.

If a subject requires a computer classroom, only classrooms belonging to the appropriate category are considered suitable.

---

## 13. Notifications

The system provides notifications for important schedule-related events.

Notifications can contain information about:

* the subject;
* the group;
* the scheduled time;
* schedule changes.

The notification interface is available through the notification section of the application.

Example:

> Компьютерная графика — 25БД-1
> Время: 08:30–09:30

Notifications help users quickly identify changes or important schedule information.

---

## 14. Schedule Export

ScheduleSystem supports exporting schedules into several formats:

* PDF;
* Excel;
* CSV.

The system supports exporting the general schedule as well as schedules for individual entities where applicable.

Typical file names include:

```text
Общее_расписание.pdf
Общее_расписание.xlsx
Общее_расписание.csv
```

Individual schedule exports may use a format similar to:

```text
Расписание_Название.xlsx
```

Exported files can be opened using compatible desktop applications.

---

## 15. Account

The application uses authentication based on ASP.NET Core Identity.

Users can access their personal account through the account interface.

The profile page contains information about the current account.

Depending on the user's permissions, additional administrative functionality may be available.

---

## 16. Profile

The profile section provides access to personal information and account-related functions.

The page can contain:

* display name;
* email address;
* account status;
* quick actions;
* notification settings.

The profile interface is designed to provide access to frequently used account functions without navigating through multiple pages.

---

## 17. Settings

The application provides settings for account and system-related functionality.

Available sections depend on the user's role.

The settings interface may contain:

* profile settings;
* notification settings;
* program settings for administrators;
* application information.

---

## 18. Roles and Permissions

ScheduleSystem supports role-based access control.

The main roles are:

* **User** — regular application user;
* **Admin** — administrator with access to administrative functions.

Administrative functions may include management of system data and configuration.

Users should only perform actions allowed by their assigned role.

---

## 19. Working with Data

When entering data into the system, use consistent naming.

### Recommended group naming

Use the official group name used by the educational institution.

### Recommended teacher naming

Enter the teacher's full name in a consistent format.

### Recommended subject naming

Use the official subject name from the curriculum.

### Recommended classroom naming

Use the actual classroom number or identifier.

Consistent naming makes searching, filtering and exporting information easier.

---

## 20. Recommended Workflow

For a new installation, it is recommended to enter information in the following order:

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
Teacher–Subject Relations
        ↓
Schedule Generation
        ↓
Schedule Validation
        ↓
Schedule Export
```

This order ensures that the generator has the necessary data available.

---

## 21. Creating a Complete Timetable

A typical workflow for creating a timetable is:

### Step 1 — Add classrooms

Create the available classrooms and specify their categories and capacities.

### Step 2 — Add teachers

Create teacher accounts and assign the subjects they teach.

### Step 3 — Add subjects

Create the subjects and specify their course and classroom requirements.

### Step 4 — Add groups

Create student groups and specify the number of students.

### Step 5 — Generate the schedule

Use the schedule generation functionality to create an initial timetable.

### Step 6 — Review the timetable

Check the generated schedule using the available filters.

### Step 7 — Correct individual entries

If necessary, edit individual schedule entries.

### Step 8 — Export

Export the final timetable to the required format.

---

## 22. Avoiding Common Problems

### Teacher is not available

Check whether the teacher already has a lesson during the selected period.

### Classroom is not suitable

Check:

* classroom category;
* classroom capacity;
* existing lessons in the same period.

### Group conflict

Check whether the group already has another lesson at the selected time.

### Subject cannot be assigned to a teacher

Check the teacher's assigned subjects.

If the assignment is intentional, review the confirmation warning shown by the system.

### Schedule overlaps

Check the selected:

* group;
* teacher;
* classroom;
* day;
* start time;
* end time.

---

## 23. Data Accuracy Recommendations

Before generating or exporting a timetable, verify that:

* all teachers have been added;
* subjects are correctly configured;
* teacher-subject relationships are correct;
* all groups have correct student counts;
* classrooms have correct capacities;
* classroom categories are correct;
* schedule restrictions are configured correctly.

Incorrect source data can lead to an incomplete or unsuitable generated timetable.

---

## 24. Export Recommendations

It is recommended to export the final schedule after all changes have been reviewed.

Use:

* **PDF** for printing and viewing;
* **Excel** for further editing and analysis;
* **CSV** for data processing or import into other systems.

Keep an exported copy of the final timetable as a backup.

---

## 25. Troubleshooting

### The application does not open

Check that:

* PostgreSQL is running;
* the database connection string is correct;
* the required .NET runtime/SDK is installed;
* the application starts without build errors.

### Database errors appear

Check the database connection and ensure that all EF Core migrations have been applied.

### Data is missing

Verify that the correct database is configured in the application.

### Schedule generation produces unexpected results

Check the source data and generation settings.

In particular, verify:

* teacher-subject relationships;
* group student counts;
* classroom capacities;
* classroom categories;
* subject requirements;
* existing schedule conflicts.

---

## 26. Best Practices

For reliable operation:

1. Keep all reference data up to date.
2. Use consistent naming.
3. Check teacher-subject relationships.
4. Verify classroom capacities.
5. Review generated schedules before publishing them.
6. Export final schedules as backups.
7. Use administrator permissions only when necessary.
8. Avoid deleting reference data that is still used by schedules.

---

## 27. Summary

ScheduleSystem provides a centralized environment for managing college schedules.

The recommended workflow is:

```text
Prepare data
    ↓
Configure teachers and subjects
    ↓
Configure groups and classrooms
    ↓
Generate schedule
    ↓
Validate schedule
    ↓
Review and edit
    ↓
Export final schedule
```

Following this workflow helps maintain consistent schedule data and reduces the number of manual scheduling errors.
