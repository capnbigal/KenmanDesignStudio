using Microsoft.EntityFrameworkCore;
using KenmanDesignStudio.Core.Common;
using KenmanDesignStudio.Core.Entities;
using KenmanDesignStudio.Core.Enums;
using KenmanDesignStudio.Infrastructure.Data;

namespace KenmanDesignStudio.Infrastructure.Seeding;

/// <summary>
/// Builds a large, internally-consistent demo dataset and writes on-brand SVG plates for every
/// project image. Deterministic (fixed RNG seed) so re-seeding reproduces the same world.
/// </summary>
public static class DataSeeder
{
    private const int Seed = 73_115;

    public static async Task SeedAsync(AppDbContext db, string webRootPath)
    {
        if (await db.Clients.AnyAsync())
            return;

        EnsureImageFolders(webRootPath);

        var rng = new Random(Seed);
        var now = DateTime.UtcNow;

        // ---------- Phase 1: clients, projects, media, contacts ----------
        var clients = BuildClients(rng, now, webRootPath);
        db.Clients.AddRange(clients);
        await db.SaveChangesAsync();

        // ---------- Phase 2: campaigns, leads, requests, testimonials, notifications ----------
        var campaigns = BuildCampaigns(rng, now);
        var leads = BuildLeads(rng, now, campaigns, clients);
        ReconcileCampaigns(campaigns, leads);

        db.Campaigns.AddRange(campaigns);
        db.Leads.AddRange(leads);
        db.Requests.AddRange(BuildRequests(rng, now));
        db.Testimonials.AddRange(BuildTestimonials());
        db.Notifications.AddRange(BuildNotifications(rng, now, clients, leads));
        await db.SaveChangesAsync();
    }

    // ----------------------------------------------------------------- clients

    private static List<Client> BuildClients(Random rng, DateTime now, string webRoot)
    {
        var clients = new List<Client>();
        var seq = 1;

        foreach (var cs in SeedPools.Clients)
        {
            var place = Pick(rng, SeedPools.Places);
            var client = new Client
            {
                Name = cs.Name,
                Type = cs.Type,
                Monogram = Monogram(cs.Name),
                City = place.City,
                Region = place.Region,
                Country = place.Country,
                About = AboutText(cs),
                Website = "www." + Slugify(cs.Name) + DomainSuffix(cs.Type),
                ClientSince = now.AddDays(-rng.Next(220, 9 * 365)),
            };

            // Contacts (1-2).
            var contactCount = rng.Next(1, 3);
            for (var i = 0; i < contactCount; i++)
            {
                var fn = Pick(rng, SeedPools.FirstNames);
                var ln = cs.Type == ClientType.PrivateIndividual ? FamilyName(cs.Name) : Pick(rng, SeedPools.LastNames);
                client.Contacts.Add(new Contact
                {
                    FirstName = fn,
                    LastName = ln,
                    Title = cs.Type == ClientType.PrivateIndividual ? (i == 0 ? "Principal" : "Estate Manager") : Pick(rng, SeedPools.ContactTitles),
                    Email = $"{fn.ToLowerInvariant()}.{ln.ToLowerInvariant()}@{Slugify(cs.Name)}{DomainSuffix(cs.Type)}",
                    Phone = $"+1 ({rng.Next(201, 989)}) {rng.Next(200, 999)}-{rng.Next(1000, 9999)}",
                    IsPrimary = i == 0,
                });
            }

            // Projects: count & scale driven by client "scale".
            var projectCount = cs.Scale switch
            {
                5 => rng.Next(4, 7),
                4 => rng.Next(3, 6),
                3 => rng.Next(2, 4),
                2 => rng.Next(1, 3),
                _ => 1,
            };

            var preferredCategory = PreferredCategory(cs.Type);

            for (var p = 0; p < projectCount; p++)
            {
                var category = p == 0 || rng.NextDouble() < 0.6
                    ? preferredCategory
                    : RandomCategory(rng);

                var project = BuildProject(rng, now, cs, category, place, seq++, webRoot);
                client.Projects.Add(project);
            }

            client.IsRepeatClient = client.Projects.Count(p => p.Status.IsBooked()) > 1;

            // Anchor "client since" to first project, and set last-contact (some intentionally stale).
            var firstStart = client.Projects.Select(p => p.StartDate).DefaultIfEmpty(client.ClientSince).Min();
            if (firstStart < client.ClientSince) client.ClientSince = firstStart.AddDays(-rng.Next(20, 160));
            client.LastContactDate = rng.NextDouble() < 0.2
                ? now.AddDays(-rng.Next(95, 240))   // triggers "no contact in 90 days"
                : now.AddDays(-rng.Next(1, 75));

            clients.Add(client);
        }

        return clients;
    }

