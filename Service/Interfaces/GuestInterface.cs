using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Service.Interfaces
{
    public interface IGuestService
    {
        IEnumerable<Guest> GetAllGuests();
        Guest GetGuestById(int id);
        void CreateGuest(Guest guest);
        void UpdateGuest(Guest guest);
        void DeleteGuest(int id);
    }
}