using Android.App;
using Android.Content;
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
// Second launcher entry, for the Android TV home row. MainLauncher above emits
// the usual MAIN/LAUNCHER filter for phones and tablets; this one adds the
// LEANBACK_LAUNCHER category the TV launcher scans for. Same activity, so a TV
// with a touchscreen (some do) still shows just one icon per launcher.
[IntentFilter(
    new[] { Intent.ActionMain },
    Categories = new[] { "android.intent.category.LEANBACK_LAUNCHER" })]
public sealed class MainActivity : Activity
{
    private readonly Stack<Screen> _stack = new();
    private FrameLayout _container = null!;
    private TextView _titleView = null!;
    private Button _backButton = null!;

    /// <summary>
    /// True when running on an Android TV / leanback device. Drives the
    /// 10-foot adjustments: an overscan-safe inset around the whole UI and an
    /// explicit initial D-pad focus every time a screen is shown.
    /// </summary>
    private bool _isTelevision;

    /// <inheritdoc />
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _isTelevision =
            PackageManager?.HasSystemFeature(PackageManager.FeatureLeanback) == true;

        var root = new LinearLayout(this) { Orientation = Orientation.Vertical };
        root.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

        // TV overscan: many panels crop ~5% of every edge. Keep the whole UI
        // clear of that band; phones and tablets get no extra inset.
        if (_isTelevision)
        {
            var inset = Dp(27);
            root.SetPadding(inset, inset, inset, inset);
        }

        var bar = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        bar.SetBackgroundColor(AndroidColor.ParseColor("#4A2E1D"));
        bar.SetGravity(GravityFlags.CenterVertical);
        bar.SetPadding(Dp(8), Dp(8), Dp(16), Dp(8));
        bar.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

        _backButton = new Button(this) { Text = "‹ Back" };
        _backButton.SetTextColor(AndroidColor.ParseColor("#E3C67F"));
        _backButton.Click += (_, _) => Pop();
        // On touch the bare button reads fine; on TV it needs a drawn focus
        // state, so give it the same brass-flip background the screen buttons use.
        if (_isTelevision)
            _backButton.SetBackgroundResource(Resource.Drawable.button_background);
        else
            _backButton.Background = null;
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
        var view = screen.GetView(this);
        _container.AddView(view);
        _titleView.Text = screen.Title;
        _backButton.Visibility = _stack.Count > 1 ? ViewStates.Visible : ViewStates.Invisible;

        // A TV remote has no cursor: without an explicit focus the D-pad is
        // inert until the user guesses a direction that lands somewhere. Hand
        // focus to the screen's first focusable view once it has been laid out.
        if (_isTelevision)
            view.Post(() => view.RequestFocus());
    }

    /// <inheritdoc />
    public override void OnBackPressed() => Pop();

    private int Dp(int value) => (int)(value * Resources!.DisplayMetrics!.Density);
}
