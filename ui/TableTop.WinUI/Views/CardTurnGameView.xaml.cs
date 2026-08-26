using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TableTop.Presentation.ViewModels;

namespace TableTop.WinUI.Views;

/// <summary>Interaction logic for <see cref="CardTurnGameView"/>.</summary>
public sealed partial class CardTurnGameView : UserControl
{
    private bool _flipping;

    /// <summary>Initialises the view.</summary>
    public CardTurnGameView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) => PlayDealInIfCardAlreadyPresent();
    }

    /// <summary>
    /// Re-subscribes the deal animation to whichever ViewModel is current —
    /// the view is reused across sessions, so the old VM's handler must be
    /// dropped, not just added to on top of it.
    /// </summary>
    private CardTurnGameViewModel? _subscribedVm;

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (_subscribedVm is not null)
            _subscribedVm.PropertyChanged -= OnViewModelPropertyChanged;

        _subscribedVm = args.NewValue as CardTurnGameViewModel;
        if (_subscribedVm is not null)
            _subscribedVm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // CardTitle changes exactly when a genuinely new card is dealt — never
        // on a flip, which only raises CardBodyText and FlipButtonText (see
        // Flip() in GameViewModels.cs). That's what keeps DealIn from firing a
        // second time mid-turn when the player just turns the same card over.
        if (e.PropertyName == nameof(CardTurnGameViewModel.CardTitle))
            DealIn.Begin();
    }

    /// <summary>Plays the deal-in once for whatever card is already showing when the view first loads, not only for ones that arrive afterward.</summary>
    private void PlayDealInIfCardAlreadyPresent()
    {
        if (DataContext is CardTurnGameViewModel { CardTitle.Length: > 0 })
            DealIn.Begin();
    }

    /// <summary>
    /// Turns the card over.
    ///
    /// WinUI has no CSS-style backface-visibility, so a true 3D flip isn't
    /// available. Squeezing the card horizontally to edge-on, swapping the
    /// text at the moment it's invisible, then opening the other face out
    /// again reads as a turn — the swap is never seen, which is the whole
    /// trick. The ViewModel is untouched: it still just toggles its text, and
    /// this only decides WHEN.
    /// </summary>
    private void OnFlipClick(object sender, RoutedEventArgs e) => TryFlip();

    /// <summary>Handles the F-key accelerator on the flip button — same guard, same animation, same path as a click.</summary>
    private void OnFlipKeyboardAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        TryFlip();
        args.Handled = true;
    }

    private void TryFlip()
    {
        if (_flipping) return;                                   // ignore double taps mid-turn
        if (DataContext is not CardTurnGameViewModel vm) return;
        if (!vm.FlipCommand.CanExecute(null)) return;

        _flipping = true;

        void OnHalfway(object? s, object? args)
        {
            FlipOut.Completed -= OnHalfway;
            vm.FlipCommand.Execute(null);                        // swap while edge-on
            FlipIn.Completed += OnFinished;
            FlipIn.Begin();
        }

        void OnFinished(object? s, object? args)
        {
            FlipIn.Completed -= OnFinished;
            _flipping = false;
        }

        FlipOut.Completed += OnHalfway;
        FlipOut.Begin();
    }
}
