using System.Net;
using System.Text.Json;
using api.Data;
using api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace tests;

public class UnitTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> factory;

    public UnitTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();
                services.AddDbContext<InsuranceDbContext>(options => 
                {
                    options.UseInMemoryDatabase("ApiInsuranceContextTesting");
                    options.UseInternalServiceProvider(serviceProvider);
                });
                using (var scope = factory.Server.Host.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
                    var logger = scope.ServiceProvider
                        .GetRequiredService<ILogger<WebApplicationFactory<Program>>>();
                    db.Database.EnsureCreated();
                    try
                    {
                        Seed(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"An error occurred seeding the database with test data. Error: {ex.Message}");
                    }
                }
            });
        });
    }

    private void Seed(InsuranceDbContext db)
    {
        db.AddRange(
            new Insurance
            {
                InsuranceId = 1,
                Name = "insurance 1",
                Value = 10000
            },
            new Insurance
            {
                InsuranceId = 2,
                Name = "insurance 2",
                ParentId = 1,
                Value = 20000
            },
            new Insurance
            {
                InsuranceId = 3,
                Name = "insurance 3",
                ParentId = 2,
                Value = 30000
            },
            new Insurance
            {
                InsuranceId = 4,
                Name = "insurance 4",
                Value = 40000
            },
            new Insurance
            {
                InsuranceId = 5,
                Name = "insurance 5",
                ParentId = 4,
                Value = 50000
            },
            new Insurance
            {
                InsuranceId = 6,
                Name = "insurance 6",
                ParentId = 4,
                Value = 50000
            },
            new Insurance
            {
                InsuranceId = 7,
                Name = "insurance 7",
                ParentId = 0,
                Value = 10000
            },
            new Insurance
            {
                InsuranceId = 8,
                Name = "insurance 8",
                ParentId = 7,
                Value = 100000
            },
            new Insurance
            {
                InsuranceId = 9,
                Name = "insurance 9",
                ParentId = 8,
                Value = 100
            });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.factory?.Dispose();
        }
    }

    // Requirements:
    // We need an endpoint that can return top combined values in insurances with depth restraints.
    //
    // Rules:
    // - The Value property of the Insurance model is the property we like to combine for our results.
    // - We need to be able to set the maximum amount of returned values. Utilize the maxCount parameter.
    // - We need to be able to limit the depth of children calulated. Utilize the maxDepth parameter.
    //
    // Seed data can be found in the Seed method. If you want to change it, feel free to do so.
    // However, know that the "expectedValues" in the theory are based on initial seed data and could give you a hint toward the implementation.
    [Theory]
    [InlineData(3, 2, new [] { 140000, 110000, 100100 })]
    [InlineData(1, 3, new [] { 200000 })]
    public async Task When_I_request_insurances_containing_highest_value_with_children_all_combined_I_expect_to_get_correct_values(int maxCount, int maxDepth, int[] expectedValues)
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"https://localhost:443/insurance/top/{maxCount}/maxdepth/{maxDepth}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var models = JsonSerializer.Deserialize<IEnumerable<Insurance>>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        models.Should().HaveCount(expectedValues.Count());
        models!.Select(x => x.Value).Should().BeEquivalentTo(expectedValues);
    }
}