    private static Project BuildProject(Random rng, DateTime now, SeedPools.ClientSeed cs,
        ProjectCategory category, SeedPools.Place place, int seq, string webRoot)
    {
        // Date over the last ~38 months, weighted so there is history + active pipeline.
        var monthsAgo = WeightedMonths(rng);
        var start = now.AddMonths(-monthsAgo).AddDays(rng.Next(0, 27));
        var durationMonths = rng.Next(6, 28);

        var status = StatusFor(rng, monthsAgo, durationMonths);
        DateTime? completion = null;
        var progress = 0;
        switch (status)
        {
            case ProjectStatus.Complete:
                completion = start.AddMonths(durationMonths);
                if (completion > now) completion = now.AddDays(-rng.Next(10, 120));
                progress = 100;
                break;
            case ProjectStatus.InConstruction:
                progress = rng.Next(25, 88);
                break;
            case ProjectStatus.InDesign:
                progress = rng.Next(8, 30);
                break;
        }

        var (min, max) = SeedPools.ValueBands[category];
        // Scale nudges the value upward for larger clients.
        var scaleBias = 0.55 + cs.Scale * 0.09;
        var value = RoundMoney((decimal)((double)min + (double)(max - min) * Math.Pow(rng.NextDouble(), 1.6) * scaleBias));

        var name = ProjectName(rng, category, place);
        var year = (completion ?? start).Year;
        // Project code carries the new brand prefix; image plate filenames keep the legacy "va-"
        // stem so the photographs already dropped beside each plate stay matched.
        var num = $"{year % 100:D2}{seq:D3}";
        var code = $"KDS-{num}";

        var project = new Project
        {
            Name = name,
            CodeName = code,
            Category = category,
            Status = status,
            City = place.City,
            Region = place.Region,
            Country = place.Country,
            Value = value,
            StartDate = start,
            CompletionDate = completion,
            Year = year,
            SiteAreaAcres = Math.Round(category switch
            {
                ProjectCategory.GolfCountryClub => rng.NextDouble() * 140 + 60,
                ProjectCategory.AirportTransit => rng.NextDouble() * 40 + 10,
                ProjectCategory.CorporateCampus => rng.NextDouble() * 25 + 5,
                ProjectCategory.LuxuryResort => rng.NextDouble() * 30 + 8,
                ProjectCategory.PrivateEstate => rng.NextDouble() * 12 + 2,
                _ => rng.NextDouble() * 3 + 0.4,
            }, 1),
            ArchitectPartner = rng.NextDouble() < 0.55 ? Pick(rng, SeedPools.ArchitectPartners) : null,
            Source = (LeadSource)rng.Next(0, 5),
            IsFeatured = false,
            ProgressPercent = progress,
            Summary = SummaryText(category, place),
            Description = DescriptionText(category, name, place),
        };

        // Media plates (3-6).
        var slug = CategoryCatalog.Slug(category);
        var mediaCount = rng.Next(3, 7);
        for (var m = 0; m < mediaCount; m++)
        {
            var kind = m == 0 ? MediaKind.Photograph
                : m == mediaCount - 1 ? MediaKind.SitePlan
                : (rng.NextDouble() < 0.5 ? MediaKind.Photograph : MediaKind.Rendering);

            var fileName = $"va-{num}-{m + 1}.svg";
            var relPath = $"/images/projects/{slug}/{fileName}";
            WritePlate(webRoot, relPath, category, name, place.City, code, m);

            project.Media.Add(new ProjectMedia
            {
                Path = relPath,
                Kind = kind,
                Caption = CaptionFor(kind, m),
                SortOrder = m,
                IsPrimary = m == 0,
            });
        }

        return project;
    }

