using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using Service.Services;

namespace Service.Tests
{
    [TestClass]
    public sealed class GuestServiceTests
    {
        private AppDbContext _context;
        private GuestService _guestService;
        private string _dbName;
        private string _connectionString;
        private const string PostgresUsername = "postgres";
        private const string PostgresPassword = "0000";
        private const string PostgresHost = "localhost";

        [TestInitialize]
        public void Initialize()
        {
            // Create a unique database name for each test run to prevent conflicts
            _dbName = $"guest_test_{Guid.NewGuid().ToString().Replace("-", "_")}";
            _connectionString = $"Host={PostgresHost};Database={_dbName};Username={PostgresUsername};Password={PostgresPassword}";

            // Enable legacy timestamp behavior for PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            _context = new AppDbContext(options);

            // Ensure database is created
            _context.Database.EnsureCreated();

            // Create service instance
            _guestService = new GuestService(_context);

            // Seed initial data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add test rooms first (since guests need valid room IDs)
            var rooms = new List<Room>
            {
                new Room { Number = "101", Floor = 1, Type = "Standard" },
                new Room { Number = "201", Floor = 2, Type = "Deluxe" },
                new Room { Number = "301", Floor = 3, Type = "Suite" }
            };

            _context.Rooms.AddRange(rooms);
            _context.SaveChanges();

            // Get the room IDs that were generated
            var roomIds = _context.Rooms.Select(r => r.Id).ToList();

            // Add guests
            var guests = new List<Guest>
            {
                new Guest {
                    FirstName = "John",
                    LastName = "Doe",
                    DOB = DateTime.UtcNow.AddYears(-30),
                    Address = "123 Main St",
                    Nationality = "US",
                    CheckInDate = DateTime.UtcNow,
                    CheckOutDate = DateTime.UtcNow.AddDays(5),
                    RoomId = roomIds[0]
                },
                new Guest {
                    FirstName = "Jane",
                    LastName = "Smith",
                    DOB = DateTime.UtcNow.AddYears(-25),
                    Address = "456 Oak St",
                    Nationality = "UK",
                    CheckInDate = DateTime.UtcNow,
                    CheckOutDate = DateTime.UtcNow.AddDays(3),
                    RoomId = roomIds[1]
                }
            };

            _context.Guests.AddRange(guests);
            _context.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Dispose the context
            _context.Dispose();

            try
            {
                // Connect to the master database to drop the test database
                using (var masterConnection = new NpgsqlConnection(
                    $"Host={PostgresHost};Database=postgres;Username={PostgresUsername};Password={PostgresPassword}"))
                {
                    masterConnection.Open();

                    // Terminate all connections to the database
                    using (var terminateCommand = masterConnection.CreateCommand())
                    {
                        terminateCommand.CommandText = $@"
                            SELECT pg_terminate_backend(pg_stat_activity.pid)
                            FROM pg_stat_activity
                            WHERE pg_stat_activity.datname = '{_dbName}'
                            AND pid <> pg_backend_pid();";
                        terminateCommand.ExecuteNonQuery();
                    }

                    // Drop the database
                    using (var command = masterConnection.CreateCommand())
                    {
                        command.CommandText = $"DROP DATABASE IF EXISTS \"{_dbName}\";";
                        command.ExecuteNonQuery();
                    }

                    masterConnection.Close();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the test
                Console.WriteLine($"Error cleaning up test database: {ex.Message}");
            }
        }

        [TestMethod]
        public void GetAllGuests_ShouldReturnAllGuests()
        {
            // Act
            var result = _guestService.GetAllGuests();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(g => g.FirstName == "John" && g.LastName == "Doe"));
            Assert.IsTrue(result.Any(g => g.FirstName == "Jane" && g.LastName == "Smith"));
        }

        [TestMethod]
        public void GetGuestById_WithValidId_ReturnsGuest()
        {
            // Arrange
            var guest = _context.Guests.First(g => g.FirstName == "John");

            // Act
            var result = _guestService.GetGuestById(guest.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John", result.FirstName);
            Assert.AreEqual("Doe", result.LastName);
            Assert.AreEqual("US", result.Nationality);
        }

        [TestMethod]
        public void GetGuestById_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _guestService.GetGuestById(999);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void CreateGuest_ShouldAddNewGuest()
        {
            // Arrange
            var roomId = _context.Rooms.First(r => r.Number == "301").Id;
            var guest = new Guest
            {
                FirstName = "Bob",
                LastName = "Johnson",
                DOB = DateTime.UtcNow.AddYears(-40),
                Address = "789 Pine St",
                Nationality = "CA",
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(7),
                RoomId = roomId
            };

            // Act
            _guestService.CreateGuest(guest);

            // Assert
            var savedGuest = _context.Guests.FirstOrDefault(g => g.FirstName == "Bob" && g.LastName == "Johnson");
            Assert.IsNotNull(savedGuest);
            Assert.AreEqual("789 Pine St", savedGuest.Address);
            Assert.AreEqual("CA", savedGuest.Nationality);
            Assert.AreEqual(roomId, savedGuest.RoomId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateGuest_WithNullGuest_ThrowsArgumentNullException()
        {
            // Act - This should throw ArgumentNullException
            _guestService.CreateGuest(null);
        }

        [TestMethod]
        public void UpdateGuest_ShouldModifyExistingGuest()
        {
            // Arrange
            var guest = _context.Guests.First(g => g.FirstName == "Jane");
            var roomId = _context.Rooms.First(r => r.Number == "301").Id;

            var guestToUpdate = new Guest
            {
                Id = guest.Id,
                FirstName = "Jane",
                LastName = "Green", // Changed last name
                DOB = guest.DOB,
                Address = "555 Maple Ave", // Changed address
                Nationality = "AU", // Changed nationality
                CheckInDate = guest.CheckInDate,
                CheckOutDate = DateTime.UtcNow.AddDays(10), // Extended stay
                RoomId = roomId // Changed room
            };

            // Act
            _guestService.UpdateGuest(guestToUpdate);

            // Assert - Reload from database to ensure changes were saved
            _context.Entry(guest).Reload();
            var updatedGuest = _context.Guests.Find(guest.Id);

            Assert.AreEqual("Green", updatedGuest.LastName);
            Assert.AreEqual("555 Maple Ave", updatedGuest.Address);
            Assert.AreEqual("AU", updatedGuest.Nationality);
            Assert.AreEqual(roomId, updatedGuest.RoomId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateGuest_WithNullGuest_ThrowsArgumentNullException()
        {
            // Act - This should throw ArgumentNullException
            _guestService.UpdateGuest(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateGuest_WithInvalidId_ThrowsInvalidOperationException()
        {
            // Arrange
            var guest = new Guest
            {
                Id = 999, // Non-existent ID
                FirstName = "Invalid",
                LastName = "Guest",
                DOB = DateTime.UtcNow.AddYears(-30),
                Address = "Invalid Address",
                Nationality = "XX",
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(5),
                RoomId = 1
            };

            // Act - This should throw InvalidOperationException
            _guestService.UpdateGuest(guest);
        }

        [TestMethod]
        public void DeleteGuest_WithValidId_DeletesGuest()
        {
            // Arrange
            var guest = _context.Guests.First(g => g.FirstName == "John");

            // Act
            _guestService.DeleteGuest(guest.Id);

            // Assert
            var deletedGuest = _context.Guests.Find(guest.Id);
            Assert.IsNull(deletedGuest);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteGuest_WithInvalidId_ThrowsInvalidOperationException()
        {
            // Act - This should throw InvalidOperationException
            _guestService.DeleteGuest(999);
        }
    }
}