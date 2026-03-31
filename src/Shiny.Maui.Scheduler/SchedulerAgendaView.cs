using Shiny.Maui.Scheduler.Internal;

namespace Shiny.Maui.Scheduler;

public class SchedulerAgendaView : ContentView
{
    readonly Grid _rootGrid;
    readonly AllDayEventsSection _allDaySection;
    readonly DateCarouselPicker _datePicker;
    readonly ScrollView _scrollView;
    readonly Grid _columnsGrid;
    readonly ContentView _loaderOverlay;
    readonly CurrentTimeIndicator _timeIndicator;
    readonly List<AgendaTimelinePanel> _panels = [];

    CancellationTokenSource? _loadCts;
    IDispatcherTimer? _timer;
    PinchGestureRecognizer? _pinchGesture;
    double _startTimeSlotHeight;

    #region BindableProperties

    public static readonly BindableProperty ProviderProperty = BindableProperty.Create(
        nameof(Provider), typeof(ISchedulerEventProvider), typeof(SchedulerAgendaView),
        propertyChanged: (b, _, _) => ((SchedulerAgendaView)b).OnProviderChanged());

    public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
        nameof(SelectedDate), typeof(DateOnly), typeof(SchedulerAgendaView),
        defaultValue: DateOnly.FromDateTime(DateTime.Today),
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((SchedulerAgendaView)b).OnSelectedDateChanged());

    public static readonly BindableProperty DaysToShowProperty = BindableProperty.Create(
        nameof(DaysToShow), typeof(int), typeof(SchedulerAgendaView), 1,
        propertyChanged: (b, _, _) => ((SchedulerAgendaView)b).Rebuild());

    public static readonly BindableProperty ShowCarouselDatePickerProperty = BindableProperty.Create(
        nameof(ShowCarouselDatePicker), typeof(bool), typeof(SchedulerAgendaView), true,
        propertyChanged: (b, _, n) => ((SchedulerAgendaView)b)._datePicker.IsVisible = (bool)n);

    public static readonly BindableProperty ShowCurrentTimeMarkerProperty = BindableProperty.Create(
        nameof(ShowCurrentTimeMarker), typeof(bool), typeof(SchedulerAgendaView), true);

    public static readonly BindableProperty EventItemTemplateProperty = BindableProperty.Create(
        nameof(EventItemTemplate), typeof(DataTemplate), typeof(SchedulerAgendaView));

    public static readonly BindableProperty DayPickerItemTemplateProperty = BindableProperty.Create(
        nameof(DayPickerItemTemplate), typeof(DataTemplate), typeof(SchedulerAgendaView),
        propertyChanged: (b, _, _) => ((SchedulerAgendaView)b)._datePicker.ItemTemplate =
            ((SchedulerAgendaView)b).DayPickerItemTemplate);

    public static readonly BindableProperty LoaderTemplateProperty = BindableProperty.Create(
        nameof(LoaderTemplate), typeof(DataTemplate), typeof(SchedulerAgendaView));

    public static readonly BindableProperty CurrentTimeMarkerColorProperty = BindableProperty.Create(
        nameof(CurrentTimeMarkerColor), typeof(Color), typeof(SchedulerAgendaView), Colors.Red,
        propertyChanged: (b, _, n) => ((SchedulerAgendaView)b)._timeIndicator.MarkerColor = (Color)n);

    public static readonly BindableProperty TimezoneColorProperty = BindableProperty.Create(
        nameof(TimezoneColor), typeof(Color), typeof(SchedulerAgendaView), Colors.Gray);

    public static readonly BindableProperty DefaultEventColorProperty = BindableProperty.Create(
        nameof(DefaultEventColor), typeof(Color), typeof(SchedulerAgendaView), Colors.CornflowerBlue);

    public static readonly BindableProperty TimeSlotHeightProperty = BindableProperty.Create(
        nameof(TimeSlotHeight), typeof(double), typeof(SchedulerAgendaView), 60.0,
        propertyChanged: (b, _, _) => ((SchedulerAgendaView)b).Rebuild());

    public static readonly BindableProperty MinDateProperty = BindableProperty.Create(
        nameof(MinDate), typeof(DateOnly?), typeof(SchedulerAgendaView));

    public static readonly BindableProperty MaxDateProperty = BindableProperty.Create(
        nameof(MaxDate), typeof(DateOnly?), typeof(SchedulerAgendaView));

    public static readonly BindableProperty AllowPanProperty = BindableProperty.Create(
        nameof(AllowPan), typeof(bool), typeof(SchedulerAgendaView), true,
        propertyChanged: (b, _, n) => ((SchedulerAgendaView)b)._scrollView.Orientation =
            (bool)n ? ScrollOrientation.Vertical : ScrollOrientation.Neither);

    public static readonly BindableProperty AllowZoomProperty = BindableProperty.Create(
        nameof(AllowZoom), typeof(bool), typeof(SchedulerAgendaView), false,
        propertyChanged: (b, _, _) => ((SchedulerAgendaView)b).UpdateZoomGesture());

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

    public int DaysToShow
    {
        get => (int)GetValue(DaysToShowProperty);
        set => SetValue(DaysToShowProperty, Math.Clamp(value, 1, 7));
    }

    public bool ShowCarouselDatePicker
    {
        get => (bool)GetValue(ShowCarouselDatePickerProperty);
        set => SetValue(ShowCarouselDatePickerProperty, value);
    }

    public bool ShowCurrentTimeMarker
    {
        get => (bool)GetValue(ShowCurrentTimeMarkerProperty);
        set => SetValue(ShowCurrentTimeMarkerProperty, value);
    }

    public DataTemplate? EventItemTemplate
    {
        get => (DataTemplate?)GetValue(EventItemTemplateProperty);
        set => SetValue(EventItemTemplateProperty, value);
    }

    public DataTemplate? DayPickerItemTemplate
    {
        get => (DataTemplate?)GetValue(DayPickerItemTemplateProperty);
        set => SetValue(DayPickerItemTemplateProperty, value);
    }

    public DataTemplate? LoaderTemplate
    {
        get => (DataTemplate?)GetValue(LoaderTemplateProperty);
        set => SetValue(LoaderTemplateProperty, value);
    }

    public Color CurrentTimeMarkerColor
    {
        get => (Color)GetValue(CurrentTimeMarkerColorProperty);
        set => SetValue(CurrentTimeMarkerColorProperty, value);
    }

    public Color TimezoneColor
    {
        get => (Color)GetValue(TimezoneColorProperty);
        set => SetValue(TimezoneColorProperty, value);
    }

    public Color DefaultEventColor
    {
        get => (Color)GetValue(DefaultEventColorProperty);
        set => SetValue(DefaultEventColorProperty, value);
    }

    public double TimeSlotHeight
    {
        get => (double)GetValue(TimeSlotHeightProperty);
        set => SetValue(TimeSlotHeightProperty, value);
    }

    public DateOnly? MinDate
    {
        get => (DateOnly?)GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    public DateOnly? MaxDate
    {
        get => (DateOnly?)GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
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

    public SchedulerAgendaView()
    {
        _allDaySection = new AllDayEventsSection();

        _datePicker = new DateCarouselPicker();
        _datePicker.DateSelected = date =>
        {
            if (MinDate.HasValue && date < MinDate.Value) return;
            if (MaxDate.HasValue && date > MaxDate.Value) return;
            SelectedDate = date;
        };

        _timeIndicator = new CurrentTimeIndicator();

        _columnsGrid = new Grid();
        _scrollView = new ScrollView
        {
            Content = _columnsGrid,
            Orientation = ScrollOrientation.Vertical
        };

        _loaderOverlay = new ContentView
        {
            BackgroundColor = Color.FromRgba(255, 255, 255, 200),
            IsVisible = false
        };

        _rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            RowSpacing = 0
        };

        _rootGrid.Add(_allDaySection, 0, 0);
        _rootGrid.Add(_datePicker, 0, 1);

        var contentGrid = new Grid();
        contentGrid.Add(_scrollView);
        contentGrid.Add(_loaderOverlay);
        _rootGrid.Add(contentGrid, 0, 2);

        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        Content = _rootGrid;
        Rebuild();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
            StartTimer();
        else
            StopTimer();
    }

    void StartTimer()
    {
        if (_timer != null) return;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMinutes(1);
        _timer.Tick += (_, _) => UpdateTimeMarker();
        _timer.Start();
    }

    void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    void UpdateTimeMarker()
    {
        if (!ShowCurrentTimeMarker) return;
        // Rebuild will reposition the marker
        Rebuild();
    }

    void UpdateZoomGesture()
    {
        if (_pinchGesture != null)
            _scrollView.GestureRecognizers.Remove(_pinchGesture);

        if (AllowZoom)
        {
            _pinchGesture = new PinchGestureRecognizer();
            _pinchGesture.PinchUpdated += OnPinchUpdated;
            _scrollView.GestureRecognizers.Add(_pinchGesture);
        }
    }

    void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _startTimeSlotHeight = TimeSlotHeight;
                break;
            case GestureStatus.Running:
                TimeSlotHeight = Math.Clamp(_startTimeSlotHeight * e.Scale, 20.0, 200.0);
                break;
        }
    }

    void OnProviderChanged() => LoadEvents();
    void OnSelectedDateChanged()
    {
        _datePicker.SelectedDate = SelectedDate;
        Rebuild();
    }

    void Rebuild()
    {
        _datePicker.SelectedDate = SelectedDate;
        _datePicker.DaysToShow = DaysToShow;
        BuildColumns();
        LoadEvents();
    }

    void BuildColumns()
    {
        _columnsGrid.Children.Clear();
        _columnsGrid.ColumnDefinitions.Clear();
        _panels.Clear();

        for (var i = 0; i < DaysToShow; i++)
        {
            _columnsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var panel = new AgendaTimelinePanel
            {
                TimeSlotHeight = TimeSlotHeight,
                TimezoneColor = TimezoneColor,
                DefaultEventColor = DefaultEventColor,
                EventTemplate = EventItemTemplate,
                EventTapped = OnEventTapped,
                TimeSlotTapped = OnTimeSlotTapped
            };
            _panels.Add(panel);
            _columnsGrid.Add(panel, i);
        }
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
            var startDate = SelectedDate;
            var endDate = startDate.AddDays(DaysToShow);

            var start = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue));
            var end = new DateTimeOffset(endDate.ToDateTime(TimeOnly.MinValue));

            var events = await Provider.GetEvents(start, end);
            if (token.IsCancellationRequested) return;

            var allDayEvents = events.Where(e => e.IsAllDay).ToList();
            _allDaySection.SetEvents(allDayEvents, EventItemTemplate, OnEventTapped);

            var timedEvents = events.Where(e => !e.IsAllDay).ToList();

            for (var i = 0; i < DaysToShow && i < _panels.Count; i++)
            {
                var date = startDate.AddDays(i);
                var dayEvents = timedEvents
                    .Where(e => DateOnly.FromDateTime(e.Start.LocalDateTime) <= date &&
                                DateOnly.FromDateTime(e.End.LocalDateTime) >= date)
                    .ToList();

                var indicator = i == 0 ? _timeIndicator : null;
                _panels[i].Build(date, dayEvents, indicator, ShowCurrentTimeMarker);
            }

            ScrollToCurrentTime();
        }
        catch (TaskCanceledException) { }
        finally
        {
            if (!token.IsCancellationRequested)
                ShowLoader(false);
        }
    }

    async void ScrollToCurrentTime()
    {
        await Task.Delay(100);
        if (SelectedDate == DateOnly.FromDateTime(DateTime.Today))
        {
            var now = DateTime.Now.TimeOfDay.TotalMinutes;
            var scrollY = Math.Max(0, (now - 60) * TimeSlotHeight / 60.0);
            await _scrollView.ScrollToAsync(0, scrollY, false);
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

    void OnEventTapped(SchedulerEvent evt) => Provider?.OnEventSelected(evt);

    void OnTimeSlotTapped(DateTimeOffset time)
    {
        if (Provider == null) return;
        if (!Provider.CanSelectAgendaTime(time)) return;
        Provider.OnAgendaTimeSelected(time);
    }
}
