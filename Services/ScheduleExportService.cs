using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScheduleSystem.Models;

namespace ScheduleSystem.Services;

public class ScheduleExportService
{
    // ============================================================
    // EXCEL
    // ============================================================

    public byte[] CreateAllExcel(List<Schedule> schedules)
    {
        return CreateExcel(
            "Расписание всех занятий",
            "Полное расписание",
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Преподаватель",
                "Группа",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Teacher?.FullName ?? "—",
                schedule.Group?.Name ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    public byte[] CreateGroupExcel(
        string groupName,
        List<Schedule> schedules)
    {
        return CreateExcel(
            $"Расписание группы {groupName}",
            $"Группа: {groupName}",
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Преподаватель",
                "Аудитория",
                "Группа"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Teacher?.FullName ?? "—",
                schedule.Classroom?.Name ?? "—",
                schedule.Group?.Name ?? "—"
            });
    }

    public byte[] CreateTeacherExcel(
        string teacherName,
        List<Schedule> schedules)
    {
        return CreateExcel(
            $"Расписание преподавателя {teacherName}",
            $"Преподаватель: {teacherName}",
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Группа",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Group?.Name ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    private static byte[] CreateExcel(
        string title,
        string subtitle,
        string[] headers,
        List<Schedule> schedules,
        Func<Schedule, string[]> rowSelector)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Расписание");

        ConfigureWorksheet(worksheet);

        var sortedSchedules = SortSchedules(schedules);

        int currentRow = 1;

        // Заголовок
        worksheet.Cell(currentRow, 1).Value = title;
        worksheet.Range(
            currentRow,
            1,
            currentRow,
            headers.Length)
            .Merge();

        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 1).Style.Font.FontSize = 18;
        worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
        worksheet.Cell(currentRow, 1).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Row(currentRow).Height = 30;

        currentRow++;

        // Подзаголовок
        worksheet.Cell(currentRow, 1).Value =
            $"{subtitle} • Занятий: {sortedSchedules.Count}";

        worksheet.Range(
            currentRow,
            1,
            currentRow,
            headers.Length)
            .Merge();

        worksheet.Cell(currentRow, 1).Style.Font.FontSize = 11;
        worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
        worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        currentRow++;

        currentRow++;

        // Заголовки таблицы
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(currentRow, i + 1);

            cell.Value = headers[i];

            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;
        }

        worksheet.Row(currentRow).Height = 24;

        int headerRow = currentRow;

        currentRow++;

        // Данные
        foreach (var schedule in sortedSchedules)
        {
            var values = rowSelector(schedule);

            for (int i = 0; i < values.Length; i++)
            {
                var cell = worksheet.Cell(currentRow, i + 1);

                cell.Value = values[i];

                cell.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                cell.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;

                cell.Style.Border.BottomBorder =
                    XLBorderStyleValues.Thin;
            }

            worksheet.Row(currentRow).Height = 22;

            currentRow++;
        }

        // Если расписание пустое
        if (sortedSchedules.Count == 0)
        {
            worksheet.Cell(currentRow, 1).Value =
                "Расписание отсутствует.";

            worksheet.Range(
                currentRow,
                1,
                currentRow,
                headers.Length)
                .Merge();

            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
        }

        // Автоширина
        worksheet.Columns().AdjustToContents();

        // Ограничиваем слишком широкие столбцы
        foreach (var column in worksheet.ColumnsUsed())
        {
            if (column.Width > 40)
            {
                column.Width = 40;
            }

            if (column.Width < 12)
            {
                column.Width = 12;
            }
        }

        worksheet.SheetView.FreezeRows(headerRow);

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    private static void ConfigureWorksheet(
        IXLWorksheet worksheet)
    {
        worksheet.Style.Font.FontName = "Aptos";
        worksheet.Style.Font.FontSize = 11;

        worksheet.PageSetup.PageOrientation =
            XLPageOrientation.Landscape;

        worksheet.PageSetup.PaperSize =
            XLPaperSize.A4Paper;

        worksheet.PageSetup.Margins.Top = 0.5;
        worksheet.PageSetup.Margins.Bottom = 0.5;
        worksheet.PageSetup.Margins.Left = 0.5;
        worksheet.PageSetup.Margins.Right = 0.5;

        worksheet.PageSetup.CenterHorizontally = true;
    }

    // ============================================================
    // PDF
    // ============================================================

