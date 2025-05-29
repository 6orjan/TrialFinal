// Create this file: Data/Repositories/Interfaces/IRoomRepository.cs
using System;
using System.Collections.Generic;
using Data.Entities;

namespace Data.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        IEnumerable<Room> GetAll();
        Room GetById(int id);
        void Add(Room entity);
        void Update(Room entity);
        void Delete(Room entity);
        void Delete(int id);
        void SaveChanges();

        // Room-specific methods
        IEnumerable<Room> GetRoomsByFloor(int floor);
        IEnumerable<Room> GetAvailableRooms(DateTime startDate, DateTime endDate);
        bool IsRoomAvailable(int roomId, DateTime startDate, DateTime endDate);
    }
}