namespace ThisOrThat;

public partial class SwipePage : ContentPage
{
    record Pair(string Left, string Right);

    static readonly Pair[] Deck =
    {
        new("Sunrise", "Sunset"),
        new("Reply instantly", "Reply in 3 days"),
        new("Window seat", "Aisle seat"),
        new("Always cold", "Always hot"),
        new("Know the ending", "Be surprised"),
        new("Loud restaurant", "Quiet one")
    };

    const double CommitThreshold = 60;
    const double FlyDistance = 420;

    readonly List<string> _picks = new();
    int _index;
    double _startX;
    bool _animating;

    public SwipePage()
    {
        InitializeComponent();
        Load();
    }

    void Load()
    {
        var remaining = Deck.Length - _index;
        CountLabel.Text = remaining > 0 ? $"{remaining} left" : "all done";

        if (remaining == 0)
        {
            TopCard.IsVisible = false;
            BackCard1.IsVisible = false;
            BackCard2.IsVisible = false;
            DoneCard.IsVisible = true;
            return;
        }

        ThisLabel.Text = Deck[_index].Left;
        ThatLabel.Text = Deck[_index].Right;

        BackCard1.IsVisible = remaining > 1;
        BackCard2.IsVisible = remaining > 2;

        TopCard.TranslationX = 0;
        TopCard.Rotation = 0;
        TopCard.Opacity = 1;
        TopCard.IsVisible = true;
    }

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_animating)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // TotalX is documented as cumulative from the start of the gesture.
                // If you see drift on Android, accumulate deltas here instead.
                _startX = TopCard.TranslationX;
                break;

            case GestureStatus.Running:
                var dx = _startX + e.TotalX;
                TopCard.TranslationX = dx;
                TopCard.Rotation = dx / 22;
                HintLeft.TextColor = dx < -20 ? Color.FromArgb("#AFA9EC") : Color.FromArgb("#888780");
                HintRight.TextColor = dx > 20 ? Color.FromArgb("#F0997B") : Color.FromArgb("#888780");
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                Release();
                break;
        }
    }

    async void Release()
    {
        var dx = TopCard.TranslationX;
        HintLeft.TextColor = Color.FromArgb("#888780");
        HintRight.TextColor = Color.FromArgb("#888780");

        if (Math.Abs(dx) <= CommitThreshold)
        {
            _animating = true;
            await Task.WhenAll(
                TopCard.TranslateTo(0, 0, 180, Easing.SpringOut),
                TopCard.RotateTo(0, 180, Easing.SpringOut));
            _animating = false;
            return;
        }

        var direction = dx > 0 ? 1 : -1;
        _picks.Add(direction > 0 ? Deck[_index].Right : Deck[_index].Left);

        _animating = true;
        await Task.WhenAll(
            TopCard.TranslateTo(direction * FlyDistance, 0, 200, Easing.CubicOut),
            TopCard.RotateTo(direction * 18, 200, Easing.CubicOut),
            TopCard.FadeTo(0, 200));

        _index++;
        Load();
        _animating = false;
    }

    void OnSkip(object sender, TappedEventArgs e)
    {
        if (_animating)
            return;

        if (_index < Deck.Length)
            _index++;
        else
            _index = 0;

        Load();
    }

    async void OnSeeResults(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ResultsPage());
    }
}
