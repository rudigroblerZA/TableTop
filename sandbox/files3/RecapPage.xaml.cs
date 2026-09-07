namespace FactOrFiction;

public partial class RecapPage : ContentPage
{
    public RecapPage()
    {
        InitializeComponent();
    }

    async void OnPlayAgain(object sender, TappedEventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}
