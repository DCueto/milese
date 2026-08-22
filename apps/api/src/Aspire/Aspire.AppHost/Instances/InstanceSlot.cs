namespace Milese.Aspire.AppHost.Instances;

public sealed class InstanceSlot
{
    public required int Index { get; init; }

    public required int PortOffset { get; init; }

    public required string DatabaseName { get; init; }
}