    public byte[] CreateAllPdf(List<Schedule> schedules)
    {
        return CreatePdf(
            "Расписание всех занятий",
            "Полное расписание",
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Преподаватель",
                "Группа",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Teacher?.FullName ?? "—",
                schedule.Group?.Name ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    public byte[] CreateGroupPdf(
        string groupName,
        List<Schedule> schedules)
    {
        return CreatePdf(
            $"Расписание группы {groupName}",
            $"Группа: {groupName}",
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Преподаватель",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Teacher?.FullName ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    public byte[] CreateTeacherPdf(
        string teacherName,
        List<Schedule> schedules)
    {
        return CreatePdf(
            $"Расписание преподавателя {teacherName}",
            $"Преподаватель: {teacherName}",
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Группа",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Group?.Name ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    private static byte[] CreatePdf(
        string title,
        string subtitle,
        string[] headers,
        List<Schedule> schedules,
        Func<Schedule, string[]> rowSelector)
    {
        var sortedSchedules = SortSchedules(schedules);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());

                page.Margin(25);

                page.DefaultTextStyle(
                    TextStyle.Default.FontSize(9));

                // ====================================================
                // HEADER
                // ====================================================

                page.Header()
                    .PaddingBottom(10)
                    .Column(column =>
                    {
                        column.Item()
                            .AlignCenter()
                            .Text(title)
                            .FontSize(18)
                            .Bold();

                        column.Item()
                            .PaddingTop(4)
                            .AlignCenter()
                            .Text(
                                $"{subtitle} • Занятий: {sortedSchedules.Count}")
                            .FontSize(10);
                    });

                // ====================================================
                // CONTENT
                // ====================================================

                page.Content()
                    .Table(table =>
                    {
                        // Колонки
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0;
                                 i < headers.Length;
                                 i++)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        // =================================================
                        // HEADER ROW
                        // =================================================

                        foreach (var header in headers)
                        {
                            table.Cell()
                                .Background("#2563EB")
                                .Padding(6)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(header)
                                .FontColor("#FFFFFF")
                                .Bold()
                                .FontSize(9);
                        }

                        // =================================================
                        // DATA ROWS
                        // =================================================

                        foreach (var schedule in sortedSchedules)
                        {
                            var values = rowSelector(schedule);

                            foreach (var value in values)
                            {
                                table.Cell()
                                    .BorderBottom(1)
                                    .BorderColor("#D1D5DB")
                                    .Padding(5)
                                    .AlignMiddle()
                                    .Text(value)
                                    .FontSize(8);
                            }
                        }

                        // =================================================
                        // EMPTY STATE
                        // =================================================

                        if (sortedSchedules.Count == 0)
                        {
                            table.Cell()
                                .ColumnSpan((uint)headers.Length)
                                .Padding(15)
                                .AlignCenter()
                                .Text("Расписание отсутствует.")
                                .FontSize(10);
                        }
                    });

                // ====================================================
                // FOOTER
                // ====================================================

                page.Footer()
                    .PaddingTop(8)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(
                                $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(8);

                        row.RelativeItem()
                            .AlignRight()
                            .Text(text =>
                            {
                                text.Span("Страница ");
                                text.CurrentPageNumber();
                                text.Span(" из ");
                                text.TotalPages();
                            });
                    });
            });
        });

        return document.GeneratePdf();
    }

    // ============================================================
    // CSV
    // ============================================================

    public byte[] CreateAllCsv(List<Schedule> schedules)
    {
        return CreateCsv(
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Преподаватель",
                "Группа",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Teacher?.FullName ?? "—",
                schedule.Group?.Name ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    public byte[] CreateGroupCsv(
        string groupName,
        List<Schedule> schedules)
    {
        return CreateCsv(
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Преподаватель",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Teacher?.FullName ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    public byte[] CreateTeacherCsv(
        string teacherName,
        List<Schedule> schedules)
    {
        return CreateCsv(
            new[]
            {
                "День",
                "Время",
                "Предмет",
                "Группа",
                "Аудитория"
            },
            schedules,
            schedule => new[]
            {
                GetDayName(schedule.DayOfWeek),
                GetTimeRange(schedule),
                schedule.Subject?.Name ?? "—",
                schedule.Group?.Name ?? "—",
                schedule.Classroom?.Name ?? "—"
            });
    }

    private static byte[] CreateCsv(
        string[] headers,
        List<Schedule> schedules,
        Func<Schedule, string[]> rowSelector)
    {
        var builder = new StringBuilder();

        // BOM для корректного открытия кириллицы в Excel
        builder.Append('\uFEFF');

        // Заголовок
        builder.AppendLine(
            string.Join(
                ";",
                headers.Select(EscapeCsv)));

        // Данные
        foreach (var schedule in SortSchedules(schedules))
        {
            var values = rowSelector(schedule);

            builder.AppendLine(
                string.Join(
                    ";",
                    values.Select(EscapeCsv)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.Contains(';') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    // ============================================================
    // SORTING
    // ============================================================

    private static List<Schedule> SortSchedules(
        IEnumerable<Schedule> schedules)
    {
        return schedules
            .OrderBy(s => GetDayOrder(s.DayOfWeek))
            .ThenBy(s => s.StartTime)
            .ThenBy(s => s.EndTime)
            .ThenBy(s => s.Group?.Name)
            .ThenBy(s => s.Classroom?.Name)
            .ToList();
    }

    private static int GetDayOrder(
        DayOfWeek? day)
    {
        if (day == null)
        {
            return 99;
        }

        return day.Value switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => 99
        };
    }

    private static string GetDayName(
        DayOfWeek? day)
    {
        return day switch
        {
            DayOfWeek.Monday => "Понедельник",
            DayOfWeek.Tuesday => "Вторник",
            DayOfWeek.Wednesday => "Среда",
            DayOfWeek.Thursday => "Четверг",
            DayOfWeek.Friday => "Пятница",
            DayOfWeek.Saturday => "Суббота",
            DayOfWeek.Sunday => "Воскресенье",
            _ => "—"
        };
    }

    private static string GetTimeRange(
        Schedule schedule)
    {
        return
            $"{schedule.StartTime:hh\\:mm} – {schedule.EndTime:hh\\:mm}";
    }
}