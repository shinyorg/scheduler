using Microsoft.Maui.Layouts;

namespace Shiny.Maui.Scheduler.Internal;

internal class AgendaTimelinePanel : ContentView
{
    readonly AbsoluteLayout _eventsLayer;
    readonly Grid _timelineGrid;
    double _timeSlotHeight = 60;
    Color _timezoneColor = Colors.Gray;
    Color _defaultEventColor = Colors.CornflowerBlue;
    DataTemplate? _eventTemplate;

    public Action<SchedulerEvent>? EventTapped { get; set; }
    public Action<DateTimeOffset>? TimeSlotTapped { get; set; }

    public AgendaTimelinePanel()
    {
        _timelineGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(56)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        _eventsLayer = new AbsoluteLayout();

        Content = _timelineGrid;
    }

    public double TimeSlotHeight
    {
        get => _timeSlotHeight;
        set { _timeSlotHeight = value; }
    }

    public Color TimezoneColor
    {
        get => _timezoneColor;
        set { _timezoneColor = value; }
    }

    public Color DefaultEventColor
    {
        get => _defaultEventColor;
        set { _defaultEventColor = value; }
    }

    public DataTemplate? EventTemplate
    {
        get => _eventTemplate;
        set { _eventTemplate = value; }
    }

    public void Build(DateOnly date, IReadOnlyList<SchedulerEvent> timedEvents, CurrentTimeIndicator? timeIndicator, bool showTimeMarker)
    {
        _timelineGrid.Children.Clear();
        _timelineGrid.RowDefinitions.Clear();

        var totalHeight = 24 * _timeSlotHeight;

        for (var hour = 0; hour < 24; hour++)
            _timelineGrid.RowDefinitions.Add(new RowDefinition(new GridLength(_timeSlotHeight)));

        // time labels
        for (var hour = 0; hour < 24; hour++)
        {
            var lbl = new Label
            {
                Text = new TimeOnly(hour, 0).ToString("HH:mm"),
                FontSize = 11,
                TextColor = _timezoneColor,
                VerticalTextAlignment = TextAlignment.Start,
                HorizontalTextAlignment = TextAlignment.End,
                Padding = new Thickness(0, 0, 8, 0)
            };
            _timelineGrid.Add(lbl, 0, hour);

            var separator = new BoxView
            {
                Color = Color.FromRgba(200, 200, 200, 80),
                HeightRequest = 1,
                VerticalOptions = LayoutOptions.Start
            };
            _timelineGrid.Add(separator, 1, hour);
        }

        // events overlay in column 1
        _eventsLayer.Children.Clear();
        _eventsLayer.HeightRequest = totalHeight;

        var overlaps = DetectOverlaps(timedEvents);

        foreach (var (evt, column, totalColumns) in overlaps)
        {
            var startMinutes = evt.Start.LocalDateTime.TimeOfDay.TotalMinutes;
            var endMinutes = evt.End.LocalDateTime.TimeOfDay.TotalMinutes;
            if (DateOnly.FromDateTime(evt.Start.LocalDateTime) < date)
                startMinutes = 0;
            if (DateOnly.FromDateTime(evt.End.LocalDateTime) > date)
                endMinutes = 24 * 60;
            var duration = Math.Max(endMinutes - startMinutes, 15);

            var y = startMinutes * _timeSlotHeight / 60.0;
            var h = duration * _timeSlotHeight / 60.0;

            View eventView;
            if (_eventTemplate != null)
            {
                eventView = (View)_eventTemplate.CreateContent();
                eventView.BindingContext = evt;
            }
            else
            {
                eventView = CreateDefaultEventView(evt);
            }

            var tap = new TapGestureRecognizer();
            var captured = evt;
            tap.Tapped += (_, _) => EventTapped?.Invoke(captured);
            eventView.GestureRecognizers.Add(tap);

            AbsoluteLayout.SetLayoutBounds(eventView, new Rect(
                (double)column / totalColumns, y, 1.0 / totalColumns, h));
            AbsoluteLayout.SetLayoutFlags(eventView,
                AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional);

            _eventsLayer.Children.Add(eventView);
        }

        // time marker
        if (showTimeMarker && timeIndicator != null && date == DateOnly.FromDateTime(DateTime.Today))
        {
            var now = DateTime.Now.TimeOfDay.TotalMinutes;
            var markerY = now * _timeSlotHeight / 60.0;

            AbsoluteLayout.SetLayoutBounds(timeIndicator, new Rect(0, markerY, 1, 2));
            AbsoluteLayout.SetLayoutFlags(timeIndicator, AbsoluteLayoutFlags.WidthProportional);

            if (!_eventsLayer.Children.Contains(timeIndicator))
                _eventsLayer.Children.Add(timeIndicator);
        }

        // tappable background
        var bgTap = new TapGestureRecognizer();
        bgTap.Tapped += (_, e) =>
        {
            if (TimeSlotTapped == null) return;
            var pos = e.GetPosition(_eventsLayer);
            if (pos.HasValue)
            {
                var minutes = pos.Value.Y / _timeSlotHeight * 60.0;
                var rounded = Math.Floor(minutes / 30.0) * 30.0;
                var ts = TimeSpan.FromMinutes(rounded);
                var dt = date.ToDateTime(new TimeOnly(ts.Hours, ts.Minutes));
                TimeSlotTapped(new DateTimeOffset(dt));
            }
        };
        _eventsLayer.GestureRecognizers.Add(bgTap);

        _timelineGrid.Add(_eventsLayer, 1);
        Grid.SetRowSpan(_eventsLayer, 24);
    }

