using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationController : ControllerBase
    {
        private readonly IDestinationService _destinationService;

        public DestinationController(IDestinationService destinationService)
        {
            _destinationService = destinationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DestinationDTO>>> GetAllDestinations()
        {
            var destinations = await _destinationService.GetAllDestinationsAsync();
            var destinationDtos = destinations.Select(MapDestinationToDto).ToList();
            return Ok(destinationDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DestinationDTO>> GetDestination(int id)
        {
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null)
                return NotFound();

            return Ok(MapDestinationToDto(destination));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DestinationDTO>> CreateDestination(CreateDestinationDTO destinationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var destination = new Destination
            {
                Name = destinationDto.Name,
                Description = destinationDto.Description,
                Country = destinationDto.Country,
                City = destinationDto.City,
                ImageUrl = destinationDto.ImageUrl
            };

            var createdDestination = await _destinationService.CreateDestinationAsync(destination);
            return CreatedAtAction(nameof(GetDestination), new { id = createdDestination.Id },
                MapDestinationToDto(createdDestination));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DestinationDTO>> UpdateDestination(int id, UpdateDestinationDTO destinationDto)
        {
            if (id != destinationDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var destination = new Destination
            {
                Id = destinationDto.Id,
                Name = destinationDto.Name,
                Description = destinationDto.Description,
                Country = destinationDto.Country,
                City = destinationDto.City,
                ImageUrl = destinationDto.ImageUrl
            };

            var updatedDestination = await _destinationService.UpdateDestinationAsync(id, destination);
            if (updatedDestination == null)
                return NotFound();

            return Ok(MapDestinationToDto(updatedDestination));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDestination(int id)
        {
            var result = await _destinationService.DeleteDestinationAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateDestinationImage(int id, [FromBody] UpdateDestinationImageDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get the destination first
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null)
                return NotFound();

            // Update the image URL
            destination.ImageUrl = model.ImageUrl;

            // Save the changes
            var result = await _destinationService.UpdateDestinationAsync(id, destination);
            if (result == null)
                return BadRequest("Failed to update destination image");

            return NoContent();
        }

        // Helper method to map Destination entity to DestinationDTO
        private DestinationDTO MapDestinationToDto(Destination destination)
        {
            return new DestinationDTO
            {
                Id = destination.Id,
                Name = destination.Name,
                Description = destination.Description ?? string.Empty,
                Country = destination.Country,
                City = destination.City,
                ImageUrl = destination.ImageUrl
            };
        }
    }

    // DTO for updating destination image
    public class UpdateDestinationImageDTO
    {
        public string? ImageUrl { get; set; }
    }
}