    // ----------------------------------------------------------------- campaigns & leads

    private static List<Campaign> BuildCampaigns(Random rng, DateTime now)
    {
        var defs = new (string Name, CampaignChannel Channel, decimal Spend, int MonthsAgo, bool Active)[]
        {
            ("ASLA Honor Awards Showcase", CampaignChannel.DesignAwards, 145_000m, 11, false),
            ("Architectural Digest Feature", CampaignChannel.ArchitectureDigest, 220_000m, 8, true),
            ("Private Collectors' Salon", CampaignChannel.PrivateEvents, 96_000m, 5, true),
            ("Search — Luxury Landscape", CampaignChannel.Search, 64_000m, 6, true),
            ("Social — Portfolio Films", CampaignChannel.Social, 48_000m, 4, true),
            ("Architect Partner Program", CampaignChannel.Referral, 38_000m, 14, true),
        };

        return defs.Select(d => new Campaign
        {
            Name = d.Name,
            Channel = d.Channel,
            Spend = d.Spend,
            StartDate = now.AddMonths(-d.MonthsAgo),
            EndDate = d.Active ? null : now.AddMonths(-d.MonthsAgo + 3),
            IsActive = d.Active,
        }).ToList();
    }

    private static List<Lead> BuildLeads(Random rng, DateTime now, List<Campaign> campaigns, List<Client> clients)
    {
        var leads = new List<Lead>();
        // Target ~52 leads with a healthy conversion profile.
        const int total = 52;

        for (var i = 0; i < total; i++)
        {
            var source = WeightedSource(rng);
            var category = RandomCategory(rng);
            var place = Pick(rng, SeedPools.Places);
            var fn = Pick(rng, SeedPools.FirstNames);
            var ln = Pick(rng, SeedPools.LastNames);
            var status = WeightedLeadStatus(rng, i, total);

            var campaign = MatchCampaign(rng, campaigns, source);

            var received = now.AddDays(-rng.Next(2, 330));
            var lead = new Lead
            {
                ContactName = $"{fn} {ln}",
                CompanyName = CompanyForSource(rng, source, category),
                Email = $"{fn.ToLowerInvariant()}.{ln.ToLowerInvariant()}@example.com",
                Phone = $"+1 ({rng.Next(201, 989)}) {rng.Next(200, 999)}-{rng.Next(1000, 9999)}",
                Source = source,
                Status = status,
                InterestCategory = category,
                Region = place.Region,
                EstimatedValue = RoundMoney((decimal)(rng.NextDouble() * 14_000_000 + 800_000)),
                ReceivedDate = received,
                Campaign = campaign,
                Notes = LeadNote(category, place),
            };

            if (status == LeadStatus.Converted)
            {
                lead.ConvertedClientId = Pick(rng, clients).Id;
                lead.ConvertedDate = received.AddDays(rng.Next(20, 120));
            }

            leads.Add(lead);
        }

        return leads;
    }

    private static void ReconcileCampaigns(List<Campaign> campaigns, List<Lead> leads)
    {
        foreach (var c in campaigns)
        {
            var attributed = leads.Where(l => ReferenceEquals(l.Campaign, c)).ToList();
            c.LeadsGenerated = attributed.Count;
            c.Conversions = attributed.Count(l => l.Status == LeadStatus.Converted);
            c.RevenueAttributed = attributed.Where(l => l.Status == LeadStatus.Converted)
                                            .Sum(l => l.EstimatedValue);
        }
    }

