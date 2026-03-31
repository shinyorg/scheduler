namespace Shiny.Maui.Scheduler.Internal;

internal class CurrentTimeIndicator : ContentView
{
    readonly BoxView _line;
    readonly Label _timeLabel;
    readonly BoxView _dot;

    public CurrentTimeIndicator()
    {
        _dot = new BoxView
        {
            Color = Colors.Red,
            CornerRadius = 4,
            WidthRequest = 8,
            HeightRequest = 8,
            VerticalOptions = LayoutOptions.Center
        };

        _timeLabel = new Label
        {
            FontSize = 9,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Red,
            VerticalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(2, 0, 0, 0)
        };

        _line = new BoxView
        {
            Color = Colors.Red,
            HeightRequest = 2,
            VerticalOptions = LayoutOptions.Center
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            VerticalOptions = LayoutOptions.Start
        };

        grid.Add(_dot, 0);
        grid.Add(_timeLabel, 1);
        grid.Add(_line, 2);

        Content = grid;
    }

    public Color MarkerColor
    {
        set
        {
            _line.Color = value;
            _dot.Color = value;
            _timeLabel.TextColor = value;
        }
    }

    public void UpdateTime(bool use24HourTime)
    {
        var now = DateTime.Now;
        _timeLabel.Text = now.ToString(use24HourTime ? "HH:mm" : "h:mm tt");
    }
}
