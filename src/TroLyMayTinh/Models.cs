using System.Windows.Input;

namespace TroLyMayTinh;

public sealed class SoftwareItem : Observable
{
    public required string Name { get; init; }
    public required string Id { get; init; }
    public required string Publisher { get; init; }
    public required string DescriptionVi { get; init; }
    public required string DescriptionEn { get; init; }
    public required string[] DetectionNames { get; init; }
    public string Category { get; init; } = "utilities";
    public string CategoryText { get; private set; } = "";
    public string Description { get; set; } = "";
    private string state = "";
    public string State { get => state; set => Set(ref state, value); }
    private bool selected;
    public bool Selected { get => selected; set => Set(ref selected, value); }
    public void Translate(bool english) { Description = english ? DescriptionEn : DescriptionVi; CategoryText = SoftwareCatalog.Categories(english).FirstOrDefault(x => x.Key == Category)?.Label ?? Category; Changed(nameof(Description)); Changed(nameof(CategoryText)); }
}

public sealed class ToolItem(string name, string description, string icon, ICommand openCommand, string id, string group) : Observable
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string Icon { get; } = icon;
    public string Id { get; } = id;
    public string Group { get; } = group;
    public ICommand OpenCommand { get; } = openCommand;
    public ICommand FavoriteCommand { get; set; } = new ActionCommand(() => { });
    private bool favorite;
    public bool Favorite { get => favorite; set { Set(ref favorite, value); Changed(nameof(Star)); } }
    public string Star => Favorite ? "★" : "☆";
}

public sealed record CategoryOption(string Key, string Label);
public sealed record HardwareField(string Label, string Value);
public sealed record HardwareGroup(string Title, IReadOnlyList<HardwareField> Fields);

public sealed class CleanupItem(TempEntry entry) : Observable
{
    public TempEntry Entry { get; } = entry;
    public string Name => Path.GetFileName(Entry.Path);
    public string PathText => Entry.Path;
    public string Size => MainViewModel.FormatBytes(Entry.Length);
    public string Date => Entry.WrittenUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    private bool selected;
    public bool Selected { get => selected; set => Set(ref selected, value); }
}
