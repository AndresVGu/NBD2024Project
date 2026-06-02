using Microsoft.EntityFrameworkCore;
using NBDProject2024.Models;

namespace NBDProject2024.Data
{
    public static class LookupDataInitializer
    {
        public static async Task SeedAsync(IApplicationBuilder applicationBuilder)
        {
            await using var scope = applicationBuilder.ApplicationServices.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<NBDContext>();

            // Keep schema current before checking lookup records.
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync();
            }

            if (!await context.Provinces.AnyAsync())
            {
                var provinces = new List<Province>
                {
                    new() { ID = "AB", Name = "Alberta" },
                    new() { ID = "BC", Name = "British Columbia" },
                    new() { ID = "MB", Name = "Manitoba" },
                    new() { ID = "NB", Name = "New Brunswick" },
                    new() { ID = "NL", Name = "Newfoundland and Labrador" },
                    new() { ID = "NS", Name = "Nova Scotia" },
                    new() { ID = "NT", Name = "Northwest Territories" },
                    new() { ID = "NU", Name = "Nunavut" },
                    new() { ID = "ON", Name = "Ontario" },
                    new() { ID = "PE", Name = "Prince Edward Island" },
                    new() { ID = "QC", Name = "Quebec" },
                    new() { ID = "SK", Name = "Saskatchewan" },
                    new() { ID = "YT", Name = "Yukon" }
                };

                context.Provinces.AddRange(provinces);
                await context.SaveChangesAsync();
            }

            if (!await context.Cities.AnyAsync())
            {
                var cities = new List<City>
                {
                    new() { Name = "Toronto", ProvinceID = "ON" },
                    new() { Name = "Halifax", ProvinceID = "NS" },
                    new() { Name = "Calgary", ProvinceID = "AB" }
                };

                context.Cities.AddRange(cities);
                await context.SaveChangesAsync();
            }
        }
    }
}
