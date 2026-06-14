using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Infrastructure.Seeding;

/// <summary>Curated, prestige-appropriate name and place pools used by <see cref="DataSeeder"/>.</summary>
internal static class SeedPools
{
    internal sealed record ClientSeed(string Name, ClientType Type, int Scale);
    // Scale 1-5 drives how many / how large the client's projects are (→ tier spread).

    internal static readonly ClientSeed[] Clients =
    {
        // Developers & REITs (large)
        new("Marquis Development Group", ClientType.Developer, 5),
        new("Sterling Harbor Partners", ClientType.Developer, 5),
        new("Ardent Capital Realty", ClientType.REIT, 4),
        new("Northcrest Properties Trust", ClientType.REIT, 4),
        new("Vanguard Urban Developments", ClientType.Developer, 5),
        new("Halcyon Real Estate Trust", ClientType.REIT, 4),
        new("Meridian Pacific Holdings", ClientType.Developer, 5),
        new("Castellan Property Group", ClientType.Developer, 3),
        new("Brightwater REIT", ClientType.REIT, 4),
        new("Camden & Howe Developments", ClientType.Developer, 3),
        new("Sovereign Land Partners", ClientType.Developer, 4),
        new("Pinnacle Equities Trust", ClientType.REIT, 5),
        new("Westbrook Estates Group", ClientType.Developer, 3),
        new("Ironwood Capital Partners", ClientType.Developer, 4),

        // Airport & transit authorities (very large)
        new("Coastal Aviation Authority", ClientType.AirportAuthority, 5),
        new("Metropolitan Airports Commission", ClientType.AirportAuthority, 5),
        new("Harbor Gateway Transit Authority", ClientType.AirportAuthority, 4),
        new("Skylink International Airports", ClientType.AirportAuthority, 5),
        new("Continental Aviation Authority", ClientType.AirportAuthority, 4),

        // Resort & hospitality groups
        new("Aurelia Resorts & Spas", ClientType.ResortGroup, 5),
        new("Solène Hospitality Collection", ClientType.ResortGroup, 4),
        new("Azure Coast Hotels", ClientType.ResortGroup, 4),
        new("Maison Lumière Resorts", ClientType.ResortGroup, 5),
        new("Calista Resort Collection", ClientType.ResortGroup, 3),
        new("Mandara Bay Resorts", ClientType.ResortGroup, 4),
        new("Évora Hospitality Group", ClientType.ResortGroup, 3),

        // Country clubs & golf
        new("Pinehurst National Club", ClientType.CountryClub, 4),
        new("Cypress Ridge Country Club", ClientType.CountryClub, 3),
        new("Eagle's Crest Golf Club", ClientType.CountryClub, 3),
        new("Wentworth Hills Club", ClientType.CountryClub, 4),
        new("Laurel Glen Country Club", ClientType.CountryClub, 2),
        new("Saratoga Polo & Country Club", ClientType.CountryClub, 3),

        // Corporate
        new("Helios Technologies", ClientType.Corporate, 5),
        new("Atlas Financial Group", ClientType.Corporate, 4),
        new("Northwind Energy Corp.", ClientType.Corporate, 4),
        new("Vireo Biosciences", ClientType.Corporate, 3),
        new("Quantum Dynamics Corp.", ClientType.Corporate, 4),
        new("Ironclad Holdings", ClientType.Corporate, 3),
        new("Lumen Capital Group", ClientType.Corporate, 3),

        // Private individuals / estates (smaller, but some very large)
        new("The Ashford Estate", ClientType.PrivateIndividual, 2),
        new("Pinehurst Family Estate", ClientType.PrivateIndividual, 2),
        new("Belmont Family Estate", ClientType.PrivateIndividual, 3),
        new("The Carrington Residence", ClientType.PrivateIndividual, 1),
        new("Rosencliff Estate", ClientType.PrivateIndividual, 2),
        new("The Whitmore Residence", ClientType.PrivateIndividual, 1),
        new("Highgrove Private Estate", ClientType.PrivateIndividual, 3),
        new("The Devereux Estate", ClientType.PrivateIndividual, 2),
        new("Ravenswood Estate", ClientType.PrivateIndividual, 2),
        new("The Sinclair Residence", ClientType.PrivateIndividual, 1),
        new("Thornbury Hall Estate", ClientType.PrivateIndividual, 2),
        new("The Vanderlyn Estate", ClientType.PrivateIndividual, 3),
        new("Kestrel Point Residence", ClientType.PrivateIndividual, 1),
        new("Montclair Family Estate", ClientType.PrivateIndividual, 2),
    };

    internal sealed record Place(string City, string Region, string Country);

