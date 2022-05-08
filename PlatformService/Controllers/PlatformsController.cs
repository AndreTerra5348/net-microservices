using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(
            IPlatformRepo repository,
            IMapper mapper,
            IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetAllPlatforms()
        {
            var platforms = _repository.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platform = _repository.GetPlatformById(id);
            if (platform == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<PlatformReadDto>(platform));
        }

        [HttpPost]
        public ActionResult<PlatformReadDto> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            var platform = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(platform);
            _repository.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformReadDto>(platform);

            try
            {
                var platformCreateEventDto = _mapper.Map<PlatformCreateEventDto>(platform);
                platformCreateEventDto.Event = "Platform_Created";
                _messageBusClient.PublishEvent(platformCreateEventDto);
                Console.WriteLine($">>> Event sent: {platformCreateEventDto.Event}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Error sending Platform_Created event: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
        }

        [HttpDelete("{id}")]
        public ActionResult DeletePlatform(int id)
        {
            var platform = _repository.GetPlatformById(id);
            if (platform == null)
            {
                return NotFound();
            }

            _repository.DeletePlatform(platform);
            _repository.SaveChanges();

            try
            {
                var platformDeleteEventDto = _mapper.Map<PlatformDeleteEventDto>(platform);
                platformDeleteEventDto.Event = "Platform_Deleted";
                _messageBusClient.PublishEvent(platformDeleteEventDto);
                Console.WriteLine($">>> Event sent: {platformDeleteEventDto.Event}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> Error sending Platform_Deleted event: {ex.Message}");
            }

            return NoContent();
        }
    }
}