using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Data.Entities;
using Service.Interfaces;

namespace Service.Services
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Room> GetAllRooms()
        {
            return _context.Rooms.ToList();
        }

        public Room GetRoomById(int id)
        {
            return _context.Rooms.FirstOrDefault(r => r.Id == id);
        }

        public void CreateRoom(Room room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            _context.Rooms.Add(room);
            _context.SaveChanges();
        }

        public void UpdateRoom(Room room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var existingRoom = _context.Rooms.FirstOrDefault(r => r.Id == room.Id);
            if (existingRoom == null)
                throw new InvalidOperationException($"Room with ID {room.Id} not found.");

            // Update properties
            existingRoom.Number = room.Number;
            existingRoom.Floor = room.Floor;
            existingRoom.Type = room.Type;

            _context.SaveChanges();
        }

        public void DeleteRoom(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
                throw new InvalidOperationException($"Room with ID {id} not found.");

            // Check if room has guests
            var hasGuests = _context.Guests.Any(g => g.RoomId == id);
            if (hasGuests)
                throw new InvalidOperationException($"Cannot delete room with ID {id} because it has associated guests.");

            _context.Rooms.Remove(room);
            _context.SaveChanges();
        }
    }
}