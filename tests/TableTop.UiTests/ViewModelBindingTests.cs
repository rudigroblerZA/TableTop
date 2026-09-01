using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace TableTop.UiTests;

/// <summary>
/// The first tests that touch the UI layer at all.
///
/// Until this project existed, <c>TableTop.Tests</c> referenced only Core, Games
/// and Hosting: not one of its tests touched a ViewModel. That's backlog G.4,
/// and it matters because the failures this layer produces are silent.
///
/// Originally written against WPF, and repointed to WinUI when WPF was removed.
/// The tests are reflection-driven rather than one-per-ViewModel, so that was a
/// change of assembly rather than a rewrite — which is the payoff for writing
/// them that way. A
/// binding to a name that doesn't exist, or to one that's <c>static</c>, doesn't
/// throw and doesn't warn — the control renders EMPTY. A command left null is a
/// button that does nothing. Both look like data problems, and both have reached
/// shipped builds here before.
///
/// Scope is deliberately split with <c>scripts/check-xaml-bindings.py</c>:
///
///   • the script owns BINDING NAMES. It resolves a name against every type in
///     the head and in src/, which is what you want when a DataTemplate or a
///     nested DataContext means a binding inside FooView is not addressing
///     FooViewModel at all. A type-level version of that check was tried here
///     and removed: without a real XAML parser it cannot tell which object a
///     binding actually addresses, and it reported five correct bindings in
///     CardEditorView as broken. A test that cries wolf gets disabled.
///
///   • these tests own RUNTIME BEHAVIOUR — that commands exist and that
///     properties notify. Neither is visible to any static check, and neither
///     was covered by anything at all before now.
///
/// The tests are reflection-driven rather than one-per-ViewModel on purpose, so
/// a newly added ViewModel is covered the moment it appears rather than when
/// someone remembers to write a test for it.
/// </summary>
/// <summary>
/// The two assemblies these tests sweep, deliberately held in their OWN type.
///
/// <para>
/// <b>This separation is the fix for backlog N.7, and it is not cosmetic.</b>
/// These were <c>static readonly</c> fields of <see cref="ViewModelBindingTests"/>.
/// That class is <c>beforefieldinit</c>, so its initializer runs at the first
/// static access — and the first access happened on a THREAD-POOL thread,
/// inside <c>WithDeadline(() =&gt; ViewModelTypes().ToList())</c>, because
/// <c>ViewModelTypes()</c> reads <c>UiAssembly</c>.
/// </para>
///
/// <para>
/// The CI hang dump (run 33426915180) shows both ends of the resulting block:
/// </para>
///
/// <code>
/// thread 0x2078   [InlinedCallFrame]            ← native, never returns
///                 InitClassSlow
///                 ViewModelTypes()
///                 &lt;Every_settable_property_raises_PropertyChanged&gt;b__7_0()
///
/// thread 0x1be4   ViewModelBindingTests..cctor()  ← waiting for 0x2078
///                 InitClassSlow
///                 &lt;WithDeadline&gt;d__18.MoveNext()
///                 Every_settable_property_raises_PropertyChanged()
/// </code>
///
/// <para>
/// The pool thread holds the type-initialization lock while it loads the WinUI
/// app assembly, which does not return on a runner with no interactive desktop
/// session. The test thread then needs the SAME type initialized to read
/// <c>PropertyDeadline</c> for its <c>Task.Delay</c> — so it blocks on that
/// lock, and <b>the deadline never starts counting</b>.
/// </para>
///
/// <para>
/// That is why three rounds of adding guards changed nothing and why no
/// deadline ever fired: every guard lived in the class whose initializer was
/// stuck. Holding the risky load in a separate type means the deadline
/// machinery initializes independently, the timer runs, and a load that hangs
/// is reported as a named failure in seconds instead of a five-minute abort.
/// </para>
/// </summary>
internal static class ScannedAssemblies
{
    internal static readonly Assembly Ui = typeof(TableTop.WinUI.Infrastructure.Navigator).Assembly;

