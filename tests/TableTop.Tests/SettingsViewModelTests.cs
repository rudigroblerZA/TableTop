using TableTop.Presentation.ViewModels;
using TableTop.Tests.Helpers;

namespace TableTop.Tests;

/// <summary>
/// Zero test references before this. Part of item 2's untested set — the
/// shared ViewModels are a single point of failure for both heads now.
/// </summary>
public sealed class SettingsViewModelTests
{
    private static (SettingsViewModel vm, FakeAppSettings settings, FakeNavigator nav) Build()
    {
        var settings = new FakeAppSettings();
        var nav = new FakeNavigator();
        return (new SettingsViewModel(nav, settings), settings, nav);
    }

    [Fact]
    public void BackCommand_CallsNavigatorGoBack()
    {
        var (vm, _, nav) = Build();
        vm.BackCommand.Execute(null);
        nav.GoBackCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, "dark")]
    [InlineData(1, "light")]
    [InlineData(2, "system")]
    public void ThemeIndex_RoundTripsThroughSettings(int index, string expectedStoredValue)
    {
        var (vm, settings, _) = Build();
        vm.ThemeIndex = index;
        settings.Theme.Should().Be(expectedStoredValue);
        vm.ThemeIndex.Should().Be(index, "reading it back must reproduce the same index");
    }

    [Fact]
    public void FontSizeIndex_MapsToRealPointSizes()
    {
        var (vm, settings, _) = Build();
        vm.FontSizeIndex = 2; // "Large (18)"
        settings.CardFontSize.Should().Be(18);
    }

    [Fact]
    public void FontSizeIndex_ClampsOutOfRangeValues()
    {
        var (vm, settings, _) = Build();
        vm.FontSizeIndex = 99;
        settings.CardFontSize.Should().Be(20, "clamped to the largest defined size, not out of bounds");
    }

    [Fact]
    public void MinDifficultyIndex_RaisingItAboveMax_PullsMaxUpToMatch()
    {
        var (vm, settings, _) = Build();
        settings.MaxDifficulty = 1;
        vm.MinDifficultyIndex = 3;
        settings.MaxDifficulty.Should().Be(3, "the pair must never invert");
    }

    [Fact]
    public void MaxDifficultyIndex_LoweringItBelowMin_PullsMinDownToMatch()
    {
        var (vm, settings, _) = Build();
        settings.MinDifficulty = 3;
        vm.MaxDifficultyIndex = 0;
        settings.MinDifficulty.Should().Be(0);
    }

    [Fact]
    public void TimerIndex_MapsToRealSecondCounts()
    {
        var (vm, settings, _) = Build();
        vm.TimerIndex = 4; // "3 minutes"
        settings.TimerSeconds.Should().Be(180);
    }

    [Fact]
    public void ResetCommand_CallsSettingsResetToDefaults()
    {
        var (vm, settings, _) = Build();
        settings.ShuffleCards = false;
        vm.ResetCommand.Execute(null);
        settings.ResetCount.Should().Be(1);
    }

    [Fact]
    public void ResetToDefaults_RaisesAWholeScreenChangeNotification()
    {
        // Passing null to OnPropertyChanged is the "everything changed"
        // signal a binding engine understands — verified by actually
        // subscribing, not assumed from reading the source.
        var (vm, _, _) = Build();
        string? propertyThatChanged = "not yet raised";
        vm.PropertyChanged += (_, e) => propertyThatChanged = e.PropertyName;

        vm.ResetToDefaults();

        propertyThatChanged.Should().BeNull("null means the whole screen must re-read, not one property");
    }

    [Fact]
    public void ShowCardCount_RoundTripsThroughSettings()
    {
        var (vm, settings, _) = Build();
        vm.ShowCardCount = false;
        settings.ShowCardCount.Should().BeFalse();
        vm.ShowCardCount.Should().BeFalse();
    }

    [Fact]
    public void AutoNextPlayer_RoundTripsThroughSettings()
    {
        // The property WinUI never had until the merge — the whole reason
        // this pair got merged first.
        var (vm, settings, _) = Build();
        vm.AutoNextPlayer = false;
        settings.AutoNextPlayer.Should().BeFalse();
    }

    [Fact]
    public void OptionLists_HaveTheExpectedCounts()
    {
        var (vm, _, _) = Build();
        vm.ThemeOptions.Should().HaveCount(3);
        vm.FontSizeOptions.Should().HaveCount(4);
        vm.DifficultyOptions.Should().HaveCount(4);
        vm.AgeOptions.Should().HaveCount(3);
        vm.TimerOptions.Should().HaveCount(6);
    }
}
