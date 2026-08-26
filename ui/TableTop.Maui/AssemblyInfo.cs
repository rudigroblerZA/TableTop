using Microsoft.Maui.Controls.Xaml;

// ── FORCE COMPILE-TIME XAML VALIDATION ───────────────────────────────────────
//
// Without this, XAML is parsed when a page is NAVIGATED TO, so a property that
// doesn't exist on a control is a runtime crash in the user's hands rather than
// a build error on the developer's machine. That is exactly how
//
//     <Entry Padding="12" CornerRadius="8" />
//
// shipped: Entry supports neither property, xmllint says the file is perfectly
// well-formed XML, and nothing complained until someone tapped Start Game and
// got "Position 43:17. Cannot assign property Padding".
//
// Compiling the XAML turns that into a build failure, which is where it belongs.
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
