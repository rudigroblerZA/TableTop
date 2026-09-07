namespace FactOrFiction;

public partial class FlipPage : ContentPage
{
    record Claim(string Text, bool IsFact, string Why);

    static readonly Claim[] Cards =
    {
        new("Bananas are berries, but strawberries are not.", true,
            "Botanically a banana is a berry. A strawberry is an aggregate accessory fruit."),
        new("The Great Wall of China is visible from space with the naked eye.", false,
            "It is far too narrow. Astronauts have said they cannot pick it out unaided."),
        new("Octopuses have three hearts.", true,
            "Two pump blood through the gills, the third moves it round the rest of the body."),
        new("Goldfish have a memory of about three seconds.", false,
            "They can be trained on tasks they still remember months later."),
        new("Honey found in ancient tombs was still edible.", true,
            "Low water content and high acidity mean it does not spoil.")
    };

    static readonly Color RightInk = Color.FromArgb("#C0DD97");
    static readonly Color RightFill = Color.FromArgb("#27500A");
    static readonly Color WrongInk = Color.FromArgb("#F09595");
    static readonly Color WrongFill = Color.FromArgb("#501313");
    static readonly Color Slab = Color.FromArgb("#2C2C2A");

    const string TickGlyph = "\ueaa5";   // ti-check — replace with the real Tabler codepoint
    const string CrossGlyph = "\ueb55";  // ti-x — replace with the real Tabler codepoint

    int _index;
    int _streak;
    bool _answered;
    bool _flipping;

    public FlipPage()
    {
        InitializeComponent();
        ShowFront();
    }

    void OnFact(object sender, TappedEventArgs e) => Answer(saidFact: true);

    void OnFiction(object sender, TappedEventArgs e) => Answer(saidFact: false);

    async void Answer(bool saidFact)
    {
        if (_answered || _flipping)
            return;

        _answered = true;

        var card = Cards[_index];
        var correct = saidFact == card.IsFact;

        _streak = correct ? _streak + 1 : 0;
        StreakLabel.Text = $"{_streak} streak";

        VerdictLabel.Text = correct ? "Right" : "Wrong";
        VerdictLabel.TextColor = correct ? RightInk : WrongInk;
        VerdictIcon.Text = correct ? TickGlyph : CrossGlyph;
        VerdictIcon.TextColor = correct ? RightInk : WrongInk;
        AnswerLabel.Text = card.IsFact ? "Fact" : "Fiction";
        WhyLabel.Text = card.Why;
        WhyLabel.TextColor = correct ? RightInk : Color.FromArgb("#F7C1C1");

        await Flip(() =>
        {
            FlipCard.BackgroundColor = correct ? RightFill : WrongFill;
            FrontFace.IsVisible = false;
            BackFace.IsVisible = true;
        });

        Choices.IsVisible = false;
        NextButton.IsVisible = true;
    }

    async void OnNext(object sender, TappedEventArgs e)
    {
        if (_flipping)
            return;

        _index = (_index + 1) % Cards.Length;
        NextButton.IsVisible = false;

        await Flip(() =>
        {
            FlipCard.BackgroundColor = Slab;
            BackFace.IsVisible = false;
            FrontFace.IsVisible = true;
            ShowFront();
        });

        Choices.IsVisible = true;
        _answered = false;
    }

    // Half-turn out, swap the face at the edge-on moment, half-turn back in.
    async Task Flip(Action swapFaces)
    {
        _flipping = true;

        // On .NET 8 and 9 these are RotateYTo / the Async suffix does not exist yet.
        await FlipCard.RotateYToAsync(90, 160, Easing.CubicIn);
        swapFaces();
        FlipCard.RotationY = -90;
        await FlipCard.RotateYToAsync(0, 160, Easing.CubicOut);

        _flipping = false;
    }

    void ShowFront()
    {
        ClaimLabel.Text = Cards[_index].Text;
        ProgressLabel.Text = $"card {_index + 1} of {Cards.Length}";
    }
}
