using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Data;  
using Service.DTOs;
using Service.Interfaces;
using Data.Entities;

namespace TrialFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestController : ControllerBase
    {
        private readonly IGuestService _guestService;

        public GuestController(IGuestService guestService)
        {
            _guestService = guestService ?? throw new ArgumentNullException(nameof(guestService));
        }

        // GET: api/Guest
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<GuestDTO>> GetGuests()
        {
            var guests = _guestService.GetAllGuests();
            var guestDTOs = new List<GuestDTO>();

            foreach (var guest in guests)
            {
                guestDTOs.Add(new GuestDTO
                {
                    Id = guest.Id,
                    FirstName = guest.FirstName,
                    LastName = guest.LastName,
                    DOB = guest.DOB,
                    Address = guest.Address,
                    Nationality = guest.Nationality,
                    CheckInDate = guest.CheckInDate,
                    CheckOutDate = guest.CheckOutDate,
                    RoomId = guest.RoomId
                });
            }

            return Ok(guestDTOs);
        }

        // GET: api/Guest/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<GuestDTO> GetGuest(int id)
        {
            var guest = _guestService.GetGuestById(id);

            if (guest == null)
            {
                return NotFound();
            }

            var guestDTO = new GuestDTO
            {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                DOB = guest.DOB,
                Address = guest.Address,
                Nationality = guest.Nationality,
                CheckInDate = guest.CheckInDate,
                CheckOutDate = guest.CheckOutDate,
                RoomId = guest.RoomId
            };

            return Ok(guestDTO);
        }

        // POST: api/Guest
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<GuestDTO> CreateGuest(GuestDTO guestDTO)
        {
            if (guestDTO == null)
            {
                return BadRequest();
            }

            var guest = new Guest
            {
                FirstName = guestDTO.FirstName,
                LastName = guestDTO.LastName,
                DOB = guestDTO.DOB,
                Address = guestDTO.Address,
                Nationality = guestDTO.Nationality,
                CheckInDate = guestDTO.CheckInDate,
                CheckOutDate = guestDTO.CheckOutDate,
                RoomId = guestDTO.RoomId
            };

            try
            {
                _guestService.CreateGuest(guest);

                // Update the DTO with the generated ID
                guestDTO.Id = guest.Id;

                return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, guestDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Guest/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateGuest(int id, GuestDTO guestDTO)
        {
            if (guestDTO == null || id != guestDTO.Id)
            {
                return BadRequest();
            }

            var existingGuest = _guestService.GetGuestById(id);
            if (existingGuest == null)
            {
                return NotFound();
            }

            var guest = new Guest
            {
                Id = guestDTO.Id,
                FirstName = guestDTO.FirstName,
                LastName = guestDTO.LastName,
                DOB = guestDTO.DOB,
                Address = guestDTO.Address,
                Nationality = guestDTO.Nationality,
                CheckInDate = guestDTO.CheckInDate,
                CheckOutDate = guestDTO.CheckOutDate,
                RoomId = guestDTO.RoomId
            };

            try
            {
                _guestService.UpdateGuest(guest);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Guest/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteGuest(int id)
        {
            var guest = _guestService.GetGuestById(id);
            if (guest == null)
            {
                return NotFound();
            }

            try
            {
                _guestService.DeleteGuest(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}