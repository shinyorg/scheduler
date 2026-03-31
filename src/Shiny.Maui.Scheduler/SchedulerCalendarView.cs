using Shiny.Maui.Scheduler.Internal;

namespace Shiny.Maui.Scheduler;

public class SchedulerCalendarView : ContentView
{
    readonly Grid _rootGrid;
    readonly Grid _headerGrid;
    readonly Label _monthLabel;
    readonly Grid _dayHeaderGrid;
    readonly Grid _calendarGrid;
    readonly ContentView _loaderOverlay;
    readonly CalendarDayCell[] _cells = new CalendarDayCell[42];
    readonly SwipeGestureRecognizer _swipeLeft;
    readonly SwipeGestureRecognizer _swipeRight;
    PinchGestureRecognizer? _pinchGesture;
    double _currentScale = 1;
    double _startScale = 1;

    CancellationTokenSource? _loadCts;

    #region BindableProperties

    public static readonly BindableProperty ProviderProperty = BindableProperty.Create(
        nameof(Provider), typeof(ISchedulerEventProvider), typeof(SchedulerCalendarView),
        propertyChanged: (b, _, _) => ((SchedulerCalendarView)b).OnProviderChanged());

    public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
        nameof(SelectedDate), typeof(DateOnly), typeof(SchedulerCalendarView),
        defaultValue: DateOnly.FromDateTime(DateTime.Today),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((SchedulerCalendarView)b).OnSelectedDateChanged());

    public static readonly BindableProperty DisplayMonthProperty = BindableProperty.Create(
        nameof(DisplayMonth), typeof(DateOnly), typeof(SchedulerCalendarView),
        defaultValue: DateOnly.FromDateTime(DateTime.Today),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((SchedulerCalendarView)b).OnDisplayMonthChanged());

    public static readonly BindableProperty ShowCalendarCellEventCountOnlyProperty = BindableProperty.Create(
        nameof(ShowCalendarCellEventCountOnly), typeof(bool), typeof(SchedulerCalendarView), false);

    public static readonly BindableProperty EventItemTemplateProperty = BindableProperty.Create(
        nameof(EventItemTemplate), typeof(DataTemplate), typeof(SchedulerCalendarView));

    public static readonly BindableProperty OverflowItemTemplateProperty = BindableProperty.Create(
        nameof(OverflowItemTemplate), typeof(DataTemplate), typeof(SchedulerCalendarView));

    public static readonly BindableProperty LoaderTemplateProperty = BindableProperty.Create(
        nameof(LoaderTemplate), typeof(DataTemplate), typeof(SchedulerCalendarView));

    public static readonly BindableProperty MaxEventsPerCellProperty = BindableProperty.Create(
        nameof(MaxEventsPerCell), typeof(int), typeof(SchedulerCalendarView), 3);

    public static readonly BindableProperty CalendarCellColorProperty = BindableProperty.Create(
        nameof(CalendarCellColor), typeof(Color), typeof(SchedulerCalendarView), Colors.White);

    public static readonly BindableProperty CalendarCellSelectedColorProperty = BindableProperty.Create(
        nameof(CalendarCellSelectedColor), typeof(Color), typeof(SchedulerCalendarView), Colors.LightBlue);

    public static readonly BindableProperty CurrentDayColorProperty = BindableProperty.Create(
        nameof(CurrentDayColor), typeof(Color), typeof(SchedulerCalendarView), Colors.DodgerBlue);

    public static readonly BindableProperty FirstDayOfWeekProperty = BindableProperty.Create(
        nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(SchedulerCalendarView), DayOfWeek.Sunday,
        propertyChanged: (b, _, _) => ((SchedulerCalendarView)b).RebuildCalendar());

    public static readonly BindableProperty AllowPanProperty = BindableProperty.Create(
        nameof(AllowPan), typeof(bool), typeof(SchedulerCalendarView), true,
        propertyChanged: (b, _, _) => ((SchedulerCalendarView)b).UpdateGestures());

    public static readonly BindableProperty AllowZoomProperty = BindableProperty.Create(
        nameof(AllowZoom), typeof(bool), typeof(SchedulerCalendarView), false,
        propertyChanged: (b, _, _) => ((SchedulerCalendarView)b).UpdateGestures());

    public ISchedulerEventProvider? Provider
    {
        get => (ISchedulerEventProvider?)GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    public DateOnly SelectedDate
    {
        get => (DateOnly)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public DateOnly DisplayMonth
    {
        get => (DateOnly)GetValue(DisplayMonthProperty);
        set => SetValue(DisplayMonthProperty, value);
    }

    public bool ShowCalendarCellEventCountOnly
    {
        get => (bool)GetValue(ShowCalendarCellEventCountOnlyProperty);
        set => SetValue(ShowCalendarCellEventCountOnlyProperty, value);
    }

    public DataTemplate? EventItemTemplate
    {
        get => (DataTemplate?)GetValue(EventItemTemplateProperty);
        set => SetValue(EventItemTemplateProperty, value);
    }

    public DataTemplate? OverflowItemTemplate
    {
        get => (DataTemplate?)GetValue(OverflowItemTemplateProperty);
        set => SetValue(OverflowItemTemplateProperty, value);
    }

    public DataTemplate? LoaderTemplate
    {
        get => (DataTemplate?)GetValue(LoaderTemplateProperty);
        set => SetValue(LoaderTemplateProperty, value);
    }

    public int MaxEventsPerCell
    {
        get => (int)GetValue(MaxEventsPerCellProperty);
        set => SetValue(MaxEventsPerCellProperty, value);
    }

    public Color CalendarCellColor
    {
        get => (Color)GetValue(CalendarCellColorProperty);
        set => SetValue(CalendarCellColorProperty, value);
    }

    public Color CalendarCellSelectedColor
    {
        get => (Color)GetValue(CalendarCellSelectedColorProperty);
        set => SetValue(CalendarCellSelectedColorProperty, value);
    }

    public Color CurrentDayColor
    {
        get => (Color)GetValue(CurrentDayColorProperty);
        set => SetValue(CurrentDayColorProperty, value);
    }

    public DayOfWeek FirstDayOfWeek
    {
        get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    public bool AllowPan
    {
        get => (bool)GetValue(AllowPanProperty);
        set => SetValue(AllowPanProperty, value);
    }

    public bool AllowZoom
    {
        get => (bool)GetValue(AllowZoomProperty);
        set => SetValue(AllowZoomProperty, value);
    }

    #endregion

    public SchedulerCalendarView()
    {
        _monthLabel = new Label
        {
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        var prevButton = new Button { Text = "◀", FontSize = 16, BackgroundColor = Colors.Transparent, WidthRequest = 44, BorderWidth = 0 };
        prevButton.Clicked += (_, _) => NavigateMonth(-1);

        var nextButton = new Button { Text = "▶", FontSize = 16, BackgroundColor = Colors.Transparent, WidthRequest = 44, BorderWidth = 0 };
        nextButton.Clicked += (_, _) => NavigateMonth(1);

        _headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(44)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(44))
            },
            HeightRequest = 44
        };
        _headerGrid.Add(prevButton, 0);
        _headerGrid.Add(_monthLabel, 1);
        _headerGrid.Add(nextButton, 2);

        _dayHeaderGrid = new Grid { HeightRequest = 30 };
        for (var i = 0; i < 7; i++)
            _dayHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        _calendarGrid = new Grid();
        for (var i = 0; i < 7; i++)
            _calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (var i = 0; i < 6; i++)
            _calendarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        for (var i = 0; i < 42; i++)
        {
            var cell = new CalendarDayCell();
            cell.DayTapped = OnDayTapped;
            cell.EventTapped = OnEventTapped;
            _cells[i] = cell;
            _calendarGrid.Add(cell, i % 7, i / 7);
        }

        _loaderOverlay = new ContentView
        {
            BackgroundColor = Color.FromRgba(255, 255, 255, 200),
            IsVisible = false
        };

        _rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(44)),
                new RowDefinition(new GridLength(30)),
                new RowDefinition(GridLength.Star)
            },
            RowSpacing = 0
        };
        _rootGrid.Add(_headerGrid, 0, 0);
        _rootGrid.Add(_dayHeaderGrid, 0, 1);
        _rootGrid.Add(_calendarGrid, 0, 2);
        _rootGrid.Add(_loaderOverlay, 0, 2);

        _swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        _swipeLeft.Swiped += (_, _) => NavigateMonth(1);
        _swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        _swipeRight.Swiped += (_, _) => NavigateMonth(-1);

        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        Content = _rootGrid;
        UpdateGestures();
        RebuildCalendar();
    }

    void UpdateGestures()
    {
        _calendarGrid.GestureRecognizers.Remove(_swipeLeft);
        _calendarGrid.GestureRecognizers.Remove(_swipeRight);

        if (AllowPan)
        {
            _calendarGrid.GestureRecognizers.Add(_swipeLeft);
            _calendarGrid.GestureRecognizers.Add(_swipeRight);
        }

        if (_pinchGesture != null)
            _calendarGrid.GestureRecognizers.Remove(_pinchGesture);

        if (AllowZoom)
        {
            _pinchGesture = new PinchGestureRecognizer();
            _pinchGesture.PinchUpdated += OnPinchUpdated;
            _calendarGrid.GestureRecognizers.Add(_pinchGesture);
        }
        else
        {
            _calendarGrid.Scale = 1;
            _currentScale = 1;
        }
    }

    void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _startScale = _currentScale;
                break;
            case GestureStatus.Running:
                _currentScale = Math.Clamp(_startScale * e.Scale, 0.5, 3.0);
                _calendarGrid.Scale = _currentScale;
                break;
        }
    }

    void NavigateMonth(int direction)
    {
        var current = DisplayMonth;
        DisplayMonth = current.AddMonths(direction);
    }

    void OnProviderChanged() => LoadEvents();
    void OnSelectedDateChanged() => UpdateCellSelection();
    void OnDisplayMonthChanged() => RebuildCalendar();

    void RebuildCalendar()
    {
        var dm = DisplayMonth;
        _monthLabel.Text = new DateTime(dm.Year, dm.Month, 1).ToString("MMMM yyyy");

        BuildDayHeaders();
        BuildDayCells();
        LoadEvents();
    }

    void BuildDayHeaders()
    {
        _dayHeaderGrid.Children.Clear();
        var names = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
        var first = (int)FirstDayOfWeek;

        for (var i = 0; i < 7; i++)
        {
            var idx = (first + i) % 7;
            var lbl = new Label
            {
                Text = names[idx],
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Colors.Gray
            };
            _dayHeaderGrid.Add(lbl, i);
        }
    }

    void BuildDayCells()
    {
        var dm = DisplayMonth;
        var firstOfMonth = new DateOnly(dm.Year, dm.Month, 1);
        var firstDayOffset = ((int)firstOfMonth.DayOfWeek - (int)FirstDayOfWeek + 7) % 7;
        var startDate = firstOfMonth.AddDays(-firstDayOffset);
        var today = DateOnly.FromDateTime(DateTime.Today);

        for (var i = 0; i < 42; i++)
        {
            var date = startDate.AddDays(i);
            var cell = _cells[i];
            cell.Date = date;
            cell.IsCurrentMonth = date.Month == dm.Month && date.Year == dm.Year;
            cell.IsToday = date == today;
            cell.IsSelected = date == SelectedDate;
            cell.MaxEvents = MaxEventsPerCell;
            cell.ShowCountOnly = ShowCalendarCellEventCountOnly;
            cell.EventTemplate = EventItemTemplate;
            cell.OverflowTemplate = OverflowItemTemplate;
            cell.CellColor = CalendarCellColor;
            cell.SelectedColor = CalendarCellSelectedColor;
            cell.CurrentDayColor = CurrentDayColor;
            cell.Events = [];
        }
    }

    void UpdateCellSelection()
    {
        for (var i = 0; i < 42; i++)
            _cells[i].IsSelected = _cells[i].Date == SelectedDate;
    }

    async void LoadEvents()
    {
        if (Provider == null) return;

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        ShowLoader(true);

        try
        {
            var dm = DisplayMonth;
            var firstOfMonth = new DateOnly(dm.Year, dm.Month, 1);
            var firstDayOffset = ((int)firstOfMonth.DayOfWeek - (int)FirstDayOfWeek + 7) % 7;
            var startDate = firstOfMonth.AddDays(-firstDayOffset);
            var endDate = startDate.AddDays(42);

            var start = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue));
            var end = new DateTimeOffset(endDate.ToDateTime(TimeOnly.MinValue));

            var events = await Provider.GetEvents(start, end);
            if (token.IsCancellationRequested) return;

            var grouped = events
                .SelectMany(e =>
                {
                    var results = new List<(DateOnly Date, SchedulerEvent Event)>();
                    var eventStart = DateOnly.FromDateTime(e.Start.LocalDateTime);
                    var eventEnd = DateOnly.FromDateTime(e.End.LocalDateTime);
                    if (e.End.LocalDateTime.TimeOfDay == TimeSpan.Zero && eventEnd > eventStart)
                        eventEnd = eventEnd.AddDays(-1);

                    for (var d = eventStart; d <= eventEnd; d = d.AddDays(1))
                        results.Add((d, e));
                    return results;
                })
                .GroupBy(x => x.Date)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<SchedulerEvent>)g.Select(x => x.Event).ToList());

            for (var i = 0; i < 42; i++)
            {
                if (grouped.TryGetValue(_cells[i].Date, out var dayEvents))
                    _cells[i].Events = dayEvents;
                else
                    _cells[i].Events = [];
            }
        }
        catch (TaskCanceledException) { }
        finally
        {
            if (!token.IsCancellationRequested)
                ShowLoader(false);
        }
    }

    void ShowLoader(bool show)
    {
        if (show && _loaderOverlay.Content == null)
        {
            var template = LoaderTemplate ?? DefaultTemplates.CreateLoaderTemplate();
            _loaderOverlay.Content = (View)template.CreateContent();
        }
        _loaderOverlay.IsVisible = show;
    }

    void OnDayTapped(DateOnly date)
    {
        if (Provider != null && !Provider.CanCalendarSelect(date))
            return;

        SelectedDate = date;

        if (date.Month != DisplayMonth.Month || date.Year != DisplayMonth.Year)
            DisplayMonth = new DateOnly(date.Year, date.Month, 1);

        Provider?.OnCalendarDateSelected(date);
    }

    void OnEventTapped(SchedulerEvent evt)
    {
        Provider?.OnEventSelected(evt);
    }
}
