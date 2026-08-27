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
    private static readonly Assembly UiAssembly = typeof(TableTop.WinUI.Infrastructure.Navigator).Assembly;

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
    private static readonly Assembly PresentationAssembly =
        typeof(TableTop.Presentation.ViewModels.SettingsViewModel).Assembly;

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

    private static readonly Regex Binding = new(
        @"\{(?:x:Bind|Binding)\s+(?:Path\s*=\s*)?(?<path>[A-Za-z_][\w\.]*)(?<rest>[^}]*)",
        RegexOptions.Compiled);

    private static readonly Regex Redirected = new(
        @"\b(?:RelativeSource|ElementName|Source)\s*=", RegexOptions.Compiled);

    // ── Commands ──────────────────────────────────────────────────────────────

    [Fact]
    public void Every_command_property_is_non_null_after_construction()
    {
        var failures = new List<string>();
        var constructed = 0;

        foreach (var vmType in ViewModelTypes())
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

    [Fact]
    public void Every_settable_property_raises_PropertyChanged()
    {
        var failures = new List<string>();
        var exercised = 0;

        foreach (var vmType in ViewModelTypes())
        {
            if (!typeof(INotifyPropertyChanged).IsAssignableFrom(vmType)) continue;
            if (!TryConstruct(vmType, out var vm, out _)) continue;

            var raised = new List<string>();
            ((INotifyPropertyChanged)vm!).PropertyChanged += (_, e) => raised.Add(e.PropertyName ?? "");

            foreach (var p in vmType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.CanRead && p.CanWrite && p.SetMethod!.IsPublic))
            {
                if (!TryMakeDistinctValue(p, vm, out var candidate)) continue;

                raised.Clear();
                try { p.SetValue(vm, candidate); }
                catch (TargetInvocationException) { continue; }   // guard clauses are legitimate

                exercised++;
                if (!raised.Contains(p.Name))
                    failures.Add($"{vmType.Name}.{p.Name} changed without raising PropertyChanged — " +
                                 $"the UI keeps showing the old value");
            }
        }

        exercised.Should().BeGreaterThan(0,
            "no settable property was exercised, so this test proved nothing");

        failures.Should().BeEmpty(
            "a bound property that doesn't notify leaves the UI stale — assign through SetField. " +
            $"{string.Join("\n  ", failures)}");
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
