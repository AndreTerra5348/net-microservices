using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewPlatform(PlatformCreateEventDto platformPublishedDto);
    }
}