    // ----------------------------------------------------------------- requests / testimonials / notifications

    private static List<Request> BuildRequests(Random rng, DateTime now)
    {
        var budgets = new[] { "$500K – $1M", "$1M – $2.5M", "$2.5M – $5M", "$5M – $10M", "$10M+" };
        var msgs = new[]
        {
            "We are developing a new waterfront property and would like to discuss a signature landscape vision.",
            "Seeking a design-led partner for the rooftop amenity terrace of our flagship tower.",
            "Our board is exploring a full restoration of the club grounds ahead of our centennial.",
            "Interested in a private consultation regarding our estate's gardens and motor court.",
            "Planning a new resort and want to understand your approach to arrival-sequence landscapes.",
            "We'd love to talk about biophilic design for our new corporate headquarters campus.",
        };
        var statuses = new[] { RequestStatus.New, RequestStatus.New, RequestStatus.New, RequestStatus.InReview, RequestStatus.InReview, RequestStatus.Scheduled, RequestStatus.Closed };

        var list = new List<Request>();
        var count = rng.Next(13, 18);
        for (var i = 0; i < count; i++)
        {
            var fn = Pick(rng, SeedPools.FirstNames);
            var ln = Pick(rng, SeedPools.LastNames);
            var place = Pick(rng, SeedPools.Places);
            list.Add(new Request
            {
                Name = $"{fn} {ln}",
                Email = $"{fn.ToLowerInvariant()}.{ln.ToLowerInvariant()}@example.com",
                Phone = $"+1 ({rng.Next(201, 989)}) {rng.Next(200, 999)}-{rng.Next(1000, 9999)}",
                Company = rng.NextDouble() < 0.6 ? $"{Pick(rng, SeedPools.MarqueeNames)} Group" : null,
                InterestCategory = RandomCategory(rng),
                Region = place.Region,
                BudgetBand = Pick(rng, budgets),
                Message = Pick(rng, msgs),
                Status = Pick(rng, statuses),
                SubmittedAt = now.AddDays(-rng.Next(0, 55)).AddHours(-rng.Next(0, 23)),
            });
        }
        return list.OrderByDescending(r => r.SubmittedAt).ToList();
    }

