using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using TableTop.Droid.Infrastructure;
using TableTop.Droid.Screens;
using AndroidColor = Android.Graphics.Color;

namespace TableTop.Droid;

/// <summary>
/// The single activity. Hosts a top bar (back affordance + title) over a
/// <see cref="FrameLayout"/> into which the current <see cref="Screen"/>'s view
/// is swapped. Owns a hand-rolled screen back-stack.
/// </summary>
[Activity(
    Label = "TableTop",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
        | ConfigChanges.UiMode | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public sealed class MainActivity : Activity
{
    private readonly Stack<Screen> _stack = new();
    private FrameLayout _container = null!;
    private TextView _titleView = null!;
    private Button _backButton = null!;

    /// <inheritdoc />
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var root = new LinearLayout(this) { Orientation = Orientation.Vertical };
        root.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

        var bar = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        bar.SetBackgroundColor(AndroidColor.ParseColor("#4A2E1D"));
        bar.SetGravity(GravityFlags.CenterVertical);
        bar.SetPadding(Dp(8), Dp(8), Dp(16), Dp(8));
        bar.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

        _backButton = new Button(this) { Text = "‹ Back" };
        _backButton.SetTextColor(AndroidColor.ParseColor("#E3C67F"));
        _backButton.Background = null;
        _backButton.Click += (_, _) => Pop();
        bar.AddView(_backButton);

        _titleView = new TextView(this) { Text = "TableTop", TextSize = 20 };
        _titleView.SetTextColor(AndroidColor.ParseColor("#E3C67F"));
        _titleView.SetPadding(Dp(8), 0, 0, 0);
        bar.AddView(_titleView);

        root.AddView(bar);

        _container = new FrameLayout(this);
        _container.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, 0, 1f);
        root.AddView(_container);

        SetContentView(root);

        if (_stack.Count == 0)
            Push(new ModePickerScreen());
    }

    /// <summary>Pushes a screen and shows it.</summary>
    public void Push(Screen screen)
    {
        screen.Attach(this, new StackNavigator(this));
        _stack.Push(screen);
        ShowTop();
    }

    /// <summary>Pops the current screen, or finishes the activity when it is the last one.</summary>
    public void Pop()
    {
        if (_stack.Count <= 1)
        {
            Finish();
            return;
        }

        _stack.Pop().OnRemoved();
        ShowTop();
    }

    private void ShowTop()
    {
        var screen = _stack.Peek();
        _container.RemoveAllViews();
        _container.AddView(screen.GetView(this));
        _titleView.Text = screen.Title;
        _backButton.Visibility = _stack.Count > 1 ? ViewStates.Visible : ViewStates.Invisible;
    }

    /// <inheritdoc />
    public override void OnBackPressed() => Pop();

    private int Dp(int value) => (int)(value * Resources!.DisplayMetrics!.Density);
}
