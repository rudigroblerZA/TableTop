using TableTop.Core.Abstractions.Cards;
using TableTop.Core.Abstractions.Players;

namespace TableTop.Core.Domain.Cards;

/// <summary>
/// Resolves card text by matching the player's "gender" attribute.
/// Falls back to a default text when no match is found so the card
/// is always playable regardless of gender (OCP — add new variants without modifying this class).
/// </summary>
public sealed class GenderDirectedTextResolver : ICardTextResolver
{
    private readonly string _defaultText;
    private readonly Dictionary<string, string> _variantsByGender;

    /// <param name="defaultText">Text shown when no gender variant matches.</param>
    /// <param name="variantsByGender">
    /// Map of gender value (case-insensitive) → prompt text.
    /// Keys should match values stored in the "gender" player attribute, e.g. "male", "female", "other".
    /// </param>
    public GenderDirectedTextResolver(
        string defaultText,
        IDictionary<string, string> variantsByGender)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultText);
        ArgumentNullException.ThrowIfNull(variantsByGender);

        _defaultText = defaultText;
        _variantsByGender = new Dictionary<string, string>(
            variantsByGender,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public string Resolve(IPlayer player)
    {
        if (player.Attributes.TryGetValue("gender", out var gender) &&
            _variantsByGender.TryGetValue(gender, out var variant))
        {
            return variant;
        }

        return _defaultText;
    }
}
