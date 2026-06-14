namespace KenmanDesignStudio.Core.Enums;

/// <summary>The six signature project disciplines the firm practices.</summary>
public enum ProjectCategory
{
    RooftopSkyGarden = 1,   // Commercial high-rise & rooftop sky gardens / terraces
    AirportTransit = 2,     // Airport & transit-hub landscapes
    LuxuryResort = 3,       // Luxury resorts & hospitality grounds
    PrivateEstate = 4,      // Private residential estates & mansions
    GolfCountryClub = 5,    // Prestigious golf courses & country clubs
    CorporateCampus = 6     // Corporate campuses & mixed-use developments
}

/// <summary>Pipeline stages from first contact to handover.</summary>
public enum ProjectStatus
{
    Lead = 0,
    Proposed = 1,
    Won = 2,
    InDesign = 3,
    InConstruction = 4,
    Complete = 5
}

/// <summary>Client value tier, auto-derived from booked lifetime spend.</summary>
public enum ClientTier
{
    Standard = 0,
    Premier = 1,
    Signature = 2
}

/// <summary>Where a lead originated.</summary>
public enum LeadSource
{
    Referral = 0,
    ArchitectPartner = 1,
    AwardsPress = 2,
    Website = 3,
    Event = 4
}

/// <summary>Lifecycle of a sales lead.</summary>
public enum LeadStatus
{
    New = 0,
    Qualified = 1,
    Nurturing = 2,
    Converted = 3,
    Lost = 4
}

/// <summary>Status of an incoming consultation request.</summary>
public enum RequestStatus
{
    New = 0,
    InReview = 1,
    Scheduled = 2,
    Closed = 3
}

/// <summary>Marketing channel for a campaign.</summary>
public enum CampaignChannel
{
    DesignAwards = 0,
    ArchitectureDigest = 1,
    PrivateEvents = 2,
    Search = 3,
    Social = 4,
    Referral = 5
}

/// <summary>Type of an in-app notification (drives the icon/colour).</summary>
public enum NotificationType
{
    Lead = 0,
    Project = 1,
    Client = 2,
    Award = 3,
    Request = 4,
    System = 5
}

/// <summary>Kind of media asset attached to a project.</summary>
public enum MediaKind
{
    Photograph = 0,
    Rendering = 1,
    SitePlan = 2
}

/// <summary>The nature of the client organisation.</summary>
public enum ClientType
{
    Developer = 0,
    REIT = 1,
    AirportAuthority = 2,
    ResortGroup = 3,
    CountryClub = 4,
    PrivateIndividual = 5,
    Corporate = 6,
    Municipality = 7
}
