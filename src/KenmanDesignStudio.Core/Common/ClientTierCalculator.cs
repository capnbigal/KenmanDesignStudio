using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Common;

/// <summary>
/// Derives a <see cref="ClientTier"/> from a client's booked lifetime value.
/// Thresholds are intentionally generous so the prestige roster spreads across all three tiers.
/// </summary>
public static class ClientTierCalculator
{
    public const decimal SignatureThreshold = 12_000_000m;
    public const decimal PremierThreshold = 4_000_000m;

    public static ClientTier FromLifetimeValue(decimal lifetimeValue) => lifetimeValue switch
    {
        >= SignatureThreshold => ClientTier.Signature,
        >= PremierThreshold => ClientTier.Premier,
        _ => ClientTier.Standard
    };
}
