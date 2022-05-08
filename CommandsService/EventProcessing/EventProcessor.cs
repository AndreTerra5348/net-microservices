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
                case EventType.PlatformPublished:
                    AddPlatform(message);
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
                case "Platform_Published":
                    System.Console.WriteLine(">>> Platform published event");
                    return EventType.PlatformPublished;
                default:
                    System.Console.WriteLine(">>> Undetermined event");
                    return EventType.Undetermined;

            }
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try
                {
                    var platform = _mapper.Map<Platform>(platformPublishedDto);
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
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}