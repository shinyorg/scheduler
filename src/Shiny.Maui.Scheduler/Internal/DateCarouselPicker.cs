namespace Shiny.Maui.Scheduler.Internal;

internal class DateCarouselPicker : ContentView
{
    readonly HorizontalStackLayout _stack;
    readonly ScrollView _scroll;
    DateOnly _selectedDate;
    int _daysToShow = 1;
    readonly List<Button> _buttons = [];

    public Action<DateOnly>? DateSelected { get; set; }

    public DateCarouselPicker()
    {
        _stack = new HorizontalStackLayout { Spacing = 4, Padding = new Thickness(4) };
        _scroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = _stack,
            HeightRequest = 50
        };
        Content = _scroll;
    }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            Build();
        }
    }

    public int DaysToShow
    {
        get => _daysToShow;
        set
        {
            _daysToShow = value;
            Build();
        }
    }

    void Build()
    {
        _stack.Children.Clear();
        _buttons.Clear();

        var start = _selectedDate.AddDays(-14);

        for (var i = 0; i < 29; i++)
        {
            var date = start.AddDays(i);
            var isSelected = date == _selectedDate;
            var btn = new Button
            {
                Text = $"{date:ddd}\n{date.Day}",
                FontSize = 11,
                WidthRequest = 50,
                HeightRequest = 44,
                Padding = new Thickness(2),
                CornerRadius = 8,
                BackgroundColor = isSelected ? Colors.DodgerBlue : Colors.Transparent,
                TextColor = isSelected ? Colors.White : Colors.Black,
                BorderWidth = 0
            };
            var captured = date;
            btn.Clicked += (_, _) =>
            {
                _selectedDate = captured;
                DateSelected?.Invoke(captured);
                UpdateSelection();
            };
            _buttons.Add(btn);
            _stack.Children.Add(btn);
        }

        ScrollToSelected();
    }

    void UpdateSelection()
    {
        var start = _selectedDate.AddDays(-14);
        for (var i = 0; i < _buttons.Count; i++)
        {
            var date = start.AddDays(i);
            var isSelected = date == _selectedDate;
            _buttons[i].BackgroundColor = isSelected ? Colors.DodgerBlue : Colors.Transparent;
            _buttons[i].TextColor = isSelected ? Colors.White : Colors.Black;
        }
    }

    async void ScrollToSelected()
    {
        await Task.Delay(50);
        var selectedIdx = 14;
        if (selectedIdx < _buttons.Count)
            await _scroll.ScrollToAsync(_buttons[selectedIdx], ScrollToPosition.Center, false);
    }
}