    private static List<Testimonial> BuildTestimonials()
    {
        return new List<Testimonial>
        {
            new() { Quote = "Kenman Design Studio reimagined our flagship tower's crown as a living sky garden. It has become the single most photographed space in the building.", AuthorName = "Eleanor Marquis", AuthorTitle = "Principal", Company = "Marquis Development Group", Monogram = "MD", Category = ProjectCategory.RooftopSkyGarden, IsFeatured = true },
            new() { Quote = "They understood that an airport's first impression is its landscape. Arrivals now feel like an embrace rather than a corridor.", AuthorName = "Marcus Ellsworth", AuthorTitle = "Director of Capital Projects", Company = "Coastal Aviation Authority", Monogram = "CA", Category = ProjectCategory.AirportTransit, IsFeatured = true },
            new() { Quote = "Every garden journey on the property tells a story. Our guests don't just stay with us — they remember us.", AuthorName = "Vivienne Laurent", AuthorTitle = "VP of Brand Experience", Company = "Aurelia Resorts & Spas", Monogram = "AR", Category = ProjectCategory.LuxuryResort, IsFeatured = true },
            new() { Quote = "Discreet, exacting, and utterly world-class. Our gardens are now the heart of the estate.", AuthorName = "Theodore Carrington", AuthorTitle = "Owner", Company = "The Carrington Residence", Monogram = "TC", Category = ProjectCategory.PrivateEstate, IsFeatured = false },
            new() { Quote = "The course restoration honoured a century of history while making it play beautifully for the next hundred years.", AuthorName = "Genevieve Harrington", AuthorTitle = "Club President", Company = "Pinehurst National Club", Monogram = "PN", Category = ProjectCategory.GolfCountryClub, IsFeatured = true },
            new() { Quote = "Our campus is now a destination. Recruitment, wellbeing, brand — the landscape moved every metric that matters.", AuthorName = "Julian Vance", AuthorTitle = "Chief Operating Officer", Company = "Helios Technologies", Monogram = "HT", Category = ProjectCategory.CorporateCampus, IsFeatured = false },
            new() { Quote = "From concept films to the final planting, the craft was flawless. A true atelier in every sense.", AuthorName = "Camille Beaumont", AuthorTitle = "Managing Director", Company = "Maison Lumière Resorts", Monogram = "ML", Category = ProjectCategory.LuxuryResort, IsFeatured = false },
            new() { Quote = "They delivered a landmark on a complex high-rise envelope, on time and beyond the brief.", AuthorName = "Sebastian Northrop", AuthorTitle = "Head of Development", Company = "Sterling Harbor Partners", Monogram = "SH", Category = ProjectCategory.RooftopSkyGarden, IsFeatured = false },
            new() { Quote = "A rare firm that pairs ecological intelligence with genuine artistry. We would not build with anyone else.", AuthorName = "Beatrice Kingsley", AuthorTitle = "Director of Real Estate", Company = "Pinnacle Equities Trust", Monogram = "PE", Category = ProjectCategory.CorporateCampus, IsFeatured = true },
        };
    }

    private static List<Notification> BuildNotifications(Random rng, DateTime now, List<Client> clients, List<Lead> leads)
    {
        var list = new List<Notification>();
        void Add(NotificationType t, string title, string msg, int hoursAgo, string? link = null, bool read = false)
            => list.Add(new Notification { Type = t, Title = title, Message = msg, OccurredAt = now.AddHours(-hoursAgo), Link = link, IsRead = read });

        var signature = clients.Where(c => c.Tier == ClientTier.Signature).ToList();
        var topClient = signature.OrderByDescending(c => c.LifetimeValue).FirstOrDefault() ?? clients.First();
        var recentConverted = leads.Where(l => l.Status == LeadStatus.Converted).OrderByDescending(l => l.ConvertedDate).FirstOrDefault();

        var inConstruction = clients.SelectMany(c => c.Projects).FirstOrDefault(p => p.Status == ProjectStatus.InConstruction);

        Add(NotificationType.Lead, "New consultation request", "A rooftop sky-garden enquiry arrived via the website.", 2, "/admin/requests");
        if (inConstruction is not null)
            Add(NotificationType.Project, "Project advanced to In Construction", $"\"{inConstruction.Name}\" broke ground this week.", 6, "/admin/projects");
        Add(NotificationType.Award, "Award shortlist", "Two projects shortlisted for the ASLA Professional Honor Awards.", 20);
        Add(NotificationType.Client, "Signature client milestone", $"{topClient.Name} surpassed {Money(topClient.LifetimeValue)} in lifetime commissions.", 28, $"/admin/clients/{topClient.Id}");
        if (recentConverted is not null)
            Add(NotificationType.Lead, "Lead converted", $"{recentConverted.ContactName} converted — estimated {Money(recentConverted.EstimatedValue)}.", 34, "/admin/leads", true);
        Add(NotificationType.Request, "Consultation scheduled", "A private estate consultation was scheduled for next Tuesday.", 46, "/admin/requests", true);
        Add(NotificationType.Project, "Proposal sent", "A resort grounds proposal was issued to Azure Coast Hotels.", 52, "/admin/projects", true);
        Add(NotificationType.Client, "No recent contact", "3 Signature clients have had no contact in 90+ days.", 60, "/admin/clients");
        Add(NotificationType.Project, "Design milestone approved", "Schematic design approved for a corporate campus commons.", 74, "/admin/projects", true);
        Add(NotificationType.Award, "Press feature", "A sky-garden project was featured in Architectural Digest.", 96, null, true);
        Add(NotificationType.System, "Quarter close", "Q-to-date booked revenue is pacing 14% ahead of plan.", 120, "/admin", true);
        Add(NotificationType.Lead, "New lead from event", "A new lead was captured at the Private Collectors' Salon.", 140, "/admin/leads", true);
        Add(NotificationType.Project, "Project completed", "A golf course restoration reached practical completion.", 160, "/admin/projects", true);
        Add(NotificationType.Client, "New repeat commission", "An existing developer client commissioned a second tower terrace.", 180, "/admin/clients", true);

        return list;
    }