    View CreateDefaultEventView(SchedulerEvent evt)
    {
        var color = evt.Color ?? _defaultEventColor;
        var border = new Border
        {
            BackgroundColor = color,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(6, 4),
            Margin = new Thickness(1)
        };

        var stack = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = evt.Title,
                    TextColor = Colors.White,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.TailTruncation
                },
                new Label
                {
                    Text = $"{evt.Start.LocalDateTime:HH:mm} - {evt.End.LocalDateTime:HH:mm}",
                    TextColor = Color.FromRgba(255, 255, 255, 200),
                    FontSize = 10
                }
            }
        };

        border.Content = stack;
        return border;
    }

    static List<(SchedulerEvent Event, int Column, int TotalColumns)> DetectOverlaps(IReadOnlyList<SchedulerEvent> events)
    {
        if (events.Count == 0) return [];

        var sorted = events.OrderBy(e => e.Start).ThenBy(e => e.End).ToList();
        var result = new List<(SchedulerEvent Event, int Column, int TotalColumns)>();
        var groups = new List<List<SchedulerEvent>>();

        foreach (var evt in sorted)
        {
            var placed = false;
            foreach (var group in groups)
            {
                if (group.Any(g => Overlaps(g, evt)))
                {
                    group.Add(evt);
                    placed = true;
                    break;
                }
            }
            if (!placed)
                groups.Add([evt]);
        }

        foreach (var group in groups)
        {
            var columns = new List<List<SchedulerEvent>>();
            foreach (var evt in group.OrderBy(e => e.Start))
            {
                var placed = false;
                for (var c = 0; c < columns.Count; c++)
                {
                    if (!columns[c].Any(e => Overlaps(e, evt)))
                    {
                        columns[c].Add(evt);
                        result.Add((evt, c, 0));
                        placed = true;
                        break;
                    }
                }
                if (!placed)
                {
                    columns.Add([evt]);
                    result.Add((evt, columns.Count - 1, 0));
                }
            }

            var totalCols = columns.Count;
            for (var i = result.Count - group.Count; i < result.Count; i++)
                result[i] = (result[i].Event, result[i].Column, totalCols);
        }

        return result;
    }

    static bool Overlaps(SchedulerEvent a, SchedulerEvent b) =>
        a.Start < b.End && b.Start < a.End;
}
