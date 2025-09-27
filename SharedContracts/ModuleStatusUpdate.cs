namespace SharedContracts;
public record ModuleStatusUpdate
{
    public List<ModuleInfo> Modules { get; init; } = new();
}
