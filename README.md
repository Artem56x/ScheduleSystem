<div align="center">

<img src="https://capsule-render.vercel.app/api?type=blur&color=0:0A84FF,50:5E5CE6,100:AF52DE&height=220&section=header&text=ScheduleSystem&fontSize=58&fontColor=FFFFFF&animation=fadeIn&fontAlignY=38&desc=Smart%20College%20Schedule%20Management%20System&descAlignY=62&descSize=18&descColor=FFFFFF" width="100%"/>

<br>

<img src="https://readme-typing-svg.demolab.com?font=JetBrains+Mono&weight=600&size=20&duration=2800&pause=900&color=0A84FF&center=true&vCenter=true&width=700&lines=Smart+Schedule+Management;ASP.NET+Core+MVC+%2B+PostgreSQL;Conflict+Detection+%2B+Smart+Recommendations;Modern+Glassmorphism+UI;Built+with+C%23+%F0%9F%92%99" alt="Typing SVG" />

<br><br>

<img src="https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white"/>
<img src="https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white"/>
<img src="https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>

<br>

<img src="https://img.shields.io/badge/MVC-0A84FF?style=for-the-badge"/>
<img src="https://img.shields.io/badge/Razor-0A84FF?style=for-the-badge"/>
<img src="https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white"/>
<img src="https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white"/>
<img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black"/>

<br><br>

<a href="https://github.com/Artem56x/ScheduleSystem">
<img src="https://img.shields.io/github/stars/Artem56x/ScheduleSystem?style=flat-square&logo=github&label=Stars"/>
</a>
<a href="https://github.com/Artem56x/ScheduleSystem/network/members">
<img src="https://img.shields.io/github/forks/Artem56x/ScheduleSystem?style=flat-square&logo=github&label=Forks"/>
</a>
<a href="https://github.com/Artem56x/ScheduleSystem">
<img src="https://img.shields.io/github/repo-size/Artem56x/ScheduleSystem?style=flat-square&label=Size"/>
</a>

</div>

---

# 🗓️ ScheduleSystem

**ScheduleSystem** — веб-приложение для создания, управления и просмотра расписания учебного заведения.

Проект разработан на **ASP.NET Core MVC** с использованием **Entity Framework Core** и **PostgreSQL**.

Главная идея проекта — не просто хранить расписание, а помогать составлять его **без конфликтов и логических ошибок**.

---

## ✨ Возможности

### 📅 Управление расписанием

* Создание занятий
* Редактирование занятий
* Удаление занятий
* Просмотр полного расписания
* Сортировка занятий по времени
* Группировка расписания по группам и дням недели
* Отдельное расписание группы
* Отдельное расписание преподавателя
* Отдельное расписание аудитории

### 🔎 Умный поиск

Поиск и фильтрация расписания позволяют быстро находить нужные занятия.

Можно работать с расписанием конкретных:

* 👥 групп
* 👨‍🏫 преподавателей
* 📚 предметов
* 🏫 аудиторий
* 📆 дней недели

---

## 🧠 Smart Schedule Validation

Одна из основных частей проекта — автоматическая проверка расписания.

Система анализирует создаваемое занятие и предупреждает пользователя о возможных конфликтах.

### ⏱️ Проверка пересечений времени

Например:

```text
10:00 ───────── 11:30
           11:00 ───────── 12:30
```

Система определяет пересечение и проверяет сразу несколько ресурсов:

```text
👨‍🏫 Преподаватель
👥 Группа
🏫 Аудитория
```

Это позволяет избежать ситуации, когда один преподаватель, группа или кабинет одновременно назначены на несколько занятий.

---

## 🏫 Умные рекомендации аудиторий

Система учитывает не только занятость аудитории.

При выборе кабинета проверяются:

* вместимость;
* количество студентов;
* наличие компьютеров;
* категория аудитории;
* занятость в выбранное время.

Например:

> В аудитории «404» недостаточно мест.
> Вместимость: **10**
> В группе: **12 человек**

После этого система может предложить подходящие свободные аудитории.

```text
Свободные аудитории:

102 · 105 · 107

Осталось мест:
+8 · +15 · +23
```

---

## 💻 Требования к аудиториям

Для разных предметов могут использоваться разные типы помещений.

Например:

```text
💻 Информатика
→ требуется компьютерная аудитория

🔬 Лабораторная работа
→ требуется лаборатория

🚗 Практика
→ специализированная аудитория
```

Категории аудиторий хранятся в базе данных и могут расширяться без изменения структуры приложения.

---

## 👨‍🏫 Преподаватели

Для преподавателей предусмотрено:

* создание;
* редактирование;
* удаление;
* просмотр;
* привязка к предмету;
* просмотр индивидуального расписания.

Система также может определить несоответствие выбранного предмета предмету преподавателя.

---

## 👥 Группы

Для учебных групп хранятся:

* название;
* специальность;
* количество студентов;
* описание.

Количество студентов используется при автоматической проверке вместимости аудитории.

---

## 📚 Предметы

Каждый предмет является отдельной сущностью базы данных.

Это позволяет использовать единый список предметов при создании расписания и связывать их с преподавателями и требованиями к аудиториям.

---

## 🏫 Аудитории

Аудитория содержит:

```text
Название
Категория
Вместимость
Наличие компьютеров
```

Благодаря этому система может автоматически определить, подходит ли помещение для конкретного занятия.

