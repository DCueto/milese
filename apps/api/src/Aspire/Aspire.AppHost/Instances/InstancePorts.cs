namespace Milese.Aspire.AppHost.Instances;

public static class InstancePorts
{
    public const int SlotStride = 100;

    public const int ApiHttpBase = 5080;

    public static int OffsetFor(int slotIndex) => slotIndex * SlotStride;

    public static int ApiHttp(int portOffset) => ApiHttpBase + portOffset;
}
