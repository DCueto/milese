using System.Collections.Generic;

namespace Milese.Aspire.AppHost.Instances;

public sealed class CheckoutIdentity
{
    public required string Name { get; init; }

    public required bool IsMainCheckout { get; init; }

    public required string RootPath { get; init; }

    public required IReadOnlyCollection<string> WorktreeNames { get; init; }
}