    internal static readonly Assembly Presentation =
        typeof(TableTop.Presentation.ViewModels.SettingsViewModel).Assembly;
}

public sealed class ViewModelBindingTests
{
    // Anchored on Navigator, which is a real WinUI type. This used to name
    // TableTop.WinUI.Infrastructure.ViewModelBase, which stopped existing when
    // ViewModelBase moved to TableTop.Presentation during the shared-ViewModel
    // migration — so this file has not compiled since. It went unnoticed because
    // TableTop.UiTests needs the WinUI SDK and is skipped everywhere the suite
    // usually runs; see backlog item 2.
    //
    // The anchor must live in the WinUI assembly, not in Presentation: the
    // view-pairing lookups below (ResolveViewModel, EnumerateViews) reflect
    // over UiAssembly specifically to pair FooView with FooViewModel against
    // WinUI's own Views folder, and pointing it at Presentation would silently
    // scan the wrong assembly and find nothing to check.
    private static Assembly UiAssembly => ScannedAssemblies.Ui;

    // ViewModelTypes() needs a second assembly, and getting a real build
    // environment for the first time (backlog item 2) is what surfaced why:
    // every ViewModel actually declared in TableTop.WinUI — the picker chain
    // (IntroViewModel, ArchetypePickerViewModel, SubArchetypePickerViewModel,
    // GameSelectionViewModel) and UnsupportedModeViewModel — is fully
    // immutable, get-only properties and commands assigned once in the
    // constructor. Every settable, PropertyChanged-raising ViewModel
    // (SettingsViewModel, CardTurnGameViewModel, MillionaireGameViewModel,
    // PlayerSetupViewModel, …) lives in TableTop.Presentation instead. Scanning
    // UiAssembly alone meant Every_settable_property_raises_PropertyChanged
    // could never exercise a single property — not because nothing was
    // broken, but because the one assembly it looked at has nothing mutable
    // to break. TableTop.WinUI already references Presentation (it's how
    // WinUI consumes these ViewModels at all), so this assembly is already
    // loaded in the test host; scanning it too costs nothing extra.
    private static Assembly PresentationAssembly => ScannedAssemblies.Presentation;

    /// <summary>
    /// Views whose ViewModel doesn't follow the <c>FooView</c> → <c>FooViewModel</c>
    /// convention.
    ///
    /// Empty for WinUI, whose eleven views map one-to-one. It was not empty for
    /// WPF, which needed entries for its shell window and a mismatched school
    /// game view — kept here because a new view that breaks the convention still
    /// needs somewhere to declare it.
    /// </summary>
    private static readonly Dictionary<string, string?> ViewToViewModelOverrides = new(StringComparer.Ordinal);

    /// <summary>
    /// How long a single property access — read or write — gets before it is
    /// reported as blocking.
    ///
    /// Generous by three orders of magnitude: the whole suite ran 2/2 in 361ms
    /// on the CI job's own configuration, so no healthy accessor comes near it.
    /// It exists to convert an unbounded hang into a named failure — see
    /// <see cref="WithDeadline{T}"/> and backlog N.7.
    /// </summary>
    private static readonly TimeSpan PropertyDeadline = TimeSpan.FromSeconds(5);

    private static readonly Regex Binding = new(
        @"\{(?:x:Bind|Binding)\s+(?:Path\s*=\s*)?(?<path>[A-Za-z_][\w\.]*)(?<rest>[^}]*)",
        RegexOptions.Compiled);

    private static readonly Regex Redirected = new(
        @"\b(?:RelativeSource|ElementName|Source)\s*=", RegexOptions.Compiled);

