using TableTop.Core.Abstractions.Restrictions;
using TableTop.Core.Domain.Restrictions;

namespace TableTop.Core.Domain.Decks;

/// <summary>
/// Parses a restriction expression string into an <see cref="IRestriction"/> tree.
///
/// Grammar:
///   expr     = atom | combinator
///   atom     = keyword | parameterised
///   keyword  = "adult" | "male" | "female" | "couple" | "parent" | "married"
///   parameterised = "tag:" NAME
///                 | "attr:" KEY "=" VALUE
///                 | "age:" INTEGER
///   combinator = "not(" expr ")"
///              | "and(" expr "," expr ")"
///              | "or(" expr "," expr ")"
///
/// Examples:
///   "adult"
///   "male"
///   "and(adult,male)"
///   "or(parent,married)"
///   "not(adult)"
///   "and(adult,or(male,female))"
///   "tag:vip"
///   "attr:role=host"
///   "age:21"
/// </summary>
public static class RestrictionParser
{
    /// <summary>
    /// Parses <paramref name="expression"/> and returns the corresponding restriction.
    /// Returns null for null or whitespace input (no restriction).
    /// </summary>
    /// <exception cref="FormatException">
    /// Thrown when the expression is non-empty but syntactically invalid.
    /// </exception>
    public static IRestriction? Parse(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return null;

        var span = expression.Trim().AsSpan();
        var result = ParseExpr(ref span);

        if (!span.IsEmpty)
            throw new FormatException(
                $"Unexpected trailing text '{span.ToString()}' in restriction '{expression}'.");

        return result;
    }

    // ── Recursive descent ─────────────────────────────────────────────────────

    private static IRestriction ParseExpr(ref ReadOnlySpan<char> s)
    {
        // Combinators
        if (TryConsume(ref s, "not("))
        {
            var inner = ParseExpr(ref s);
            Expect(ref s, ')');
            return new NotRestriction(inner);
        }

        if (TryConsume(ref s, "and("))
        {
            var left = ParseExpr(ref s);
            Expect(ref s, ',');
            var right = ParseExpr(ref s);
            Expect(ref s, ')');
            return new AndRestriction(left, right);
        }

        if (TryConsume(ref s, "or("))
        {
            var left = ParseExpr(ref s);
            Expect(ref s, ',');
            var right = ParseExpr(ref s);
            Expect(ref s, ')');
            return new OrRestriction(left, right);
        }

        // Parameterised atoms
        if (TryConsume(ref s, "tag:"))
            return new TagRestriction(ReadToken(ref s));

        if (TryConsume(ref s, "age:"))
        {
            var raw = ReadToken(ref s);
            if (!int.TryParse(raw, out var age))
                throw new FormatException($"Expected integer after 'age:', got '{raw}'.");
            return new MinimumAgeRestriction(age);
        }

        if (TryConsume(ref s, "attr:"))
        {
            var kv = ReadToken(ref s);
            var eq = kv.IndexOf('=');
            if (eq <= 0)
                throw new FormatException($"Expected 'attr:key=value', got 'attr:{kv}'.");
            return new AttributeRestriction(kv[..eq], kv[(eq + 1)..]);
        }

        // Keywords
        return ReadKeyword(ref s);
    }

    private static IRestriction ReadKeyword(ref ReadOnlySpan<char> s)
    {
        var token = ReadToken(ref s).ToLowerInvariant();
        return token switch
        {
            "adult" => new AdultOnlyRestriction(),
            "male" => new MaleOnlyRestriction(),
            "female" => new FemaleOnlyRestriction(),
            "couple" => new CoupleOnlyRestriction(),
            "parent" => new ParentOnlyRestriction(),
            "married" => new MarriedOnlyRestriction(),
            _ => throw new FormatException($"Unknown restriction keyword '{token}'.")
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Reads contiguous non-punctuation characters.</summary>
    private static string ReadToken(ref ReadOnlySpan<char> s)
    {
        var i = 0;
        while (i < s.Length && s[i] is not (',' or '(' or ')'))
            i++;
        if (i == 0)
            throw new FormatException("Expected a token but found end of expression.");
        var token = s[..i].ToString();
        s = s[i..];
        return token;
    }

    private static bool TryConsume(ref ReadOnlySpan<char> s, string prefix)
    {
        if (s.StartsWith(prefix.AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            s = s[prefix.Length..];
            return true;
        }
        return false;
    }

    private static void Expect(ref ReadOnlySpan<char> s, char ch)
    {
        if (s.IsEmpty || s[0] != ch)
            throw new FormatException($"Expected '{ch}' but got '{(s.IsEmpty ? "EOF" : s[0])}'.");
        s = s[1..];
    }
}
