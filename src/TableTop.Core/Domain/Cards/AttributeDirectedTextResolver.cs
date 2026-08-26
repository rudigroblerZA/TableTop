using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// Resolves card text by matching any player attribute (not just gender).
/// Use this for age-group variants, role-based variants, locale, etc.
/// </summary>
public sealed class AttributeDirectedTextResolver : ICardTextResolver
{
    private readonly string _attributeKey;
    private readonly string _defaultText;
    private readonly Dictionary<string, string> _variants;

    /// <summary>Initialises a new <see cref="AttributeDirectedTextResolver"/> instance.</summary>
    public AttributeDirectedTextResolver(
        string attributeKey,
        string defaultText,
        IDictionary<string, string> variants)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultText);
        ArgumentNullException.ThrowIfNull(variants);

        _attributeKey = attributeKey;
        _defaultText = defaultText;
        _variants = new Dictionary<string, string>(variants, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public string Resolve(IPlayer player)
    {
        if (player.Attributes.TryGetValue(_attributeKey, out var value) &&
            _variants.TryGetValue(value, out var text))
        {
            return text;
        }

        return _defaultText;
    }
}