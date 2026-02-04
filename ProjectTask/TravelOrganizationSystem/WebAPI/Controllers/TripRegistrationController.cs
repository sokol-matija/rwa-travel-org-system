using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    [Authorize]
    public class TripRegistrationController : ControllerBase
    {
        private readonly ITripRegistrationService _registrationService;

        public TripRegistrationController(ITripRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TripRegistrationDTO>>> GetAllRegistrations()
        {
            var registrations = await _registrationService.GetAllRegistrationsAsync();
            var dtos = registrations.Select(MapRegistrationToDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripRegistrationDTO>> GetRegistration(int id)
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id);
            if (registration == null)
                return NotFound();

            // Check if the user is authorized to view this registration
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Admin") && registration.UserId != userId)
                return Forbid();

            return Ok(MapRegistrationToDto(registration));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TripRegistrationDTO>>> GetRegistrationsByUser(int userId)
        {
            // Check if the user is authorized to view these registrations
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Admin") && userId != currentUserId)
                return Forbid();

            var registrations = await _registrationService.GetRegistrationsByUserAsync(userId);
            var dtos = registrations.Select(MapRegistrationToDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("trip/{tripId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TripRegistrationDTO>>> GetRegistrationsByTrip(int tripId)
        {
            var registrations = await _registrationService.GetRegistrationsByTripAsync(tripId);
            var dtos = registrations.Select(MapRegistrationToDto).ToList();
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<TripRegistrationDTO>> CreateRegistration(CreateTripRegistrationDTO registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Create a new TripRegistration entity from the DTO
            var registration = new TripRegistration
            {
                TripId = registrationDto.TripId,
                NumberOfParticipants = registrationDto.NumberOfParticipants,
                RegistrationDate = DateTime.Now,
                Status = "Pending" // Default status for new registrations
            };

            // Set the user ID to the current user if not specified and not admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (registrationDto.UserId.HasValue && User.IsInRole("Admin"))
            {
                registration.UserId = registrationDto.UserId.Value;
            }
            else
            {
                registration.UserId = currentUserId;
            }

            var createdRegistration = await _registrationService.CreateRegistrationAsync(registration);
            if (createdRegistration == null)
                return BadRequest("Unable to create registration. The trip may be full or not exist.");

            return CreatedAtAction(nameof(GetRegistration), new { id = createdRegistration.Id }, MapRegistrationToDto(createdRegistration));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TripRegistrationDTO>> UpdateRegistration(int id, UpdateTripRegistrationDTO registrationDto)
        {
            if (id != registrationDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if the user is authorized to update this registration
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var existingRegistration = await _registrationService.GetRegistrationByIdAsync(id);
            if (existingRegistration == null)
                return NotFound();

            if (!User.IsInRole("Admin") && existingRegistration.UserId != currentUserId)
                return Forbid();

            // Create a TripRegistration entity from the DTO
            var registration = new TripRegistration
            {
                Id = registrationDto.Id,
                UserId = existingRegistration.UserId,
                TripId = existingRegistration.TripId,
                RegistrationDate = existingRegistration.RegistrationDate,
                NumberOfParticipants = registrationDto.NumberOfParticipants,
                Status = registrationDto.Status
            };

            var updatedRegistration = await _registrationService.UpdateRegistrationAsync(id, registration);
            if (updatedRegistration == null)
                return BadRequest("Unable to update registration. The trip may be full.");

            return Ok(MapRegistrationToDto(updatedRegistration));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRegistration(int id)
        {
            // Check if the user is authorized to delete this registration
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var registration = await _registrationService.GetRegistrationByIdAsync(id);
            if (registration == null)
                return NotFound();

            if (!User.IsInRole("Admin") && registration.UserId != currentUserId)
                return Forbid();

            var result = await _registrationService.DeleteRegistrationAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRegistrationStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
                return BadRequest("Status cannot be empty");

            var result = await _registrationService.UpdateRegistrationStatusAsync(id, status);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Helper method to map TripRegistration entity to TripRegistrationDTO
        private TripRegistrationDTO MapRegistrationToDto(TripRegistration registration)
        {
            return new TripRegistrationDTO
            {
                Id = registration.Id,
                UserId = registration.UserId,
                Username = registration.User?.Username ?? string.Empty,
                TripId = registration.TripId,
                TripName = registration.Trip?.Name ?? string.Empty,
                DestinationName = registration.Trip?.Destination?.Name ?? string.Empty,
                StartDate = registration.Trip?.StartDate ?? DateTime.MinValue,
                EndDate = registration.Trip?.EndDate ?? DateTime.MinValue,
                RegistrationDate = registration.RegistrationDate,
                NumberOfParticipants = registration.NumberOfParticipants,
                TotalPrice = registration.TotalPrice,
                Status = registration.Status ?? string.Empty
            };
        }
    }
}
