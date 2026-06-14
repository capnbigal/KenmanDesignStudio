using KenmanDesignStudio.Core.Enums;

namespace KenmanDesignStudio.Core.Common;

/// <summary>Presentation metadata for a project category — display name, asset folder slug,
/// a marketing blurb, and the two-stop gradient used by placeholder art and gallery accents.</summary>
public sealed record CategoryMeta(
    ProjectCategory Category,
    string Name,
    string ShortName,
    string Slug,
    string Tagline,
    string Blurb,
    string GradientFrom,
    string GradientTo,
    string Accent,
    string Motif);

public static class CategoryCatalog
{
    public static readonly IReadOnlyList<CategoryMeta> All = new List<CategoryMeta>
    {
        new(ProjectCategory.RooftopSkyGarden,
            "Rooftop & Sky Gardens", "Sky Gardens", "rooftop-gardens",
            "Gardens in the clouds",
            "Elevated terraces, biodiverse sky gardens and amenity decks crowning the world's signature high-rises.",
            "#16302B", "#2F5D50", "#C8A95B", "skyline"),

        new(ProjectCategory.AirportTransit,
            "Airport & Transit Landscapes", "Airports", "airports",
            "First impressions at scale",
            "Arrival gardens, biophilic concourses and resilient transit-hub plantings experienced by millions.",
            "#1A2A33", "#33586B", "#C8A95B", "runway"),

        new(ProjectCategory.LuxuryResort,
            "Luxury Resorts & Hospitality", "Resorts", "resorts",
            "Landscapes of arrival & escape",
            "Five-star grounds, infinity pools and tropical garden journeys for the most discerning hospitality brands.",
            "#163A30", "#2E7D5B", "#D8B66A", "palms"),

        new(ProjectCategory.PrivateEstate,
            "Private Estates & Mansions", "Estates", "estates",
            "The art of the private realm",
            "Bespoke gardens, motor courts and parterres for landmark private residences and family compounds.",
            "#23301C", "#4A6B36", "#C8A95B", "parterre"),

        new(ProjectCategory.GolfCountryClub,
            "Golf Courses & Country Clubs", "Golf & Clubs", "golf-courses",
            "Sculpting the playing field",
            "Championship course landscapes, clubhouse grounds and naturalised water features for prestige clubs.",
            "#1E3A24", "#3E7A3E", "#C8A95B", "fairway"),

        new(ProjectCategory.CorporateCampus,
            "Corporate Campuses & Mixed-Use", "Campuses", "corporate",
            "Where business meets nature",
            "Headquarters plazas, mixed-use parks and wellness courtyards that define the modern corporate campus.",
            "#222A2E", "#3C5A63", "#C8A95B", "plaza"),
    };

    private static readonly Dictionary<ProjectCategory, CategoryMeta> Map =
        All.ToDictionary(c => c.Category);

    public static CategoryMeta Get(ProjectCategory category) => Map[category];

    public static string Slug(ProjectCategory category) => Map[category].Slug;
    public static string Name(ProjectCategory category) => Map[category].Name;
}
