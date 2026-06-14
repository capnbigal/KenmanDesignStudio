using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KenmanDesignStudio.Core.Entities;

namespace KenmanDesignStudio.Infrastructure.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        b.ToTable("Clients");
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.Monogram).HasMaxLength(4);
        b.Property(x => x.City).HasMaxLength(120);
        b.Property(x => x.Region).HasMaxLength(80);
        b.Property(x => x.Country).HasMaxLength(80);
        b.Property(x => x.Website).HasMaxLength(200);
        b.Property(x => x.About).HasMaxLength(1200);
        b.HasIndex(x => x.Name);

        b.HasMany(x => x.Contacts).WithOne(c => c.Client!).HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Projects).WithOne(p => p.Client!).HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> b)
    {
        b.ToTable("Contacts");
        b.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(80).IsRequired();
        b.Property(x => x.Title).HasMaxLength(120);
        b.Property(x => x.Email).HasMaxLength(160);
        b.Property(x => x.Phone).HasMaxLength(40);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("Projects");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CodeName).HasMaxLength(20);
        b.Property(x => x.City).HasMaxLength(120);
        b.Property(x => x.Region).HasMaxLength(80);
        b.Property(x => x.Country).HasMaxLength(80);
        b.Property(x => x.Value).HasPrecision(18, 2);
        b.Property(x => x.Summary).HasMaxLength(400);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.ArchitectPartner).HasMaxLength(160);
        b.HasIndex(x => x.Category);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.Year);

        b.HasMany(x => x.Media).WithOne(m => m.Project!).HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProjectMediaConfiguration : IEntityTypeConfiguration<ProjectMedia>
{
    public void Configure(EntityTypeBuilder<ProjectMedia> b)
    {
        b.ToTable("ProjectMedia");
        b.Property(x => x.Path).HasMaxLength(300).IsRequired();
        b.Property(x => x.Caption).HasMaxLength(200);
    }
}

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> b)
    {
        b.ToTable("Leads");
        b.Property(x => x.ContactName).HasMaxLength(120).IsRequired();
        b.Property(x => x.CompanyName).HasMaxLength(160);
        b.Property(x => x.Email).HasMaxLength(160);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Region).HasMaxLength(80);
        b.Property(x => x.EstimatedValue).HasPrecision(18, 2);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.HasIndex(x => x.Source);
        b.HasIndex(x => x.Status);

        b.HasOne(x => x.Campaign).WithMany(c => c.Leads).HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> b)
    {
        b.ToTable("Campaigns");
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.Spend).HasPrecision(18, 2);
        b.Property(x => x.RevenueAttributed).HasPrecision(18, 2);
    }
}

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> b)
    {
        b.ToTable("Requests");
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Email).HasMaxLength(160);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Company).HasMaxLength(160);
        b.Property(x => x.Region).HasMaxLength(80);
        b.Property(x => x.BudgetBand).HasMaxLength(40);
        b.Property(x => x.Message).HasMaxLength(2000);
        b.HasIndex(x => x.Status);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Message).HasMaxLength(600);
        b.Property(x => x.Link).HasMaxLength(200);
        b.HasIndex(x => x.OccurredAt);
    }
}

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> b)
    {
        b.ToTable("Testimonials");
        b.Property(x => x.Quote).HasMaxLength(800).IsRequired();
        b.Property(x => x.AuthorName).HasMaxLength(120);
        b.Property(x => x.AuthorTitle).HasMaxLength(160);
        b.Property(x => x.Company).HasMaxLength(160);
        b.Property(x => x.Monogram).HasMaxLength(4);
    }
}
