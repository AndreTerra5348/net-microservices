using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishPlatformEvent(PlatformCreateEventDto platformPublishedDto);
    }
}