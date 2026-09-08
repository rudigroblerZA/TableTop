using TableTop.Core.Abstractions.Cards;
using TableTop.DeckDesigner.Core;
using TableTop.Presentation.Infrastructure;

namespace TableTop.DeckDesigner.ViewModels;

/// <summary>
/// Bindable wrapper around one card being authored. Kept separate from
/// <see cref="CardDraft"/> — this is the WinUI-bindable, string-oriented
/// editing surface (e.g. tags as one comma-separated <see cref="string"/>,
/// difficulty as a combo-box index); <see cref="ToDraft"/> is where it turns
/// back into the plain data <see cref="DeckCompiler"/> and
/// <see cref="CardBankCodeGenerator"/> actually consume.
/// </summary>
public sealed class CardRowViewModel : ViewModelBase
{
    /// <summary>Difficulty labels for the editor's combo box, index-matched to <see cref="Difficulty"/> (Easy=0 .. Extreme=3).</summary>
    public static IReadOnlyList<string> DifficultyOptions { get; } = ["Easy", "Medium", "Hard", "Extreme"];

    private string _category = "";
    private string _title = "";
    private string _description = "";
    private int _difficultyIndex = 1; // Medium
    private string _tags = "";
    private bool _isThisOrThat;
    private string _optionALabel = "";
    private string _optionADetail = "";
    private string _optionBLabel = "";
    private string _optionBDetail = "";

    /// <summary>Category the card belongs to.</summary>
    public string Category
    {
        get => _category;
        set { if (SetField(ref _category, value)) OnPropertyChanged(nameof(Summary)); }
    }

    /// <summary>Card title.</summary>
    public string Title
    {
        get => _title;
        set { if (SetField(ref _title, value)) OnPropertyChanged(nameof(Summary)); }
    }

    /// <summary>Card body — the question, for a this-or-that card.</summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>Index into <see cref="DifficultyOptions"/>.</summary>
    public int DifficultyIndex
    {
        get => _difficultyIndex;
        set => SetField(ref _difficultyIndex, value);
    }

    /// <summary>Raw comma-separated tags as typed. Parsed on <see cref="ToDraft"/>.</summary>
    public string Tags
    {
        get => _tags;
        set => SetField(ref _tags, value);
    }

    /// <summary>True to author this as a this-or-that card instead of a standard one.</summary>
    public bool IsThisOrThat
    {
        get => _isThisOrThat;
        set => SetField(ref _isThisOrThat, value);
    }

    /// <summary>First option's label.</summary>
    public string OptionALabel { get => _optionALabel; set => SetField(ref _optionALabel, value); }

    /// <summary>First option's revealed detail. Optional.</summary>
    public string OptionADetail { get => _optionADetail; set => SetField(ref _optionADetail, value); }

    /// <summary>Second option's label.</summary>
    public string OptionBLabel { get => _optionBLabel; set => SetField(ref _optionBLabel, value); }

    /// <summary>Second option's revealed detail. Optional.</summary>
    public string OptionBDetail { get => _optionBDetail; set => SetField(ref _optionBDetail, value); }

    /// <summary>One-line summary shown in the card list.</summary>
    public string Summary =>
        string.IsNullOrWhiteSpace(Title) ? "(untitled card)" : $"[{Category}] {Title}";

    private Difficulty Difficulty => (Difficulty)(DifficultyIndex + 1);

    /// <summary>Converts this row into the plain <see cref="CardDraft"/> the compiler and code generator consume.</summary>
    public CardDraft ToDraft() => new()
    {
        Category = Category.Trim(),
        Title = Title.Trim(),
        Description = Description.Trim(),
        Difficulty = Difficulty,
        Tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        IsThisOrThat = IsThisOrThat,
        OptionALabel = OptionALabel.Trim(),
        OptionADetail = string.IsNullOrWhiteSpace(OptionADetail) ? null : OptionADetail.Trim(),
        OptionBLabel = OptionBLabel.Trim(),
        OptionBDetail = string.IsNullOrWhiteSpace(OptionBDetail) ? null : OptionBDetail.Trim(),
    };
}
