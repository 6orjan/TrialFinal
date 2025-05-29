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
    public class GuestRepository : IGuestRepository
    {
        private readonly AppDbContext _context;

        public GuestRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Guest> GetAll()
        {
            return _context.Guests
                .Include(g => g.Room)
                .ToList();
        }

        public Guest GetById(int id)
        {
            return _context.Guests
                .Include(g => g.Room)
                .FirstOrDefault(g => g.Id == id);
        }

        public void Add(Guest entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Guests.Add(entity);
        }

        public void Update(Guest entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // No need to attach if being tracked
            if (!_context.Guests.Local.Any(g => g.Id == entity.Id))
            {
                _context.Guests.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public void Delete(Guest entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Guests.Attach(entity);
            }

            _context.Guests.Remove(entity);
        }

        public void Delete(int id)
        {
            var guest = GetById(id);
            if (guest != null)
            {
                Delete(guest);
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        // Guest-specific methods
        public IEnumerable<Guest> GetGuestsByRoomId(int roomId)
        {
            return _context.Guests
                .Where(g => g.RoomId == roomId)
                .ToList();
        }

        public IEnumerable<Guest> GetCurrentGuests()
        {
            var today = DateTime.Today;
            return _context.Guests
                .Where(g => g.CheckInDate <= today && g.CheckOutDate >= today)
                .Include(g => g.Room)
                .ToList();
        }

        public bool RoomHasGuests(int roomId)
        {
            return _context.Guests.Any(g => g.RoomId == roomId);
        }
    }
}