using Shiny.Maui.Scheduler;

namespace SampleApp.Services;

public class SampleSchedulerProvider : ISchedulerEventProvider
{
    static readonly Color[] CategoryColors =
    [
        Color.FromArgb("#4285F4"), // Blue - Meetings
        Color.FromArgb("#0F9D58"), // Green - Personal
        Color.FromArgb("#DB4437"), // Red - Important
        Color.FromArgb("#F4B400"), // Yellow - Reminders
        Color.FromArgb("#AB47BC"), // Purple - Projects
        Color.FromArgb("#00ACC1"), // Cyan - Travel
    ];

    public async Task<IReadOnlyList<SchedulerEvent>> GetEvents(DateTimeOffset start, DateTimeOffset end)
    {
        await Task.Delay(500); // simulate network

        var events = new List<SchedulerEvent>();
        var current = start.LocalDateTime.Date;
        var endDate = end.LocalDateTime.Date;

        while (current <= endDate)
        {
            var dow = current.DayOfWeek;

            // Weekday meetings
            if (dow is >= DayOfWeek.Monday and <= DayOfWeek.Friday)
            {
                events.Add(new SchedulerEvent
                {
                    Title = "Team Standup",
                    Description = "Daily sync with the team",
                    Color = CategoryColors[0],
                    Start = new DateTimeOffset(current.AddHours(9)),
                    End = new DateTimeOffset(current.AddHours(9).AddMinutes(30))
                });

                events.Add(new SchedulerEvent
                {
                    Title = "Lunch Break",
                    Description = "Take a break",
                    Color = CategoryColors[1],
                    Start = new DateTimeOffset(current.AddHours(12)),
                    End = new DateTimeOffset(current.AddHours(13))
                });
            }

            // Monday planning
            if (dow == DayOfWeek.Monday)
            {
                events.Add(new SchedulerEvent
                {
                    Title = "Sprint Planning",
                    Description = "Plan the week's work",
                    Color = CategoryColors[4],
                    Start = new DateTimeOffset(current.AddHours(10)),
                    End = new DateTimeOffset(current.AddHours(11).AddMinutes(30))
                });
            }

            // Wednesday design review
            if (dow == DayOfWeek.Wednesday)
            {
                events.Add(new SchedulerEvent
                {
                    Title = "Design Review",
                    Description = "Review new mockups",
                    Color = CategoryColors[4],
                    Start = new DateTimeOffset(current.AddHours(14)),
                    End = new DateTimeOffset(current.AddHours(15))
                });

                events.Add(new SchedulerEvent
                {
                    Title = "1:1 with Manager",
                    Description = "Weekly check-in",
                    Color = CategoryColors[2],
                    Start = new DateTimeOffset(current.AddHours(15).AddMinutes(30)),
                    End = new DateTimeOffset(current.AddHours(16))
                });
            }

            // Friday retro
            if (dow == DayOfWeek.Friday)
            {
                events.Add(new SchedulerEvent
                {
                    Title = "Sprint Retro",
                    Description = "What went well, what didn't",
                    Color = CategoryColors[4],
                    Start = new DateTimeOffset(current.AddHours(16)),
                    End = new DateTimeOffset(current.AddHours(17))
                });
            }

            // All-day: first Monday = sprint start
            if (dow == DayOfWeek.Monday && current.Day <= 7)
            {
                events.Add(new SchedulerEvent
                {
                    Title = "Sprint Start",
                    IsAllDay = true,
                    Color = CategoryColors[3],
                    Start = new DateTimeOffset(current),
                    End = new DateTimeOffset(current.AddDays(5))
                });
            }

            // All-day holiday on the 15th
            if (current.Day == 15)
            {
                events.Add(new SchedulerEvent
                {
                    Title = "Company Holiday",
                    IsAllDay = true,
                    Color = CategoryColors[2],
                    Start = new DateTimeOffset(current),
                    End = new DateTimeOffset(current.AddDays(1))
                });
            }

            // Afternoon workout on Tue/Thu
            if (dow is DayOfWeek.Tuesday or DayOfWeek.Thursday)
            {
                events.Add(new SchedulerEvent
                {
                    Title = "Gym",
                    Description = "Afternoon workout",
                    Color = CategoryColors[1],
                    Start = new DateTimeOffset(current.AddHours(17).AddMinutes(30)),
                    End = new DateTimeOffset(current.AddHours(18).AddMinutes(30))
                });
            }

            current = current.AddDays(1);
        }

        return events;
    }

    public void OnEventSelected(SchedulerEvent selectedEvent)
    {
        Application.Current?.Windows.FirstOrDefault()?.Page?.DisplayAlertAsync(
            selectedEvent.Title,
            $"{selectedEvent.Description}\n{selectedEvent.Start.LocalDateTime:g} - {selectedEvent.End.LocalDateTime:g}",
            "OK");
    }

    public bool CanCalendarSelect(DateOnly selectedDate) => true;

    public void OnCalendarDateSelected(DateOnly selectedDate) { }

    public void OnAgendaTimeSelected(DateTimeOffset selectedTime)
    {
        Application.Current?.Windows.FirstOrDefault()?.Page?.DisplayAlertAsync(
            "Time Selected",
            $"{selectedTime.LocalDateTime:g}",
            "OK");
    }

    public bool CanSelectAgendaTime(DateTimeOffset selectedTime) => true;
}
