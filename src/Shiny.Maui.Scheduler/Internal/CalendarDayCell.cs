namespace Shiny.Maui.Scheduler.Internal;

internal class CalendarDayCell : ContentView
{
    readonly Label _dateLabel;
    readonly VerticalStackLayout _eventsStack;
    readonly Grid _root;

    DateOnly _date;
    IReadOnlyList<SchedulerEvent> _events = [];
    bool _isSelected;
    bool _isCurrentMonth;
    bool _isToday;
    int _maxEvents = 3;
    bool _showCountOnly;
    DataTemplate? _eventTemplate;
    DataTemplate? _overflowTemplate;
    Color _cellColor = Colors.White;
    Color _selectedColor = Colors.LightBlue;
    Color _currentDayColor = Colors.DodgerBlue;

    public CalendarDayCell()
    {
        _dateLabel = new Label
        {
            FontSize = 12,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HeightRequest = 24,
            WidthRequest = 24
        };

        _eventsStack = new VerticalStackLayout
        {
            Spacing = 1
        };

        _root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(28)),
                new RowDefinition(GridLength.Star)
            },
            Padding = new Thickness(1)
        };

        _root.Add(_dateLabel, 0, 0);
        _root.Add(_eventsStack, 0, 1);

        Content = _root;
    }

    public DateOnly Date
    {
        get => _date;
        set { _date = value; Refresh(); }
    }

    public IReadOnlyList<SchedulerEvent> Events
    {
        get => _events;
        set { _events = value; RefreshEvents(); }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; RefreshAppearance(); }
    }

    public bool IsCurrentMonth
    {
        get => _isCurrentMonth;
        set { _isCurrentMonth = value; RefreshAppearance(); }
    }

    public bool IsToday
    {
        get => _isToday;
        set { _isToday = value; RefreshAppearance(); }
    }

    public int MaxEvents
    {
        get => _maxEvents;
        set { _maxEvents = value; RefreshEvents(); }
    }

    public bool ShowCountOnly
    {
        get => _showCountOnly;
        set { _showCountOnly = value; RefreshEvents(); }
    }

    public DataTemplate? EventTemplate
    {
        get => _eventTemplate;
        set { _eventTemplate = value; RefreshEvents(); }
    }

    public DataTemplate? OverflowTemplate
    {
        get => _overflowTemplate;
        set { _overflowTemplate = value; RefreshEvents(); }
    }

    public Color CellColor
    {
        get => _cellColor;
        set { _cellColor = value; RefreshAppearance(); }
    }

    public Color SelectedColor
    {
        get => _selectedColor;
        set { _selectedColor = value; RefreshAppearance(); }
    }

    public Color CurrentDayColor
    {
        get => _currentDayColor;
        set { _currentDayColor = value; RefreshAppearance(); }
    }

    public Action<SchedulerEvent>? EventTapped { get; set; }
    public Action<DateOnly>? DayTapped { get; set; }

    void Refresh()
    {
        _dateLabel.Text = _date.Day.ToString();
        RefreshAppearance();
    }

    void RefreshAppearance()
    {
        _dateLabel.Opacity = _isCurrentMonth ? 1.0 : 0.4;

        if (_isToday)
        {
            _dateLabel.TextColor = Colors.White;
            _dateLabel.BackgroundColor = _currentDayColor;
        }
        else
        {
            _dateLabel.TextColor = _isCurrentMonth ? Colors.Black : Colors.Gray;
            _dateLabel.BackgroundColor = Colors.Transparent;
        }

        BackgroundColor = _isSelected ? _selectedColor : _cellColor;
    }

    void RefreshEvents()
    {
        _eventsStack.Children.Clear();

        if (_events.Count == 0)
            return;

        if (_showCountOnly)
        {
            _eventsStack.Children.Add(new Label
            {
                Text = _events.Count.ToString(),
                FontSize = 10,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.Gray
            });
            return;
        }

        var allDay = _events.Where(e => e.IsAllDay).ToList();
        var timed = _events.Where(e => !e.IsAllDay).OrderBy(e => e.Start).ToList();
        var sorted = allDay.Concat(timed).ToList();

        var toShow = sorted.Take(_maxEvents).ToList();
        var overflow = sorted.Count - _maxEvents;

        foreach (var evt in toShow)
        {
            View view;
            if (_eventTemplate != null)
            {
                view = (View)_eventTemplate.CreateContent();
                view.BindingContext = evt;
            }
            else
            {
                view = CreateDefaultEventView(evt);
            }

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => EventTapped?.Invoke(evt);
            view.GestureRecognizers.Add(tap);
            _eventsStack.Children.Add(view);
        }

        if (overflow > 0)
        {
            var ctx = new CalendarOverflowContext { EventCount = sorted.Count - _maxEvents, Date = _date };
            View overflowView;
            if (_overflowTemplate != null)
            {
                overflowView = (View)_overflowTemplate.CreateContent();
                overflowView.BindingContext = ctx;
            }
            else
            {
                overflowView = new Label
                {
                    Text = $"+{ctx.EventCount} more",
                    FontSize = 10,
                    TextColor = Colors.Gray,
                    Padding = new Thickness(2, 0)
                };
            }
            _eventsStack.Children.Add(overflowView);
        }
    }

    static View CreateDefaultEventView(SchedulerEvent evt)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(3)),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 2,
            Padding = new Thickness(1)
        };

        grid.Add(new BoxView
        {
            Color = evt.Color ?? Colors.CornflowerBlue,
            CornerRadius = 1,
            WidthRequest = 3
        }, 0);

        grid.Add(new Label
        {
            Text = evt.Title,
            FontSize = 10,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        }, 1);

        return grid;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => DayTapped?.Invoke(_date);
        GestureRecognizers.Clear();
        GestureRecognizers.Add(tap);
    }
}
