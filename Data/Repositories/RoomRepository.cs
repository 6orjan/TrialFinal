using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Data.Repositories.Interfaces;
using Data.Entities;

namespace Data.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;

        public RoomRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Room> GetAll()
        {
            return _context.Rooms
                .Include(r => r.Guests)
                .ToList();
        }

        public Room GetById(int id)
        {
            return _context.Rooms
                .Include(r => r.Guests)
                .FirstOrDefault(r => r.Id == id);
        }

        public void Add(Room entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Rooms.Add(entity);
        }

        public void Update(Room entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // No need to attach if being tracked
            if (!_context.Rooms.Local.Any(r => r.Id == entity.Id))
            {
                _context.Rooms.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public void Delete(Room entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Rooms.Attach(entity);
            }

            _context.Rooms.Remove(entity);
        }

        public void Delete(int id)
        {
            var room = GetById(id);
            if (room != null)
            {
                Delete(room);
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        // Room-specific methods
        public IEnumerable<Room> GetRoomsByFloor(int floor)
        {
            return _context.Rooms
                .Where(r => r.Floor == floor)
                .Include(r => r.Guests)
                .ToList();
        }

        public IEnumerable<Room> GetAvailableRooms(DateTime startDate, DateTime endDate)
        {
            // Getting all rooms
            var allRooms = _context.Rooms.ToList();

            // Getting rooms with guests during the specified period
            var roomsWithGuests = _context.Guests
                .Where(g => (g.CheckInDate <= endDate && g.CheckOutDate >= startDate))
                .Select(g => g.RoomId)
                .Distinct()
                .ToList();

            // Returning rooms that are not in the list of rooms with guests
            return allRooms
                .Where(r => !roomsWithGuests.Contains(r.Id))
                .ToList();
        }

        public bool IsRoomAvailable(int roomId, DateTime startDate, DateTime endDate)
        {
            return !_context.Guests
                .Any(g => g.RoomId == roomId &&
                          g.CheckInDate <= endDate &&
                          g.CheckOutDate >= startDate);
        }
    }
}