namespace TruthOrDare;

public partial class LobbyPage : ContentPage
{
    public LobbyPage()
    {
        InitializeComponent();
    }

    void OnAddPlayer(object sender, TappedEventArgs e)
    {
        // Hook up to your player list. Left as a stub so the mockup runs as-is.
    }

    async void OnStartGame(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new PlayPage());
    }
}
