namespace Shiny.Maui.Scheduler.Internal;

internal class CurrentTimeIndicator : ContentView
{
    readonly BoxView _line;
    readonly Label _timeLabel;
    readonly BoxView _dot;

    public CurrentTimeIndicator()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(8)),
                new ColumnDefinition(GridLength.Star)
            },
            HeightRequest = 2,
            VerticalOptions = LayoutOptions.Start
        };

        _dot = new BoxView
        {
            Color = Colors.Red,
            CornerRadius = 4,
            WidthRequest = 8,
            HeightRequest = 8,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, -3, 0, 0)
        };

        _line = new BoxView
        {
            Color = Colors.Red,
            HeightRequest = 2,
            VerticalOptions = LayoutOptions.Center
        };

        _timeLabel = new Label
        {
            FontSize = 9,
            TextColor = Colors.Red,
            IsVisible = false
        };

        grid.Add(_dot, 0);
        grid.Add(_line, 1);

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
}
