using Milese.Aspire.AppHost.Instances;

namespace Milese.Aspire.AppHost.Tests.Instances;

public class InstanceSlotResolverTests
{
    [Test]
    public void Main_Checkout_Always_Resolves_To_Slot_Zero()
    {
        var checkout = new CheckoutIdentity
        {
            Name = CheckoutLocator.MainCheckoutName,
            IsMainCheckout = true,
            RootPath = "/repo",
            WorktreeNames = [],
        };

        var slot = InstanceSlotResolver.Resolve(checkout, _ => true);

        slot.Index.ShouldBe(0);
        slot.PortOffset.ShouldBe(0);
        slot.DatabaseName.ShouldBe(InstanceSlotResolver.MainDatabaseName);
    }

    [Test]
    public void Worktree_Slot_Follows_Its_Alphabetical_Position_Plus_One()
    {
        var checkout = new CheckoutIdentity
        {
            Name = "ui",
            IsMainCheckout = false,
            RootPath = "/repo-wt/ui",
            WorktreeNames = ["api", "ui", "zzz"],
        };

        var slot = InstanceSlotResolver.Resolve(checkout, _ => true);

        // "ui" is second alphabetically among ["api", "ui", "zzz"] (index 1) -> slot 2.
        slot.Index.ShouldBe(2);
        slot.PortOffset.ShouldBe(200);
    }

    [Test]
    public void Unknown_Worktree_Name_Falls_Back_To_Slot_One()
    {
        var checkout = new CheckoutIdentity
        {
            Name = "not-registered-yet",
            IsMainCheckout = false,
            RootPath = "/repo-wt/not-registered-yet",
            WorktreeNames = [],
        };

        var slot = InstanceSlotResolver.Resolve(checkout, _ => true);

        slot.Index.ShouldBe(1);
    }

    [Test]
    public void Takes_The_Next_Free_Slot_When_The_Preferred_One_Is_Held()
    {
        var checkout = new CheckoutIdentity
        {
            Name = "ui",
            IsMainCheckout = false,
            RootPath = "/repo-wt/ui",
            WorktreeNames = ["ui"],
        };

        var slot = InstanceSlotResolver.Resolve(checkout, index => index != 1);

        slot.Index.ShouldBe(2);
    }

    [Test]
    public void Falls_Back_To_The_Preferred_Slot_When_Every_Slot_Is_Held()
    {
        var checkout = new CheckoutIdentity
        {
            Name = "ui",
            IsMainCheckout = false,
            RootPath = "/repo-wt/ui",
            WorktreeNames = ["ui"],
        };

        var slot = InstanceSlotResolver.Resolve(checkout, _ => false);

        slot.Index.ShouldBe(1);
    }

    [Test]
    public void Worktree_Database_Name_Is_Prefixed_And_Sanitized()
    {
        var checkout = new CheckoutIdentity
        {
            Name = "case-id",
            IsMainCheckout = false,
            RootPath = "/repo-wt/case-id",
            WorktreeNames = ["case-id"],
        };

        var slot = InstanceSlotResolver.Resolve(checkout, _ => true);

        slot.DatabaseName.ShouldBe("milesedb_case_id");
    }
}