    // ----------------------------------------------------------------- text + helpers

    private static ProjectCategory PreferredCategory(ClientType type) => type switch
    {
        ClientType.AirportAuthority => ProjectCategory.AirportTransit,
        ClientType.ResortGroup => ProjectCategory.LuxuryResort,
        ClientType.CountryClub => ProjectCategory.GolfCountryClub,
        ClientType.Corporate => ProjectCategory.CorporateCampus,
        ClientType.PrivateIndividual => ProjectCategory.PrivateEstate,
        ClientType.Developer => ProjectCategory.RooftopSkyGarden,
        ClientType.REIT => ProjectCategory.CorporateCampus,
        _ => ProjectCategory.RooftopSkyGarden,
    };

    private static ProjectCategory RandomCategory(Random rng) =>
        (ProjectCategory)(rng.Next(0, 6) + 1);

    private static int WeightedMonths(Random rng)
    {
        // Bias toward the last ~24 months but keep a 3-year tail of history.
        var r = rng.NextDouble();
        return r switch
        {
            < 0.18 => rng.Next(0, 6),
            < 0.45 => rng.Next(6, 14),
            < 0.75 => rng.Next(14, 24),
            _ => rng.Next(24, 38),
        };
    }

    private static ProjectStatus StatusFor(Random rng, int monthsAgo, int durationMonths)
    {
        // Old enough to have finished → mostly complete; recent → active or pipeline.
        if (monthsAgo >= durationMonths + 2) return ProjectStatus.Complete;
        if (monthsAgo >= 14) return rng.NextDouble() < 0.7 ? ProjectStatus.Complete : ProjectStatus.InConstruction;
        if (monthsAgo >= 8) return Weighted(rng, (ProjectStatus.InConstruction, 5), (ProjectStatus.InDesign, 2), (ProjectStatus.Complete, 2));
        if (monthsAgo >= 4) return Weighted(rng, (ProjectStatus.InDesign, 4), (ProjectStatus.Won, 3), (ProjectStatus.InConstruction, 2));
        if (monthsAgo >= 2) return Weighted(rng, (ProjectStatus.Won, 3), (ProjectStatus.Proposed, 4), (ProjectStatus.InDesign, 2));
        return Weighted(rng, (ProjectStatus.Proposed, 4), (ProjectStatus.Lead, 4), (ProjectStatus.Won, 2));
    }

    private static LeadSource WeightedSource(Random rng) =>
        Weighted(rng, (LeadSource.Referral, 5), (LeadSource.ArchitectPartner, 4),
                      (LeadSource.AwardsPress, 3), (LeadSource.Website, 4), (LeadSource.Event, 2));

    private static LeadStatus WeightedLeadStatus(Random rng, int index, int total)
    {
        // ~35% converted, ~12% lost, rest in-flight → ~75% win rate among decided leads.
        return Weighted(rng, (LeadStatus.Converted, 35), (LeadStatus.Qualified, 22),
                             (LeadStatus.Nurturing, 18), (LeadStatus.New, 13), (LeadStatus.Lost, 12));
    }