---

# 🏗️ Архитектура

Проект построен по архитектуре **ASP.NET Core MVC**.

```text
ScheduleSystem
│
├── Controllers
│   ├── HomeController
│   ├── SchedulesController
│   ├── TeachersController
│   ├── GroupsController
│   ├── SubjectsController
│   ├── ClassroomsController
│   └── ClassroomCategoriesController
│
├── Data
│   └── ApplicationDbContext
│
├── Models
│   ├── Schedule
│   ├── Teacher
│   ├── Group
│   ├── Subject
│   ├── Classroom
│   └── ClassroomCategory
│
├── Views
│   ├── Home
│   ├── Schedules
│   ├── Teachers
│   ├── Groups
│   ├── Subjects
│   ├── Classrooms
│   └── ClassroomCategories
│
├── Migrations
│
└── wwwroot
    ├── css
    ├── js
    └── lib
```

---

# 🛠️ Tech Stack

<div align="center">

| Technology                | Purpose               |
| ------------------------- | --------------------- |
| **C#**                    | Основной язык         |
| **ASP.NET Core MVC**      | Web framework         |
| **.NET 8**                | Platform              |
| **Entity Framework Core** | ORM                   |
| **PostgreSQL**            | Database              |
| **Razor**                 | Server-side views     |
| **HTML / CSS**            | UI                    |
| **JavaScript**            | Client-side logic     |
| **Bootstrap**             | UI components         |
| **jQuery**                | Client-side utilities |

</div>

---

# 🎨 UI / Design

Интерфейс выполнен в современном стиле с элементами **glassmorphism**.

Основные принципы:

```text
┌─────────────────────────────────────────┐
│  Clean interface                        │
│                                         │
│  Glass cards                            │
│  Soft shadows                           │
│  Blue accent colors                     │
│  Smooth interactions                    │
│  Minimal navigation                     │
│                                         │
└─────────────────────────────────────────┘
```

Цель дизайна — сделать сложную систему расписания максимально понятной для пользователя.

---

# ⚙️ Installation

### 1. Clone

```bash
git clone https://github.com/Artem56x/ScheduleSystem.git
cd ScheduleSystem
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Configure PostgreSQL

Создай базу данных:

```text
schedulesystem
```

Строку подключения рекомендуется хранить через **.NET User Secrets**, а не непосредственно в репозитории.

### 4. Apply migrations

```bash
dotnet ef database update
```

### 5. Run

```bash
dotnet run
```

После запуска приложение будет доступно на локальном адресе, который покажет ASP.NET Core.

---

# 🗄️ Database

Проект использует **PostgreSQL**.

Основные связи:

```text
                    ┌──────────────┐
                    │   Schedule   │
                    └──────┬───────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
     ┌──────────┐     ┌──────────┐    ┌──────────┐
     │ Teacher  │     │  Group   │    │ Subject  │
     └──────────┘     └──────────┘    └──────────┘
                           │
                           │
                           ▼
                    ┌──────────────┐
                    │  Classroom   │
                    └──────┬───────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │ClassroomCategory │
                  └──────────────────┘
```

---

# 🚀 Roadmap

### ✅ Completed

* [x] MVC architecture
* [x] PostgreSQL database
* [x] Entity Framework Core
* [x] CRUD operations
* [x] Schedule management
* [x] Group schedule
* [x] Teacher schedule
* [x] Classroom schedule
* [x] Search and filtering
* [x] Teacher conflict detection
* [x] Group conflict detection
* [x] Classroom conflict detection
* [x] Classroom capacity validation
* [x] Computer-room requirements
* [x] Smart classroom recommendations
* [x] Classroom categories
* [x] Modern responsive UI

### 🔄 In Progress

* [ ] More advanced recommendation logic
* [ ] Improved schedule optimization
* [ ] More detailed statistics
* [ ] Better mobile experience

### 💡 Future

* [ ] Authentication & authorization
* [ ] Administrator panel
* [ ] User roles
* [ ] Export to PDF
* [ ] Export to Excel
* [ ] Automatic schedule generation
* [ ] Notifications
* [ ] REST API
* [ ] Docker deployment

---

# 📊 Project Focus

```text
Backend        ████████████████████  100%
Database       ████████████████████  100%
CRUD           ████████████████████  100%
Validation     ███████████████████░   95%
UI / UX        ██████████████████░░   90%
Recommendations ████████████████░░░░   80%
Optimization   ████████░░░░░░░░░░░░   40%
```

---

# 🔐 Security

Конфиденциальные данные подключения к базе данных **не должны храниться непосредственно в Git**.

Для локальной разработки используется:

```bash
dotnet user-secrets
```

Файлы с секретами и локальными настройками исключены из Git через `.gitignore`.

---

# 📌 Project Status

```text
Status:        🟢 Active Development
Version:       1.0
Platform:      Web
Framework:     ASP.NET Core
Database:      PostgreSQL
Architecture:  MVC
```

---

<div align="center">

## 💙 Built with C# & ASP.NET Core

<br>

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0A84FF,50:5E5CE6,100:AF52DE&height=120&section=footer"/>

### 👨‍💻 Artem56x

<a href="https://github.com/Artem56x">
<img src="https://img.shields.io/badge/GitHub-Artem56x-181717?style=for-the-badge&logo=github"/>
</a>

<br><br>

⭐ If you like the project, consider giving it a star.

</div>
