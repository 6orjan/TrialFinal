using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs
{
    public class RoomDTO
    {
        public int Id { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public int Floor { get; set; }

        [Required]
        public string Type { get; set; }

        // Additional useful properties for DTOs
        public bool IsOccupied { get; set; }
        public int GuestCount { get; set; }
        public IEnumerable<GuestDTO> CurrentGuests { get; set; }

        public RoomDTO()
        {
            CurrentGuests = new List<GuestDTO>();
        }

        // Helper method to format room information
        public string GetRoomInfo()
        {
            return $"Room {Number} on Floor {Floor} ({Type})";
        }
    }
}