    // ── Commands ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Every_command_property_is_non_null_after_construction()
    {
        var failures = new List<string>();
        var constructed = 0;

        // On the deadline like the sweep in the other test. This one reached
        // ViewModelTypes() unguarded, so whichever of the two Facts xUnit ran
        // first would hang — the aborted runs only ever named the other one
        // because that is the order they happened to run in (backlog N.7).
        foreach (var vmType in await DiscoverViewModelTypesAsync())
        {
            if (!TryConstruct(vmType, out var vm, out _)) continue;
            constructed++;

            foreach (var p in vmType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => typeof(ICommand).IsAssignableFrom(p.PropertyType)))
            {
                object? value;
                try { value = p.GetValue(vm); }
                catch (TargetInvocationException ex) { failures.Add($"{vmType.Name}.{p.Name} threw {ex.InnerException?.GetType().Name}"); continue; }

                if (value is null)
                    failures.Add($"{vmType.Name}.{p.Name} is null — any button bound to it is dead, silently");
            }
        }

        constructed.Should().BeGreaterThan(0,
            "no ViewModel could be constructed at all, so this test proved nothing — check TryConstruct");

        failures.Should().BeEmpty(
            $"commands must be assigned in the constructor. {string.Join("\n  ", failures)}");
    }

    // ── Change notification ───────────────────────────────────────────────────

    /// <summary>
    /// Every settable property must notify — and must return.
    ///
    /// <para>
    /// <b>Every call this test makes into product code is on a deadline
    /// (backlog N.7).</b> This test hung CI indefinitely, and two rounds of
    /// guessing narrowed it the slow way. Guarding only the write left
    /// the run hanging for exactly as long, because
    /// <see cref="TryMakeDistinctValue"/> calls <c>p.GetValue</c> first, to
    /// pick a distinct candidate. Guarding both accessors then produced the
    /// same five-minute hang with <i>neither</i> deadline firing — which
    /// clears both accessors and puts the fault in the type sweep or in
    /// construction. Those are now on the deadline as well, so whatever
    /// blocks has to name itself.
    /// </para>
    ///
    /// <para>
    /// One inference to avoid repeating: that
    /// <see cref="Every_command_property_is_non_null_after_construction"/>
    /// "passes, so construction is fine". A hung run prints no per-test
    /// results, and xUnit does not order the facts in a class, so the aborted
    /// runs never showed that test running at all. Blame names the test in
    /// flight; it certifies nothing about any other.
    /// </para>
    ///
    /// <para>
    /// This is an assertion the test should always have carried rather than
    /// scaffolding: an accessor that blocks freezes the UI thread in the app.
    /// It already asserted that a setter must notify; that both accessors must
    /// <i>return</i> is the same class of claim.
    /// </para>
    /// </summary>
    [Fact]
    public async Task Every_settable_property_raises_PropertyChanged()
    {
        var failures = new List<string>();
        var exercised = 0;

        foreach (var vmType in await DiscoverViewModelTypesAsync())
        {
            if (!typeof(INotifyPropertyChanged).IsAssignableFrom(vmType)) continue;

            // Construction is on the deadline too. It is shared with
            // Every_command_property_is_non_null_after_construction, which was
            // once read as proof that constructing is safe — but nothing in the
            // aborted run shows that test ever ran, so it proves nothing. See
            // backlog N.7.
            var built = await WithDeadline(() =>
            {
                var ok = TryConstruct(vmType, out var instance, out _);
                return (Ok: ok, Instance: instance);
            });

            if (!built.Completed)
            {
                failures.Add($"{vmType.Name} — the CONSTRUCTOR did not return within " +
                             $"{PropertyDeadline.TotalSeconds:0}s (backlog N.7)");
                continue;
            }

            if (!built.Value.Ok) continue;

            var vm = built.Value.Instance;
            var raised = new List<string>();
            var gate = new object();

            // The handler can be raised from the worker threads below, so the
            // list needs a lock. A plain List under a monitor rather than a
            // concurrent collection: every read is a membership test on a
            // handful of names.
            void OnChanged(object? _, PropertyChangedEventArgs e)
            {
                lock (gate) raised.Add(e.PropertyName ?? "");
            }

            var notifier = (INotifyPropertyChanged)vm!;
            notifier.PropertyChanged += OnChanged;

            try
            {
                foreach (var p in vmType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.CanRead && p.CanWrite && p.SetMethod!.IsPublic))
                {
                    var read = await WithDeadline(() =>
                    {
                        var usable = TryMakeDistinctValue(p, vm, out var value);
                        return (Usable: usable, Candidate: value);
                    });

                    if (!read.Completed)
                    {
                        failures.Add($"{vmType.Name}.{p.Name} — the GETTER did not return within " +
                                     $"{PropertyDeadline.TotalSeconds:0}s. A blocking accessor freezes the " +
                                     "UI thread, and an unbounded one hangs CI (backlog N.7)");
                        break;   // this instance is unusable; anything further is noise
                    }

                    if (!read.Value.Usable) continue;

                    lock (gate) raised.Clear();

                    var written = await WithDeadline(() =>
                    {
                        try { p.SetValue(vm, read.Value.Candidate); return (Exception?)null; }
                        catch (TargetInvocationException ex) { return ex; }   // guard clauses are legitimate
                    });

                    if (!written.Completed)
                    {
                        failures.Add($"{vmType.Name}.{p.Name} — the SETTER did not return within " +
                                     $"{PropertyDeadline.TotalSeconds:0}s. A blocking accessor freezes the " +
                                     "UI thread, and an unbounded one hangs CI (backlog N.7)");
                        break;   // the instance is mid-mutation; stop trusting it
                    }

                    if (written.Value is not null) continue;   // the setter threw a guard clause

                    exercised++;

                    bool notified;
                    lock (gate) notified = raised.Contains(p.Name);

                    if (!notified)
                        failures.Add($"{vmType.Name}.{p.Name} changed without raising PropertyChanged — " +
                                     $"the UI keeps showing the old value");
                }
            }
            finally
            {
                notifier.PropertyChanged -= OnChanged;
            }
        }

        // Failures first, deliberately. When a constructor or an accessor blows
        // its deadline nothing gets exercised, and asserting the count first
        // would replace the message naming the culprit with "proved nothing".
        failures.Should().BeEmpty(
            "a bound property must notify and must return — assign through SetField, and do no " +
            $"blocking work in an accessor or a constructor. {string.Join("\n  ", failures)}");

        exercised.Should().BeGreaterThan(0,
            "no settable property was exercised, so this test proved nothing");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IEnumerable<Type> ViewModelTypes() =>
        UiAssembly.GetTypes()
                   .Concat(PresentationAssembly.GetTypes())
                   .Where(t => t is { IsAbstract: false, IsPublic: true } && t.Name.EndsWith("ViewModel", StringComparison.Ordinal))
                   .Distinct()
                   .OrderBy(t => t.Name, StringComparer.Ordinal);

    private static Type? ResolveViewModel(string viewName)
    {
        if (ViewToViewModelOverrides.TryGetValue(viewName, out var mapped))
            return mapped is null ? null : UiAssembly.GetTypes().FirstOrDefault(t => t.Name == mapped);

        return UiAssembly.GetTypes().FirstOrDefault(t => t.Name == viewName + "Model");
    }

    private static HashSet<string> InstanceMemberNames(Type t) =>
        t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
         .Select(m => m.Name).ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> StaticMemberNames(Type t) =>
        t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy)
         .Select(m => m.Name).ToHashSet(StringComparer.Ordinal);

    private static IEnumerable<(int Line, string Name)> BoundNames(string xamlPath)
    {
        var text = File.ReadAllText(xamlPath);
        foreach (Match m in Binding.Matches(text))
        {
            // TemplateBinding isn't matched at all, and a binding that redirects
            // its source resolves against the templated control rather than the
            // DataContext — Padding and BorderBrush there are real framework
            // properties, not ViewModel ones.
            if (Redirected.IsMatch(m.Groups["rest"].Value)) continue;

            var head = m.Groups["path"].Value.Split('.')[0].Split('[')[0];
            yield return (text[..m.Index].Count(c => c == '\n') + 1, head);
        }
    }

    private static IEnumerable<(string ViewName, string XamlPath)> EnumerateViews()
    {
        var viewsDir = Path.Combine(FindRepositoryRoot(), "ui", "TableTop.WinUI", "Views");
        foreach (var f in Directory.GetFiles(viewsDir, "*.xaml").OrderBy(f => f, StringComparer.Ordinal))
            yield return (Path.GetFileNameWithoutExtension(f), f);
    }

    /// <summary>
    /// Builds a ViewModel, supplying auto-implemented stubs for interface
    /// dependencies via <see cref="DispatchProxy"/> — BCL only, no mocking
    /// package needed. Returns false when a dependency can't be satisfied;
    /// the callers assert that coverage never falls to zero.
    /// </summary>
    private static bool TryConstruct(Type vmType, out object? instance, out string? reason)
    {
        instance = null;
        reason = null;

        foreach (var ctor in vmType.GetConstructors().OrderBy(c => c.GetParameters().Length))
        {
            try
            {
                var args = ctor.GetParameters().Select(p => DefaultFor(p.ParameterType)).ToArray();
                instance = ctor.Invoke(args);
                return true;
            }
            catch (Exception ex)
            {
                reason = ex.InnerException?.Message ?? ex.Message;
            }
        }

        return false;
    }

    private static object? DefaultFor(Type t)
    {
        if (t == typeof(string)) return string.Empty;
        if (t.IsInterface) return StubProxy.For(t);
        if (t.IsValueType) return Activator.CreateInstance(t);
        // Archetype has no parameterless constructor, so a null default is
        // the fallback below — and a null Archetype isn't a benign "can't
        // build this" skip the way it is for most types: ArchetypePickerViewModel,
        // SubArchetypePickerViewModel and GameSelectionViewModel all
        // dereference their Archetype parameter in the constructor body
        // (parent.SubArchetypes, node.Modes), so a null default threw a
        // NullReferenceException that TryConstruct then swallowed as "this
        // constructor doesn't work" — silently dropping three of the five
        // WinUI ViewModels that exist from both tests' coverage, including
        // the only ones with real settable properties to exercise.
        if (t == typeof(TableTop.Hosting.Archetype)) return EmptyArchetype;
        if (t.GetConstructor(Type.EmptyTypes) is { } ctor) return ctor.Invoke(null);
        return null;
    }

    private static readonly TableTop.Hosting.Archetype EmptyArchetype =
        new(id: "stub", name: "Stub", description: "", emoji: "❓", modes: []);

    private static bool TryMakeDistinctValue(PropertyInfo p, object? vm, out object? value)
    {
        value = null;
        object? current;
        try { current = p.GetValue(vm); } catch { return false; }

        var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

        if (t == typeof(string)) value = current as string == "x" ? "y" : "x";
        else if (t == typeof(bool)) value = !(current is bool b && b);
        else if (t == typeof(int)) value = (current is int i ? i : 0) + 1;
        else if (t == typeof(double)) value = (current is double d ? d : 0) + 1;
        else if (t.IsEnum)
        {
            var values = Enum.GetValues(t).Cast<object>().ToList();
            value = values.FirstOrDefault(v => !Equals(v, current));
            if (value is null) return false;
        }
        else if (!t.IsValueType && current is not null) value = null;   // reference -> null is a real change
        else return false;

        return !Equals(value, current);
    }

    /// <summary>
    /// Runs <paramref name="work"/> on a worker and gives it
    /// <see cref="PropertyDeadline"/> to finish.
    ///
    /// <para>
    /// <b>Awaited, never waited on.</b> The first attempt used
    /// <c>Task.Wait(timeout)</c> and was wrong twice over: xUnit's own analyser
    /// rejected it — xUnit1031, <i>"test methods should not use blocking task
    /// operations, as they can cause deadlocks"</i> — and it did not work, the
    /// run hanging for the full five minutes exactly as before. Blocking inside
    /// a test the framework is already scheduling is precisely what that rule
    /// exists to stop.
    /// </para>
    /// </summary>
    /// <returns>
    /// <c>Completed</c> false when the deadline expired. The worker is not
    /// cancelled — it cannot be, since reflection into arbitrary user code is
    /// not interruptible — but it is a thread-pool thread, so it does not keep
    /// the host alive and the run still terminates.
    /// </returns>
    /// <summary>
    /// The single place either test reaches the assemblies under scan, and it is
    /// on the deadline. See <see cref="ScannedAssemblies"/> for why the sweep —
    /// not just the accessors — is what actually had to be guarded, and why the
    /// guard could not work until that type was split out.
    /// </summary>
    private static async Task<IReadOnlyList<Type>> DiscoverViewModelTypesAsync()
    {
        var discovered = await WithDeadline(() => ViewModelTypes().ToList());

        discovered.Completed.Should().BeTrue(
            $"reflecting over the ViewModel types must return within {PropertyDeadline.TotalSeconds:0}s — " +
            "this is where CI hung: loading the WinUI app assembly does not return on a runner with no " +
            "interactive desktop session, and the hang dump puts the stuck frame in this sweep's type " +
            "initializer (backlog N.7)");

        return discovered.Value;
    }

    private static async Task<(bool Completed, T Value)> WithDeadline<T>(Func<T> work)
    {
        var task = Task.Run(work);
        var finished = await Task.WhenAny(task, Task.Delay(PropertyDeadline)).ConfigureAwait(false);

        return finished == task
            ? (true, await task.ConfigureAwait(false))
            : (false, default!);
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "ui", "TableTop.WinUI")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root from '{AppContext.BaseDirectory}'.");
    }
}