    internal static readonly Place[] Places =
    {
        new("Manhattan, NY", "Northeast", "USA"),
        new("Greenwich, CT", "Northeast", "USA"),
        new("The Hamptons, NY", "Northeast", "USA"),
        new("Boston, MA", "Northeast", "USA"),
        new("Beverly Hills, CA", "West Coast", "USA"),
        new("San Francisco, CA", "West Coast", "USA"),
        new("Napa Valley, CA", "West Coast", "USA"),
        new("Malibu, CA", "West Coast", "USA"),
        new("Seattle, WA", "West Coast", "USA"),
        new("Palm Beach, FL", "Southeast", "USA"),
        new("Miami, FL", "Southeast", "USA"),
        new("Charleston, SC", "Southeast", "USA"),
        new("Naples, FL", "Southeast", "USA"),
        new("Atlanta, GA", "Southeast", "USA"),
        new("Aspen, CO", "Mountain West", "USA"),
        new("Jackson Hole, WY", "Mountain West", "USA"),
        new("Scottsdale, AZ", "Mountain West", "USA"),
        new("Park City, UT", "Mountain West", "USA"),
        new("Dubai", "International", "UAE"),
        new("Singapore", "International", "Singapore"),
        new("Monaco", "International", "Monaco"),
        new("Lake Como", "International", "Italy"),
        new("London", "International", "UK"),
    };

    // Descriptive project nouns per category.
    internal static readonly Dictionary<ProjectCategory, string[]> Descriptors = new()
    {
        [ProjectCategory.RooftopSkyGarden] = new[]
            { "Sky Terrace", "Sky Garden", "Rooftop Commons", "Cloud Terrace", "Skyline Gardens", "Aerial Garden", "Crown Terrace", "Summit Garden" },
        [ProjectCategory.AirportTransit] = new[]
            { "Arrivals Garden", "Concourse Gardens", "Gateway Landscape", "Terminal Forecourt", "Transit Plaza", "Skyport Gardens", "Departures Court" },
        [ProjectCategory.LuxuryResort] = new[]
            { "Resort Gardens", "Bayfront Gardens", "Spa Gardens", "Pool Terraces", "Garden Sanctuary", "Lagoon Gardens", "Oceanfront Grounds" },
        [ProjectCategory.PrivateEstate] = new[]
            { "Estate Gardens", "Manor Grounds", "Garden Court", "Parterre Gardens", "Estate Grounds", "Private Garden", "Walled Garden" },
        [ProjectCategory.GolfCountryClub] = new[]
            { "Championship Course", "Course Restoration", "Clubhouse Grounds", "Links Restoration", "National Course", "Signature Course" },
        [ProjectCategory.CorporateCampus] = new[]
            { "Campus Commons", "Headquarters Plaza", "Corporate Gardens", "Innovation Park", "Wellness Courtyard", "Campus Green" },
    };

    // Coined marquee names that read as developments / addresses.
    internal static readonly string[] MarqueeNames =
    {
        "One Vesper", "The Indigo", "Aurelia", "Meridian", "Solstice", "The Halcyon",
        "Verdé", "The Onyx", "Camellia", "The Belvedere", "Marengo", "The Carlyle",
        "Cascada", "The Athenaeum", "Lumière", "The Pinnacle", "Marisol", "The Coronet",
        "Highline", "The Sterling", "Azure", "The Conservatory", "Évora", "The Pavilion",
    };

    // Contract value bands (min, max) in USD by category.
    internal static readonly Dictionary<ProjectCategory, (decimal Min, decimal Max)> ValueBands = new()
    {
        [ProjectCategory.RooftopSkyGarden] = (1_200_000m, 9_000_000m),
        [ProjectCategory.AirportTransit] = (4_000_000m, 42_000_000m),
        [ProjectCategory.LuxuryResort] = (3_000_000m, 30_000_000m),
        [ProjectCategory.PrivateEstate] = (650_000m, 8_500_000m),
        [ProjectCategory.GolfCountryClub] = (2_000_000m, 18_000_000m),
        [ProjectCategory.CorporateCampus] = (2_500_000m, 35_000_000m),
    };

    // Architecture practices we "collaborate" with.
    internal static readonly string[] ArchitectPartners =
    {
        "Foster Lindqvist", "Aria & Stone Architects", "Holloway Práxis", "Nakamura Studio",
        "Vornado Architecture", "Bjarke & Reed", "Selva Atelier", "Corveau Architects",
        "Marrow Studio", "Heller-Vaughn", "Atelier Noor", "Praxis Group",
    };

    internal static readonly string[] FirstNames =
    {
        "Eleanor", "Julian", "Vivienne", "Marcus", "Genevieve", "Theodore", "Camille", "Sebastian",
        "Beatrice", "Nathaniel", "Isolde", "Lucian", "Arabella", "Maximilian", "Cordelia", "Adrian",
        "Rosalind", "Frederick", "Anastasia", "Dominic", "Margaux", "Laurent", "Priya", "Hiro",
        "Soraya", "Mateo", "Ingrid", "Rafael",
    };

    internal static readonly string[] LastNames =
    {
        "Ashford", "Beaumont", "Carrington", "Devereux", "Ellsworth", "Fairfax", "Greer", "Harrington",
        "Ingram", "Kingsley", "Lockwood", "Montague", "Northrop", "Pemberton", "Quint", "Radcliffe",
        "Sinclair", "Thornton", "Vance", "Whitmore", "Castellanos", "Nakamura", "Okonkwo", "Abadi",
    };

    internal static readonly string[] ContactTitles =
    {
        "Principal", "Managing Director", "VP, Development", "Director of Real Estate",
        "Chief Operating Officer", "Head of Capital Projects", "General Manager", "Estate Manager",
        "Director of Facilities", "Owner's Representative",
    };
}
