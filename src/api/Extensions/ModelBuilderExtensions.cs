using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions;

public static class ModelBuilderExtensions
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Insurance>()
            .HasData(new Insurance
            {
                InsuranceId = 1,
                Name = "insurance 1"
            },
            new Insurance
            {
                InsuranceId = 2,
                Name = "insurance 2",
                ParentId = 1
            });
    }
}