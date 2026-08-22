using Milese.Aspire.AppHost.Instances;

namespace Milese.Aspire.AppHost.Tests.Instances;

public class InstancePortsTests
{
    [Test]
    public void OffsetFor_Multiplies_Slot_By_The_Stride()
    {
        InstancePorts.OffsetFor(0).ShouldBe(0);
        InstancePorts.OffsetFor(1).ShouldBe(100);
        InstancePorts.OffsetFor(3).ShouldBe(300);
    }

    [Test]
    public void ApiHttp_Adds_The_Offset_To_The_Base_Port()
    {
        InstancePorts.ApiHttp(0).ShouldBe(InstancePorts.ApiHttpBase);
        InstancePorts.ApiHttp(100).ShouldBe(InstancePorts.ApiHttpBase + 100);
    }
}
