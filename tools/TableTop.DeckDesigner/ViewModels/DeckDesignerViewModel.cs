using System.Collections.ObjectModel;
using TableTop.DeckDesigner.Core;
using TableTop.Presentation.Infrastructure;
using Windows.ApplicationModel.DataTransfer;

namespace TableTop.DeckDesigner.ViewModels;

/// <summary>
/// The tool's only ViewModel — one screen, no navigation. Owns the deck being
/// authored, drives <see cref="DeckCompiler"/> for validation and
/// <see cref="CardBankCodeGenerator"/> for the C# output; never re-implements
/// either, so what this tool shows always matches what those two would do.
/// </summary>
public sealed class DeckDesignerViewModel : ViewModelBase
{
    private string _deckName = "";
    private string _namespaceName = "TableTop.Games.Custom";
    private string _className = "MyDeckCardBank";
    private CardRowViewModel? _selectedCard;
    private string _statusMessage = "Add a card, then Generate.";
    private bool _statusIsError;
    private string _generatedCode = "";

    /// <summary>Initialises commands and wires card-list changes to command re-evaluation.</summary>
    public DeckDesignerViewModel()
    {
        AddCardCommand = new RelayCommand(AddCard);
        AddThisOrThatCardCommand = new RelayCommand(AddThisOrThatCard);
        RemoveSelectedCommand = new RelayCommand(RemoveSelected, () => SelectedCard is not null);
        MoveUpCommand = new RelayCommand(() => Move(-1), () => CanMove(-1));
        MoveDownCommand = new RelayCommand(() => Move(1), () => CanMove(1));
        GenerateCommand = new RelayCommand(Generate, () => Cards.Count > 0);
        CopyToClipboardCommand = new RelayCommand(CopyToClipboard, () => HasGeneratedCode);

        Cards.CollectionChanged += (_, _) =>
        {
            GenerateCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
        };
    }

    /// <summary>Difficulty labels shared with every row's combo box.</summary>
    public IReadOnlyList<string> DifficultyOptions => CardRowViewModel.DifficultyOptions;

    /// <summary>Cards in authoring order — also the order they compile in.</summary>
    public ObservableCollection<CardRowViewModel> Cards { get; } = [];

    /// <summary>The deck's name. Seeds every card's stable id.</summary>
    public string DeckName { get => _deckName; set => SetField(ref _deckName, value); }

    /// <summary>Namespace the generated class is declared in.</summary>
    public string NamespaceName { get => _namespaceName; set => SetField(ref _namespaceName, value); }

    /// <summary>Name of the generated static card-bank class.</summary>
    public string ClassName { get => _className; set => SetField(ref _className, value); }

    /// <summary>The card currently shown in the editor panel.</summary>
    public CardRowViewModel? SelectedCard
    {
        get => _selectedCard;
        set
        {
            SetField(ref _selectedCard, value);
            RemoveSelectedCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>Result of the last <see cref="GenerateCommand"/> run.</summary>
    public string StatusMessage { get => _statusMessage; private set => SetField(ref _statusMessage, value); }

    /// <summary>True when <see cref="StatusMessage"/> describes a compile failure.</summary>
    public bool StatusIsError { get => _statusIsError; private set => SetField(ref _statusIsError, value); }

    /// <summary>The generated C# source, empty until a successful <see cref="GenerateCommand"/> run.</summary>
    public string GeneratedCode
    {
        get => _generatedCode;
        private set
        {
            if (SetField(ref _generatedCode, value))
            {
                OnPropertyChanged(nameof(HasGeneratedCode));
                CopyToClipboardCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>True once there is generated code to copy or save.</summary>
    public bool HasGeneratedCode => !string.IsNullOrEmpty(GeneratedCode);

    /// <summary>
    /// Base filename suggested to the save-file picker — no extension, since
    /// the picker appends one from its <c>FileTypeChoices</c>.
    /// </summary>
    public string SuggestedFileName =>
        string.IsNullOrWhiteSpace(ClassName) ? "GeneratedCardBank" : ClassName.Trim();

    /// <summary>Adds a standard card, defaulting to the last card's category.</summary>
    public RelayCommand AddCardCommand { get; }

    /// <summary>Adds a this-or-that card, defaulting to the last card's category.</summary>
    public RelayCommand AddThisOrThatCardCommand { get; }

    /// <summary>Removes <see cref="SelectedCard"/>.</summary>
    public RelayCommand RemoveSelectedCommand { get; }

    /// <summary>Moves <see cref="SelectedCard"/> one place earlier.</summary>
    public RelayCommand MoveUpCommand { get; }

    /// <summary>Moves <see cref="SelectedCard"/> one place later.</summary>
    public RelayCommand MoveDownCommand { get; }

    /// <summary>Compiles the deck via <see cref="DeckCompiler"/> and, on success, renders it via <see cref="CardBankCodeGenerator"/>.</summary>
    public RelayCommand GenerateCommand { get; }

    /// <summary>Copies <see cref="GeneratedCode"/> to the system clipboard.</summary>
    public RelayCommand CopyToClipboardCommand { get; }

    private void AddCard() => AddRow(isThisOrThat: false);

    private void AddThisOrThatCard() => AddRow(isThisOrThat: true);

    private void AddRow(bool isThisOrThat)
    {
        var row = new CardRowViewModel
        {
            Category = Cards.Count > 0 ? Cards[^1].Category : "",
            IsThisOrThat = isThisOrThat,
        };
        Cards.Add(row);
        SelectedCard = row;
    }

    private void RemoveSelected()
    {
        if (SelectedCard is null)
            return;

        var index = Cards.IndexOf(SelectedCard);
        Cards.Remove(SelectedCard);
        SelectedCard = Cards.Count == 0 ? null : Cards[Math.Min(index, Cards.Count - 1)];
    }

    private bool CanMove(int delta)
    {
        if (SelectedCard is null)
            return false;

        var target = Cards.IndexOf(SelectedCard) + delta;
        return target >= 0 && target < Cards.Count;
    }

    private void Move(int delta)
    {
        if (SelectedCard is null)
            return;

        var index = Cards.IndexOf(SelectedCard);
        Cards.Move(index, index + delta);
        MoveUpCommand.RaiseCanExecuteChanged();
        MoveDownCommand.RaiseCanExecuteChanged();
    }

    private void Generate()
    {
        var draft = new DeckDraft { Name = DeckName.Trim() };
        foreach (var card in Cards)
            draft.Cards.Add(card.ToDraft());

        var result = DeckCompiler.Compile(draft);
        if (!result.Succeeded)
        {
            StatusIsError = true;
            StatusMessage = string.Join(Environment.NewLine, result.Errors);
            GeneratedCode = "";
            return;
        }

        var className = string.IsNullOrWhiteSpace(ClassName) ? "GeneratedCardBank" : ClassName.Trim();
        var namespaceName = string.IsNullOrWhiteSpace(NamespaceName) ? "TableTop.Games.Custom" : NamespaceName.Trim();

        GeneratedCode = CardBankCodeGenerator.Generate(draft, namespaceName, className);
        StatusIsError = false;
        StatusMessage = $"{result.Cards!.Count} card(s) compiled successfully.";
    }

    private void CopyToClipboard()
    {
        if (!HasGeneratedCode)
            return;

        var package = new DataPackage();
        package.SetText(GeneratedCode);
        Clipboard.SetContent(package);
    }
}
