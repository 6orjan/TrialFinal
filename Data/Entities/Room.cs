using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public int Floor { get; set; }

        [Required]
        public string Type { get; set; }

        // Navigation property for the one-to-many relationship
        // One room can have multiple guests
        public virtual ICollection<Guest> Guests { get; set; }

        public Room()
        {
            // Initialize the collection in constructor
            Guests = new HashSet<Guest>();
        }
    }
}