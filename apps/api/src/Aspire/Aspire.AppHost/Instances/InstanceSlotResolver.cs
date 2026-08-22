using System;
using System.Linq;
using System.Text;

namespace Milese.Aspire.AppHost.Instances;

public static class InstanceSlotResolver
{
    public const string MainDatabaseName = "milesedb";
    public const int MaxSlotIndex = 9;

    private const string WorktreeDatabasePrefix = "milesedb_";

    public static InstanceSlot Resolve(CheckoutIdentity checkout, Func<int, bool> isSlotFree)
    {
        var index = FirstFreeIndexFrom(PreferredIndex(checkout), isSlotFree);

        return new InstanceSlot
        {
            Index = index,
            PortOffset = InstancePorts.OffsetFor(index),
            DatabaseName = DatabaseNameFor(checkout),
        };
    }

    private static int PreferredIndex(CheckoutIdentity checkout)
    {
        if (checkout.IsMainCheckout)
            return 0;

        var ordered = checkout.WorktreeNames.OrderBy(name => name, StringComparer.Ordinal).ToList();
        var position = ordered.IndexOf(checkout.Name);

        return position < 0 ? 1 : position + 1;
    }

    private static int FirstFreeIndexFrom(int preferred, Func<int, bool> isSlotFree)
    {
        for (var index = preferred; index <= MaxSlotIndex; index++)
        {
            if (isSlotFree(index))
                return index;
        }

        return preferred;
    }

    private static string DatabaseNameFor(CheckoutIdentity checkout) =>
        checkout.IsMainCheckout ? MainDatabaseName : WorktreeDatabasePrefix + Sanitize(checkout.Name);

    private static string Sanitize(string name)
    {
        var builder = new StringBuilder(name.Length);

        foreach (var character in name)
            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '_');

        return builder.ToString();
    }
}
