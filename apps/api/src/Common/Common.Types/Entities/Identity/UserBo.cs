using System;
using Milese.Common.Types.ValueTypes.Identity;

namespace Milese.Common.Types.Entities.Identity;

public sealed class UserBo
{
    public required UserId Id { get; init; }

    public required Guid EntraObjectId { get; init; }

    public required Email Email { get; init; }

    public required DisplayName DisplayName { get; init; }
}
