using System.Windows.Input;
using Android.Content;
using Android.Views;
using Android.Widget;
using TableTop.Droid;
using AndroidColor = Android.Graphics.Color;

namespace TableTop.Droid.Infrastructure;

/// <summary>
/// Small builders for the view trees the screens assemble by hand. Nothing here
/// is TableTop-specific — it is the boilerplate an AXML file would otherwise
/// carry, kept in code so a screen reads as one method.
/// </summary>
public static class Ui
{
    /// <summary>Baize felt — the shared board colour, also the window background.</summary>
    public static readonly AndroidColor Baize = AndroidColor.ParseColor("#174034");

    /// <summary>Walnut — bars and frames.</summary>
    public static readonly AndroidColor Walnut = AndroidColor.ParseColor("#4A2E1D");

    /// <summary>Brass — headings and accents.</summary>
    public static readonly AndroidColor Brass = AndroidColor.ParseColor("#E3C67F");

    /// <summary>Parchment — body text.</summary>
    public static readonly AndroidColor Parchment = AndroidColor.ParseColor("#F3E9D2");

    /// <summary>Density-independent pixels to raw pixels for the given context.</summary>
    public static int Dp(Context c, int value) => (int)(value * c.Resources!.DisplayMetrics!.Density);

    /// <summary>A vertical <see cref="LinearLayout"/> with uniform padding.</summary>
    public static LinearLayout Column(Context c, int pad = 16)
    {
        var l = new LinearLayout(c) { Orientation = Orientation.Vertical };
        l.SetPadding(Dp(c, pad), Dp(c, pad), Dp(c, pad), Dp(c, pad));
        l.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        return l;
    }

    /// <summary>A horizontal <see cref="LinearLayout"/>.</summary>
    public static LinearLayout Row(Context c)
    {
        var l = new LinearLayout(c) { Orientation = Orientation.Horizontal };
        l.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        return l;
    }

    /// <summary>
    /// Wraps <paramref name="content"/> in the raised card panel (see <see cref="Card"/>)
    /// with a margin around it so the baize shows at the edges, then in a vertical
    /// scroller that fills the screen.
    /// </summary>
    public static ScrollView Scroll(Context c, View content)
    {
        var card = Card(c, content);

        var outer = new LinearLayout(c) { Orientation = Orientation.Vertical };
        outer.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        outer.AddView(card, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        {
            LeftMargin = Dp(c, 12),
            TopMargin = Dp(c, 12),
            RightMargin = Dp(c, 12),
            BottomMargin = Dp(c, 12),
        });

        var s = new ScrollView(c);
        s.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        s.AddView(outer);
        return s;
    }

    /// <summary>The rounded, brass-framed panel every screen's content sits in,
    /// raised slightly off the baize with a drop shadow.</summary>
    public static FrameLayout Card(Context c, View content)
    {
        var card = new FrameLayout(c);
        card.SetBackgroundResource(Resource.Drawable.card_background);
        card.Elevation = Dp(c, 4);
        card.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        card.AddView(content);
        return card;
    }

    /// <summary>A text view. <paramref name="size"/> is in SP.</summary>
    public static TextView Label(Context c, string text = "", float size = 16f, bool bold = false)
    {
        var t = new TextView(c) { Text = text, TextSize = size };
        t.SetTextColor(Parchment);
        if (bold) t.SetTypeface(t.Typeface, Android.Graphics.TypefaceStyle.Bold);
        t.SetPadding(0, Dp(c, 4), 0, Dp(c, 4));
        return t;
    }

    /// <summary>A brass heading.</summary>
    public static TextView Heading(Context c, string text)
    {
        var t = Label(c, text, 22f, bold: true);
        t.SetTextColor(Brass);
        t.SetPadding(0, Dp(c, 8), 0, Dp(c, 12));
        return t;
    }

    /// <summary>A push button: brass-bordered walnut fill, rounded corners, brass press-flip.</summary>
    public static Button Button(Context c, string text)
    {
        var b = new Android.Widget.Button(c) { Text = text };
        b.SetAllCaps(false);
        b.StateListAnimator = null;
        b.SetBackgroundResource(Resource.Drawable.button_background);
        b.SetTextColor(c.Resources!.GetColorStateList(Resource.Color.button_text, c.Theme));
        var pad = Dp(c, 12);
        b.SetPadding(pad, pad, pad, pad);
        b.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
        {
            TopMargin = Dp(c, 6),
            BottomMargin = Dp(c, 6),
        };
        return b;
    }

    /// <summary>A single-line text field with a hint.</summary>
    public static EditText Field(Context c, string hint)
    {
        var e = new EditText(c) { Hint = hint };
        e.SetHintTextColor(AndroidColor.ParseColor("#9FB5AC"));
        e.SetTextColor(Parchment);
        e.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        return e;
    }

    /// <summary>A labelled on/off switch.</summary>
    public static Switch Toggle(Context c, string text)
    {
        var s = new Switch(c) { Text = text };
        s.SetTextColor(Parchment);
        s.SetPadding(0, Dp(c, 10), 0, Dp(c, 10));
        return s;
    }

    /// <summary>A dropdown populated with <paramref name="items"/>.</summary>
    public static Spinner Dropdown(Context c, IReadOnlyList<string> items)
    {
        var s = new Spinner(c);
        var adapter = new ArrayAdapter<string>(
            c, Android.Resource.Layout.SimpleSpinnerItem, items.ToArray());
        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        s.Adapter = adapter;
        s.LayoutParameters = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        return s;
    }

    /// <summary>Wires a button to an <see cref="ICommand"/>: click executes it, and its
    /// enabled state tracks <see cref="ICommand.CanExecute"/>.</summary>
    public static Button OnClick(this Button button, ICommand command)
    {
        button.Click += (_, _) => { if (command.CanExecute(null)) command.Execute(null); };
        command.CanExecuteChanged += (_, _) => button.Enabled = command.CanExecute(null);
        button.Enabled = command.CanExecute(null);
        return button;
    }

    /// <summary>Wires a button to a plain callback.</summary>
    public static Button OnClick(this Button button, Action handler)
    {
        button.Click += (_, _) => handler();
        return button;
    }
}