    private static Campaign? MatchCampaign(Random rng, List<Campaign> campaigns, LeadSource source)
    {
        Campaign? Find(CampaignChannel ch) => campaigns.FirstOrDefault(c => c.Channel == ch);
        return source switch
        {
            LeadSource.AwardsPress => rng.NextDouble() < 0.85 ? (rng.NextDouble() < 0.5 ? Find(CampaignChannel.DesignAwards) : Find(CampaignChannel.ArchitectureDigest)) : null,
            LeadSource.Event => rng.NextDouble() < 0.8 ? Find(CampaignChannel.PrivateEvents) : null,
            LeadSource.Website => rng.NextDouble() < 0.75 ? (rng.NextDouble() < 0.6 ? Find(CampaignChannel.Search) : Find(CampaignChannel.Social)) : null,
            LeadSource.ArchitectPartner => rng.NextDouble() < 0.7 ? Find(CampaignChannel.Referral) : null,
            LeadSource.Referral => rng.NextDouble() < 0.35 ? Find(CampaignChannel.Referral) : null,
            _ => null,
        };
    }

    private static string ProjectName(Random rng, ProjectCategory category, SeedPools.Place place)
    {
        var descriptor = Pick(rng, SeedPools.Descriptors[category]);
        var marquee = Pick(rng, SeedPools.MarqueeNames);
        var roll = rng.NextDouble();
        if (category == ProjectCategory.PrivateEstate || roll < 0.45)
            return $"{marquee} {descriptor}";
        if (roll < 0.75)
            return $"{place.City.Split(',')[0]} {descriptor}";
        // "The …" form — avoid a double "The" when the marquee name already starts with it.
        return marquee.StartsWith("The ", StringComparison.Ordinal)
            ? $"{marquee} {descriptor}"
            : $"The {marquee} {descriptor}";
    }

    private static string SummaryText(ProjectCategory category, SeedPools.Place place)
    {
        var meta = CategoryCatalog.Get(category);
        return $"{meta.Tagline} — a {meta.ShortName.ToLowerInvariant()} commission in {place.City}.";
    }

    private static string DescriptionText(ProjectCategory category, string name, SeedPools.Place place)
    {
        var meta = CategoryCatalog.Get(category);
        return $"{name} is a {meta.Name.ToLowerInvariant()} commission in {place.City}. {meta.Blurb} " +
               "Our scope spanned concept design, planting strategy, hardscape and water features, " +
               "lighting design, and full construction-phase stewardship — delivering a landscape of " +
               "lasting ecological and aesthetic value.";
    }

    private static string CaptionFor(MediaKind kind, int index) => kind switch
    {
        MediaKind.SitePlan => "Master site plan",
        MediaKind.Rendering => index % 2 == 0 ? "Concept rendering — twilight" : "Design rendering",
        _ => index switch
        {
            0 => "Signature view",
            1 => "Garden terrace",
            2 => "Water feature detail",
            _ => "Landscape composition",
        },
    };

    private static string AboutText(SeedPools.ClientSeed cs) => cs.Type switch
    {
        ClientType.Developer => $"{cs.Name} develops landmark mixed-use and residential towers across premier markets.",
        ClientType.REIT => $"{cs.Name} owns and operates a portfolio of trophy commercial assets.",
        ClientType.AirportAuthority => $"{cs.Name} operates major aviation gateways serving tens of millions of passengers.",
        ClientType.ResortGroup => $"{cs.Name} curates five-star resorts and spa retreats in sought-after destinations.",
        ClientType.CountryClub => $"{cs.Name} is a private members' club with a celebrated golf and social tradition.",
        ClientType.Corporate => $"{cs.Name} is a global enterprise investing in landmark campus environments.",
        ClientType.PrivateIndividual => $"{cs.Name} is a private client commissioning bespoke estate landscapes.",
        _ => cs.Name,
    };

