using Microsoft.Maui.Controls.Shapes;

namespace TruthOrDare;

public partial class PlayPage : ContentPage
{
    static readonly string[] Truths =
    {
        "What's the last thing you searched for on your phone?",
        "Who here would you swap lives with for a week?",
        "What's a compliment you've never given anyone in this room?",
        "Describe your worst haircut in three words."
    };

    static readonly string[] Dares =
    {
        "Speak only in questions until your next turn.",
        "Let the person on your left rename your group chat.",
        "Do your best impression of someone in this room.",
        "Text the third person in your recents just the word 'wow'."
    };

    readonly Random _random = new();
    bool _drawn;

    public PlayPage()
    {
        InitializeComponent();
    }

    void OnTruthTapped(object sender, TappedEventArgs e) => Draw(isTruth: true);

    void OnDareTapped(object sender, TappedEventArgs e) => Draw(isTruth: false);

    void Draw(bool isTruth)
    {
        if (_drawn)
            return;

        _drawn = true;

        var winner = isTruth ? TruthCard : DareCard;
        var loser = isTruth ? DareCard : TruthCard;
        var choice = isTruth ? TruthChoice : DareChoice;
        var prompt = isTruth ? TruthPrompt : DarePrompt;
        var promptText = isTruth ? TruthPromptText : DarePromptText;
        var deck = isTruth ? Truths : Dares;

        promptText.Text = deck[_random.Next(deck.Length)];

        loser.IsVisible = false;
        Stage.RowDefinitions[isTruth ? 1 : 0].Height = new GridLength(0);
        Stage.RowSpacing = 0;

        winner.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) };
        choice.IsVisible = false;
        prompt.IsVisible = true;

        Actions.IsVisible = true;
        Note.Text = string.Empty;
    }

    void OnPass(object sender, TappedEventArgs e) => Reset("Passed · Maya is up next");

    void OnDone(object sender, TappedEventArgs e) => Reset("+1 point · Maya is up next");

    async void Reset(string message)
    {
        if (!_drawn)
            return;

        _drawn = false;

        Stage.RowDefinitions[0].Height = GridLength.Star;
        Stage.RowDefinitions[1].Height = GridLength.Star;
        Stage.RowSpacing = 4;

        TruthCard.IsVisible = true;
        DareCard.IsVisible = true;
        TruthCard.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18, 18, 4, 4) };
        DareCard.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4, 4, 18, 18) };

        TruthChoice.IsVisible = true;
        DareChoice.IsVisible = true;
        TruthPrompt.IsVisible = false;
        DarePrompt.IsVisible = false;

        Actions.IsVisible = false;
        Note.Text = message;

        await Task.Delay(1800);

        if (!_drawn)
            Note.Text = string.Empty;
    }
}
