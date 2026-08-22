using System.Linq;
using System.Net.NetworkInformation;

namespace Milese.Aspire.AppHost.Instances;

public static class PortAvailability
{
    public static bool IsSlotFree(int slotIndex)
    {
        var portOffset = InstancePorts.OffsetFor(slotIndex);

        return IsFree(InstancePorts.ApiHttp(portOffset));
    }

    private static bool IsFree(int port) =>
        !IPGlobalProperties
            .GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .Any(listener => listener.Port == port);
}
