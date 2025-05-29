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
    public class GuestService : IGuestService
    {
        private readonly AppDbContext _context;

        public GuestService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Guest> GetAllGuests()
        {
            return _context.Guests.ToList();
        }

        public Guest GetGuestById(int id)
        {
            return _context.Guests.FirstOrDefault(g => g.Id == id);
        }

        public void CreateGuest(Guest guest)
        {
            if (guest == null)
                throw new ArgumentNullException(nameof(guest));

            _context.Guests.Add(guest);
            _context.SaveChanges();
        }

        public void UpdateGuest(Guest guest)
        {
            if (guest == null)
                throw new ArgumentNullException(nameof(guest));

            var existingGuest = _context.Guests.FirstOrDefault(g => g.Id == guest.Id);
            if (existingGuest == null)
                throw new InvalidOperationException($"Guest with ID {guest.Id} not found.");

            // Update properties
            existingGuest.FirstName = guest.FirstName;
            existingGuest.LastName = guest.LastName;
            existingGuest.DOB = guest.DOB;
            existingGuest.Address = guest.Address;
            existingGuest.Nationality = guest.Nationality;
            existingGuest.CheckInDate = guest.CheckInDate;
            existingGuest.CheckOutDate = guest.CheckOutDate;
            existingGuest.RoomId = guest.RoomId;

            _context.SaveChanges();
        }

        public void DeleteGuest(int id)
        {
            var guest = _context.Guests.FirstOrDefault(g => g.Id == id);
            if (guest == null)
                throw new InvalidOperationException($"Guest with ID {id} not found.");

            _context.Guests.Remove(guest);
            _context.SaveChanges();
        }
    }
}