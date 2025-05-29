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
    public sealed class RoomServiceTests
    {
        private AppDbContext _context;
        private RoomService _roomService;
        private string _dbName;
        private string _connectionString;
        private const string PostgresUsername = "postgres";
        private const string PostgresPassword = "0000";
        private const string PostgresHost = "localhost";

        [TestInitialize]
        public void Initialize()
        {
            // Create a unique database name for each test run to prevent conflicts
            _dbName = $"room_test_{Guid.NewGuid().ToString().Replace("-", "_")}";
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
            _roomService = new RoomService(_context);

            // Seed initial data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add test rooms
            var rooms = new List<Room>
            {
                new Room { Number = "101", Floor = 1, Type = "Standard" },
                new Room { Number = "201", Floor = 2, Type = "Deluxe" },
                new Room { Number = "301", Floor = 3, Type = "Suite" }
            };

            _context.Rooms.AddRange(rooms);
            _context.SaveChanges();

            // For testing room deletion with guests, add a guest to one of the rooms
            var roomWithGuests = _context.Rooms.First(r => r.Number == "101");

            var guest = new Guest
            {
                FirstName = "John",
                LastName = "Doe",
                DOB = DateTime.UtcNow.AddYears(-30),
                Address = "123 Main St",
                Nationality = "US",
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(5),
                RoomId = roomWithGuests.Id
            };

            _context.Guests.Add(guest);
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
        public void GetAllRooms_ShouldReturnAllRooms()
        {
            // Act
            var result = _roomService.GetAllRooms();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Any(r => r.Number == "101" && r.Floor == 1 && r.Type == "Standard"));
            Assert.IsTrue(result.Any(r => r.Number == "201" && r.Floor == 2 && r.Type == "Deluxe"));
            Assert.IsTrue(result.Any(r => r.Number == "301" && r.Floor == 3 && r.Type == "Suite"));
        }

        [TestMethod]
        public void GetRoomById_WithValidId_ReturnsRoom()
        {
            // Arrange
            var room = _context.Rooms.First(r => r.Number == "201");

            // Act
            var result = _roomService.GetRoomById(room.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("201", result.Number);
            Assert.AreEqual(2, result.Floor);
            Assert.AreEqual("Deluxe", result.Type);
        }

        [TestMethod]
        public void GetRoomById_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _roomService.GetRoomById(999);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void CreateRoom_ShouldAddNewRoom()
        {
            // Arrange
            var room = new Room
            {
                Number = "401",
                Floor = 4,
                Type = "Presidential"
            };

            // Act
            _roomService.CreateRoom(room);

            // Assert
            var savedRoom = _context.Rooms.FirstOrDefault(r => r.Number == "401");
            Assert.IsNotNull(savedRoom);
            Assert.AreEqual(4, savedRoom.Floor);
            Assert.AreEqual("Presidential", savedRoom.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateRoom_WithNullRoom_ThrowsArgumentNullException()
        {
            // Act - This should throw ArgumentNullException
            _roomService.CreateRoom(null);
        }

        [TestMethod]
        public void UpdateRoom_ShouldModifyExistingRoom()
        {
            // Arrange
            var room = _context.Rooms.First(r => r.Number == "201");

            var roomToUpdate = new Room
            {
                Id = room.Id,
                Number = "202", // Changed number
                Floor = 2,
                Type = "Executive" // Changed type
            };

            // Act
            _roomService.UpdateRoom(roomToUpdate);

            // Assert - Reload from database to ensure changes were saved
            _context.Entry(room).Reload();
            var updatedRoom = _context.Rooms.Find(room.Id);

            Assert.AreEqual("202", updatedRoom.Number);
            Assert.AreEqual(2, updatedRoom.Floor);
            Assert.AreEqual("Executive", updatedRoom.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateRoom_WithNullRoom_ThrowsArgumentNullException()
        {
            // Act - This should throw ArgumentNullException
            _roomService.UpdateRoom(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateRoom_WithInvalidId_ThrowsInvalidOperationException()
        {
            // Arrange
            var room = new Room
            {
                Id = 999, // Non-existent ID
                Number = "999",
                Floor = 9,
                Type = "Invalid"
            };

            // Act - This should throw InvalidOperationException
            _roomService.UpdateRoom(room);
        }

        [TestMethod]
        public void DeleteRoom_WithValidIdAndNoGuests_DeletesRoom()
        {
            // Arrange - Get a room without guests
            var room = _context.Rooms.First(r => r.Number == "301");

            // Act
            _roomService.DeleteRoom(room.Id);

            // Assert
            var deletedRoom = _context.Rooms.Find(room.Id);
            Assert.IsNull(deletedRoom);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteRoom_WithInvalidId_ThrowsInvalidOperationException()
        {
            // Act - This should throw InvalidOperationException
            _roomService.DeleteRoom(999);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteRoom_WithGuests_ThrowsInvalidOperationException()
        {
            // Arrange - Get a room with guests (room 101 has a guest per our seed data)
            var roomWithGuests = _context.Rooms.First(r => r.Number == "101");

            // Act - This should throw InvalidOperationException
            _roomService.DeleteRoom(roomWithGuests.Id);
        }

        [TestMethod]
        public void DeleteRoom_VerifyRoomWithGuestsCheckWorks()
        {
            // Arrange - Get a room with guests (room 101)
            var roomWithGuests = _context.Rooms.First(r => r.Number == "101");

            // Verify the room has guests
            bool hasGuests = _context.Guests.Any(g => g.RoomId == roomWithGuests.Id);
            Assert.IsTrue(hasGuests, "Test setup issue: Room should have guests");

            try
            {
                // Act - Try to delete the room
                _roomService.DeleteRoom(roomWithGuests.Id);

                // If we get here, the test has failed
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                // Assert - Check that the exception message mentions guests
                StringAssert.Contains(ex.Message, "has associated guests");
            }
        }
    }
}