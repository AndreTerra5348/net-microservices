using System;
using System.Collections;
using System.Collections.Generic;
using CommandsService.Models;
using CommandsService.SyncDataService.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
                Migrate(dbContext);

                var grpcPlatformClient = scope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = grpcPlatformClient.ReturnAllPlatforms();

                var commandRepository = scope.ServiceProvider.GetService<ICommandRepo>();

                SeedData(commandRepository, platforms);
            }
        }

        private static void Migrate(AppDbContext dbContext)
        {
            System.Console.WriteLine(">>> Attempting to apply migrations");
            try
            {
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($">>> Error applying migrations: {ex.Message}");
            }
        }

        private static void SeedData(ICommandRepo repository, IEnumerable<Platform> platforms)
        {
            if (platforms == null)
            {
                System.Console.WriteLine(">>> Couldn't seed platforms");
                return;
            }

            System.Console.WriteLine(">>> Seeding platforms");
            foreach (var platform in platforms)
            {
                System.Console.WriteLine($">>> platform id {platform.Id}");
                System.Console.WriteLine($">>> platform external id {platform.ExternalId}");
                if (!repository.ExternalPlatformExists(platform.ExternalId))
                {
                    repository.CreatePlatform(platform);
                }
                repository.SaveChanges();
            }
        }
    }
}