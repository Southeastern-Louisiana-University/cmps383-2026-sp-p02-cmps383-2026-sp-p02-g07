using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Selu383.SP26.Api.Features.Locations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Address)
            .IsRequired();

        // Seed data - at least 3 locations required
        builder.HasData(
            new Location
            {
                Id = 1,
                Name = "Caffeinated Lions Downtown",
                Address = "123 Main Street, Hammond, LA 70401",
                TableCount = 15
            },
            new Location
            {
                Id = 2,
                Name = "Caffeinated Lions Uptown",
                Address = "456 Oak Avenue, Hammond, LA 70403",
                TableCount = 20
            },
            new Location
            {
                Id = 3,
                Name = "Caffeinated Lions Lakeside",
                Address = "789 Lake Drive, Mandeville, LA 70448",
                TableCount = 12
            }
        );
    }
}