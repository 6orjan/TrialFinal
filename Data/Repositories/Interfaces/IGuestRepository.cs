// Create this file: Data/Repositories/Interfaces/IGuestRepository.cs
using System;
using System.Collections.Generic;
using Data.Entities;

namespace Data.Repositories.Interfaces
{
    public interface IGuestRepository
    {
        IEnumerable<Guest> GetAll();
        Guest GetById(int id);
        void Add(Guest entity);
        void Update(Guest entity);
        void Delete(Guest entity);
        void Delete(int id);
        void SaveChanges();

        // Guest-specific methods
        IEnumerable<Guest> GetGuestsByRoomId(int roomId);
        IEnumerable<Guest> GetCurrentGuests();
        bool RoomHasGuests(int roomId);
    }
}