/// <summary>
/// Auto-implements any interface with do-nothing members, so ViewModels with
/// interface dependencies can be constructed without a mocking package.
/// Collections come back empty rather than null, since ViewModels commonly
/// enumerate a repository in their constructor.
/// </summary>
public class StubProxy : DispatchProxy
{
    /// <summary>
    /// Was a reflection lookup — <c>typeof(DispatchProxy).GetMethod(nameof(Create), …)</c>
    /// — that made <c>MakeGenericMethod</c> callable on a type unknown at
    /// compile time. It threw <c>AmbiguousMatchException</c> the moment this
    /// ran on a .NET whose BCL added the non-generic
    /// <c>DispatchProxy.Create(Type, Type)</c> overload alongside the
    /// original <c>Create&lt;T, TProxy&gt;()</c> — exactly the overload this
    /// needs, since it hits the same "type only known at runtime" problem
    /// the reflection dance existed to work around. <see cref="TryConstruct"/>
    /// swallowed the exception as "this dependency can't be satisfied", so
    /// every ViewModel with an interface-typed constructor parameter
    /// silently dropped out of both tests' coverage.
    /// </summary>
    public static object? For(Type interfaceType) =>
        DispatchProxy.Create(interfaceType, typeof(StubProxy));

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var t = targetMethod?.ReturnType;
        if (t is null || t == typeof(void)) return null;
        if (t == typeof(string)) return string.Empty;
        if (t == typeof(Task)) return Task.CompletedTask;
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = t.GetGenericArguments()[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                               .MakeGenericMethod(inner)
                               .Invoke(null, [EmptyValue(inner)]);
        }
        return EmptyValue(t);
    }

    private static object? EmptyValue(Type t)
    {
        if (t == typeof(string)) return string.Empty;
        if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string))
        {
            if (t.IsArray) return Array.CreateInstance(t.GetElementType()!, 0);
            if (t.IsGenericType)
            {
                var listType = typeof(List<>).MakeGenericType(t.GetGenericArguments()[0]);
                if (t.IsAssignableFrom(listType)) return Activator.CreateInstance(listType);
            }
        }
        return t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}
