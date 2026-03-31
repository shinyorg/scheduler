namespace Shiny.Maui.Scheduler;

public static class DefaultTemplates
{
    public static DataTemplate CreateEventItemTemplate() => new(() =>
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(4)),
                new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(2),
            ColumnSpacing = 4
        };

        var colorBar = new BoxView { CornerRadius = 2, Color = Colors.CornflowerBlue };
        colorBar.SetBinding(BoxView.ColorProperty, static (SchedulerEvent e) => e.Color);

        var titleLabel = new Label
        {
            FontSize = 11,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        };
        titleLabel.SetBinding(Label.TextProperty, static (SchedulerEvent e) => e.Title);

        grid.Add(colorBar, 0);
        grid.Add(titleLabel, 1);

        return grid;
    });

    public static DataTemplate CreateOverflowTemplate() => new(() =>
    {
        var label = new Label
        {
            FontSize = 10,
            TextColor = Colors.Gray,
            Padding = new Thickness(6, 0)
        };
        label.SetBinding(Label.TextProperty, static (CalendarOverflowContext c) => c.EventCount, stringFormat: "+{0} more");
        return label;
    });

    public static DataTemplate CreateLoaderTemplate() => new(() =>
    {
        var stack = new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Spacing = 8,
            Children =
            {
                new ActivityIndicator { IsRunning = true, Color = Colors.CornflowerBlue },
                new Label
                {
                    Text = "Loading...",
                    FontSize = 12,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Colors.Gray
                }
            }
        };
        return stack;
    });

    public static DataTemplate CreateCalendarListDayHeaderTemplate() => new(() =>
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(12, 8),
            BackgroundColor = Color.FromRgba(240, 240, 240, 255),
            ColumnSpacing = 8
        };

        var dot = new BoxView
        {
            WidthRequest = 8,
            HeightRequest = 8,
            CornerRadius = 4,
            Color = Colors.DodgerBlue,
            VerticalOptions = LayoutOptions.Center
        };
        dot.SetBinding(VisualElement.IsVisibleProperty, static (CalendarListDayGroup g) => g.IsToday);

        var label = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center
        };
        label.SetBinding(Label.TextProperty, static (CalendarListDayGroup g) => g.DateDisplay);

        grid.Add(dot, 0);
        grid.Add(label, 1);

        return grid;
    });

    public static DataTemplate CreateCalendarListEventItemTemplate() => new(() =>
    {
        var colorBar = new BoxView
        {
            WidthRequest = 4,
            CornerRadius = 2,
            Color = Colors.CornflowerBlue
        };
        colorBar.SetBinding(BoxView.ColorProperty, static (SchedulerEvent e) => e.Color);

        var titleLabel = new Label
        {
            FontSize = 14,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        };
        titleLabel.SetBinding(Label.TextProperty, static (SchedulerEvent e) => e.Title);

        var descLabel = new Label
        {
            FontSize = 11,
            TextColor = Colors.Gray,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        };
        descLabel.SetBinding(Label.TextProperty, static (SchedulerEvent e) => e.Description);

        var timeLabel = new Label
        {
            FontSize = 11,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };
        timeLabel.SetBinding(Label.TextProperty, static (SchedulerEvent e) => e.Start, stringFormat: "{0:h:mm tt}");

        var textStack = new VerticalStackLayout
        {
            Spacing = 2,
            Children = { titleLabel, descLabel }
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(4)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Padding = new Thickness(12, 8),
            ColumnSpacing = 8
        };

        grid.Add(colorBar, 0);
        grid.Add(textStack, 1);
        grid.Add(timeLabel, 2);

        var border = new Border
        {
            Content = grid,
            Stroke = Color.FromRgba(230, 230, 230, 255),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Margin = new Thickness(12, 2),
            BackgroundColor = Colors.White
        };

        return border;
    });
}