    private static string LeadNote(ProjectCategory category, SeedPools.Place place) =>
        $"Interested in {CategoryCatalog.Get(category).ShortName.ToLowerInvariant()} work in the {place.Region} region.";

    private static string CompanyForSource(Random rng, LeadSource source, ProjectCategory category) =>
        source == LeadSource.Referral && rng.NextDouble() < 0.4
            ? $"Private — {Pick(rng, SeedPools.LastNames)} Family"
            : $"{Pick(rng, SeedPools.MarqueeNames)} {(category == ProjectCategory.CorporateCampus ? "Holdings" : "Group")}";

    // ---- low-level utilities ----

    private static T Pick<T>(Random rng, IReadOnlyList<T> items) => items[rng.Next(items.Count)];

    private static TEnum Weighted<TEnum>(Random rng, params (TEnum Value, int Weight)[] options)
    {
        var total = options.Sum(o => o.Weight);
        var roll = rng.Next(total);
        var acc = 0;
        foreach (var (value, weight) in options)
        {
            acc += weight;
            if (roll < acc) return value;
        }
        return options[^1].Value;
    }

    private static decimal RoundMoney(decimal v) => Math.Round(v / 50_000m) * 50_000m;

    private static string Money(decimal v) => v >= 1_000_000m
        ? $"${v / 1_000_000m:0.0}M"
        : $"${v / 1_000m:0}K";

    private static string Monogram(string name)
    {
        var clean = name.Replace("The ", "").Replace("&", " ");
        var words = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1) return words[0][..Math.Min(2, words[0].Length)].ToUpperInvariant();
        return (words[0][0].ToString() + words[1][0]).ToUpperInvariant();
    }

    private static string FamilyName(string clientName)
    {
        var clean = clientName.Replace("The ", "").Replace(" Estate", "").Replace(" Residence", "")
            .Replace(" Family", "").Replace(" Hall", "").Replace(" Point", "");
        return clean.Split(' ')[0];
    }

    private static string Slugify(string s) =>
        new string(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static string DomainSuffix(ClientType type) => type switch
    {
        ClientType.AirportAuthority or ClientType.Municipality => ".gov",
        ClientType.PrivateIndividual => ".com",
        _ => ".com",
    };

    private static void WritePlate(string webRoot, string relPath, ProjectCategory category,
        string name, string city, string code, int variant)
    {
        var full = System.IO.Path.Combine(webRoot, relPath.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar));
        var dir = System.IO.Path.GetDirectoryName(full)!;
        System.IO.Directory.CreateDirectory(dir);
        if (System.IO.File.Exists(full)) return; // never clobber a real dropped-in photo with same name
        var svg = PlaceholderArt.Build(category, name, city, code, variant);
        System.IO.File.WriteAllText(full, svg);
    }

    private static void EnsureImageFolders(string webRoot)
    {
        var root = System.IO.Path.Combine(webRoot, "images", "projects");
        System.IO.Directory.CreateDirectory(root);

        foreach (var meta in CategoryCatalog.All)
        {
            var dir = System.IO.Path.Combine(root, meta.Slug);
            System.IO.Directory.CreateDirectory(dir);
            var readme = System.IO.Path.Combine(dir, "README.txt");
            if (!System.IO.File.Exists(readme))
            {
                System.IO.File.WriteAllText(readme,
                    $"{meta.Name}\r\n" +
                    new string('-', meta.Name.Length) + "\r\n\r\n" +
                    "Drop real project photographs into this folder to replace the generated SVG plates.\r\n" +
                    "Recommended: landscape orientation, 1600x1200 or larger, .jpg or .webp.\r\n\r\n" +
                    "To make a photo appear on a specific project, name it exactly like the generated\r\n" +
                    "plate it should replace (e.g. va-24012-1.svg -> va-24012-1.jpg) — the app prefers a\r\n" +
                    "real raster file over the .svg of the same name. Or re-seed after editing ProjectMedia.\r\n");
            }
        }
    }
}
