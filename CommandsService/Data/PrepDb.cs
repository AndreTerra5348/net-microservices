using System.Collections;
using System.Collections.Generic;
using CommandsService.Models;
using CommandsService.SyncDataService.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var grpcPlatformClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = grpcPlatformClient.ReturnAllPlatforms();

                var commandRepository = serviceScope.ServiceProvider.GetService<ICommandRepo>();

                SeedData(commandRepository, platforms);
            }
        }

        private static void SeedData(ICommandRepo repository, IEnumerable<Platform> platforms)
        {
            System.Console.WriteLine(">>> Seeding platforms");
            foreach (var platform in platforms)
            {
                if (!repository.ExternalPlatformExists(platform.ExternalId))
                {
                    repository.CreatePlatform(platform);
                }
                repository.SaveChanges();
            }
        }
    }
}