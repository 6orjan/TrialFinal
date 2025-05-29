using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs
{
    public class GuestDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "First name cannot exceed 200 characters")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(400, ErrorMessage = "Last name cannot exceed 400 characters")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        [StringLength(600, ErrorMessage = "Address cannot exceed 600 characters")]
        public string Address { get; set; }

        [Required]
        public string Nationality { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public int RoomId { get; set; }

        // Additional useful properties for DTOs
        public string FullName => $"{FirstName} {LastName}";
        public int Age => CalculateAge(DOB);
        public int StayDuration => (CheckOutDate - CheckInDate).Days;

        // Helper method to calculate age
        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}