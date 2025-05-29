using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interfaces;

namespace TrialFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IGuestService _guestService;

        public RoomController(IRoomService roomService, IGuestService guestService)
        {
            _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
            _guestService = guestService ?? throw new ArgumentNullException(nameof(guestService));
        }

        // GET: api/Room
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<RoomDTO>> GetRooms()
        {
            var rooms = _roomService.GetAllRooms();
            var allGuests = _guestService.GetAllGuests();
            var roomDTOs = new List<RoomDTO>();

            foreach (var room in rooms)
            {
                var currentGuests = allGuests.Where(g => g.RoomId == room.Id && g.CheckOutDate >= DateTime.Today).ToList();

                var roomDTO = new RoomDTO
                {
                    Id = room.Id,
                    Number = room.Number,
                    Floor = room.Floor,
                    Type = room.Type,
                    IsOccupied = currentGuests.Any(),
                    GuestCount = currentGuests.Count(),
                    CurrentGuests = currentGuests.Select(g => new GuestDTO
                    {
                        Id = g.Id,
                        FirstName = g.FirstName,
                        LastName = g.LastName,
                        DOB = g.DOB,
                        Address = g.Address,
                        Nationality = g.Nationality,
                        CheckInDate = g.CheckInDate,
                        CheckOutDate = g.CheckOutDate,
                        RoomId = g.RoomId
                    })
                };

                roomDTOs.Add(roomDTO);
            }

            return Ok(roomDTOs);
        }

        // GET: api/Room/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<RoomDTO> GetRoom(int id)
        {
            var room = _roomService.GetRoomById(id);

            if (room == null)
            {
                return NotFound();
            }

            var allGuests = _guestService.GetAllGuests();
            var currentGuests = allGuests.Where(g => g.RoomId == room.Id && g.CheckOutDate >= DateTime.Today).ToList();

            var roomDTO = new RoomDTO
            {
                Id = room.Id,
                Number = room.Number,
                Floor = room.Floor,
                Type = room.Type,
                IsOccupied = currentGuests.Any(),
                GuestCount = currentGuests.Count(),
                CurrentGuests = currentGuests.Select(g => new GuestDTO
                {
                    Id = g.Id,
                    FirstName = g.FirstName,
                    LastName = g.LastName,
                    DOB = g.DOB,
                    Address = g.Address,
                    Nationality = g.Nationality,
                    CheckInDate = g.CheckInDate,
                    CheckOutDate = g.CheckOutDate,
                    RoomId = g.RoomId
                })
            };

            return Ok(roomDTO);
        }

        // POST: api/Room
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RoomDTO> CreateRoom(RoomDTO roomDTO)
        {
            if (roomDTO == null)
            {
                return BadRequest();
            }

            var room = new Room
            {
                Number = roomDTO.Number,
                Floor = roomDTO.Floor,
                Type = roomDTO.Type
            };

            try
            {
                _roomService.CreateRoom(room);

                // Update the DTO with the generated ID
                roomDTO.Id = room.Id;
                roomDTO.IsOccupied = false;
                roomDTO.GuestCount = 0;
                roomDTO.CurrentGuests = new List<GuestDTO>();

                return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, roomDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Room/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateRoom(int id, RoomDTO roomDTO)
        {
            if (roomDTO == null || id != roomDTO.Id)
            {
                return BadRequest();
            }

            var existingRoom = _roomService.GetRoomById(id);
            if (existingRoom == null)
            {
                return NotFound();
            }

            var room = new Room
            {
                Id = roomDTO.Id,
                Number = roomDTO.Number,
                Floor = roomDTO.Floor,
                Type = roomDTO.Type
            };

            try
            {
                _roomService.UpdateRoom(room);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Room/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteRoom(int id)
        {
            var room = _roomService.GetRoomById(id);
            if (room == null)
            {
                return NotFound();
            }

            try
            {
                _roomService.DeleteRoom(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}