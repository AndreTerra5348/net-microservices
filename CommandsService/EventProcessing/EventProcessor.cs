using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.PlatformCreated:
                    AddPlatform(message);
                    break;
                case EventType.PlatformDeleted:
                    DeletePlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            System.Console.WriteLine($">>> Determining event for: {notificationMessage}");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
            switch (eventType.Event)
            {
                case "Platform_Created":
                    System.Console.WriteLine(">>> Platform published event");
                    return EventType.PlatformCreated;
                case "Platform_Deleted":
                    System.Console.WriteLine(">>> Platform deleted event");
                    return EventType.PlatformDeleted;
                default:
                    System.Console.WriteLine(">>> Undetermined event");
                    return EventType.Undetermined;

            }
        }

        private void AddPlatform(string message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformCreatedDto = JsonSerializer.Deserialize<PlatformCreateEventDto>(message);

                try
                {
                    var platform = _mapper.Map<Platform>(platformCreatedDto);
                    if (!repository.ExternalPlatformExists(platform.ExternalId))
                    {
                        repository.CreatePlatform(platform);
                        repository.SaveChanges();
                        System.Console.WriteLine(">>> Platform added");
                    }
                    else
                    {
                        System.Console.WriteLine(">>> Platform already exists");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($">>> Couldn't add platform: {ex.Message}");
                }
            }
        }

        private void DeletePlatform(string message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformDeletedEventDto = JsonSerializer.Deserialize<PlatformDeleteEventDto>(message);

                try
                {
                    var deletedPlatform = _mapper.Map<Platform>(platformDeletedEventDto);
                    var platform = repository.GetPlatformByExternalId(deletedPlatform.ExternalId);
                    if (platform != null)
                    {
                        repository.DeletePlatform(platform);
                        repository.SaveChanges();
                        System.Console.WriteLine(">>> Platform deleted");
                    }
                    else
                    {
                        System.Console.WriteLine(">>> Platform doesn't exist");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($">>> Couldn't delete platform: {ex.Message}");
                }
            }
        }
    }

    enum EventType
    {
        PlatformCreated,
        PlatformDeleted,
        Undetermined
    }
}