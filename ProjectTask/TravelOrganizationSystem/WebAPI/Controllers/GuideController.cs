using System.Collections.Generic;
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
    public class GuideController : ControllerBase
    {
        private readonly IGuideService _guideService;

        public GuideController(IGuideService guideService)
        {
            _guideService = guideService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Guide>>> GetAllGuides()
        {
            var guides = await _guideService.GetAllGuidesAsync();
            return Ok(guides);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Guide>> GetGuide(int id)
        {
            var guide = await _guideService.GetGuideByIdAsync(id);
            if (guide == null)
                return NotFound();

            return Ok(guide);
        }

        [HttpGet("trip/{tripId}")]
        public async Task<ActionResult<IEnumerable<Guide>>> GetGuidesByTrip(int tripId)
        {
            var guides = await _guideService.GetGuidesByTripAsync(tripId);
            return Ok(guides);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guide>> CreateGuide(CreateGuideDTO createGuideDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var guide = new Guide
            {
                Name = createGuideDto.Name,
                Bio = createGuideDto.Bio,
                Email = createGuideDto.Email,
                Phone = createGuideDto.Phone,
                ImageUrl = createGuideDto.ImageUrl,
                YearsOfExperience = createGuideDto.YearsOfExperience,
                TripGuides = new List<TripGuide>() // Initialize empty collection
            };

            var createdGuide = await _guideService.CreateGuideAsync(guide);
            return CreatedAtAction(nameof(GetGuide), new { id = createdGuide.Id }, createdGuide);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guide>> UpdateGuide(int id, UpdateGuideDTO updateGuideDto)
        {
            if (id != updateGuideDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var guide = new Guide
            {
                Id = updateGuideDto.Id,
                Name = updateGuideDto.Name,
                Bio = updateGuideDto.Bio,
                Email = updateGuideDto.Email,
                Phone = updateGuideDto.Phone,
                ImageUrl = updateGuideDto.ImageUrl,
                YearsOfExperience = updateGuideDto.YearsOfExperience
            };

            var updatedGuide = await _guideService.UpdateGuideAsync(id, guide);
            if (updatedGuide == null)
                return NotFound();

            return Ok(updatedGuide);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteGuide(int id)
        {
            var result = await _guideService.DeleteGuideAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
