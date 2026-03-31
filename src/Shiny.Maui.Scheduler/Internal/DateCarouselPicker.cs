namespace Shiny.Maui.Scheduler.Internal;

internal class DateCarouselPicker : ContentView
{
    readonly HorizontalStackLayout _stack;
    readonly ScrollView _scroll;
    readonly List<View> _items = [];
    DateOnly _selectedDate;
    int _daysToShow = 1;
    DataTemplate? _itemTemplate;
    bool _buildPending;

    public Action<DateOnly>? DateSelected { get; set; }

    public DateCarouselPicker()
    {
        _stack = new HorizontalStackLayout { Spacing = 0, Padding = new Thickness(4) };
        _scroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = _stack,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never
        };
        Content = _scroll;
    }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            QueueBuild();
        }
    }

    public int DaysToShow
    {
        get => _daysToShow;
        set
        {
            _daysToShow = value;
            QueueBuild();
        }
    }

    public DataTemplate? ItemTemplate
    {
        get => _itemTemplate;
        set
        {
            _itemTemplate = value;
            QueueBuild();
        }
    }

    void QueueBuild()
    {
        if (_buildPending) return;
        _buildPending = true;
        Dispatcher.Dispatch(() =>
        {
            _buildPending = false;
            Build();
        });
    }

    void Build()
    {
        _stack.Children.Clear();
        _items.Clear();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = _selectedDate.AddDays(-14);

        for (var i = 0; i < 29; i++)
        {
            var date = start.AddDays(i);
            var isSelected = date == _selectedDate;
            var isToday = date == today;

            var template = _itemTemplate ?? DefaultTemplates.CreateAppleCalendarDayPickerTemplate();
            var item = (View)template.CreateContent();
            item.BindingContext = new DatePickerItemContext
            {
                Date = date,
                DayNumber = date.Day.ToString(),
                DayName = date.ToString("ddd").ToUpperInvariant(),
                MonthName = date.ToString("MMM").ToUpperInvariant(),
                IsSelected = isSelected,
                IsToday = isToday
            };

            var captured = date;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                _selectedDate = captured;
                DateSelected?.Invoke(captured);
                UpdateSelection();
            };
            item.GestureRecognizers.Add(tap);

            _items.Add(item);
            _stack.Children.Add(item);
        }

        ScrollToSelected();
    }

    void UpdateSelection()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = _selectedDate.AddDays(-14);
        for (var i = 0; i < _items.Count; i++)
        {
            var date = start.AddDays(i);
            var isSelected = date == _selectedDate;
            var isToday = date == today;

            if (_items[i].BindingContext is DatePickerItemContext ctx)
            {
                ctx.IsSelected = isSelected;
                _items[i].BindingContext = null;
                _items[i].BindingContext = ctx;
            }
        }
    }

    async void ScrollToSelected()
    {
        await Task.Delay(50);
        var selectedIdx = 14;
        if (selectedIdx < _items.Count)
            await _scroll.ScrollToAsync(_items[selectedIdx], ScrollToPosition.Center, false);
    }
}
