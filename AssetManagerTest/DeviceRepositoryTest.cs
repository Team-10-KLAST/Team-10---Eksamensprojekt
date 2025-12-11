using Application.Models;
using Data;
using Data.AdoNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AssetManagerTest;

[TestClass]
public class DeviceRepositoryTest
{
    // Repository instance to be tested
    private DeviceRepository _repository = null!;

    // Arrange: Initialize the repository before each test
    [TestInitialize]
    public void Setup()
    {
        var database = DatabaseConnection.GetInstance();
        _repository = new DeviceRepository(database);
    }

    // Test to verify that GetAll method returns all devices
    [TestMethod]
    public void GetAll_ShouldReturnAllDevices()
    {
        // Arrange (handled in Setup)

        // Act
        var result = _repository.GetAll().ToList();

        // Assert
        Assert.IsTrue(result.Count > 0, "Expected at least one device in the database.");

        foreach (var device in result)
        {
            Assert.IsTrue(device.DeviceID > 0, "DeviceID must be greater than zero.");
            Assert.IsTrue(device.DeviceDescriptionID > 0, "DeviceDescriptionID must be greater than zero.");
            Assert.IsTrue(Enum.IsDefined(typeof(DeviceStatus), device.Status),
                "DeviceStatus must be a valid enum value.");
            if (device.PurchaseDate is not null)
            {
                Assert.IsTrue(device.PurchaseDate.Value.Year > 2000,
                    "PurchaseDate must be a realistic date (after year 2000).");
            }

            if (device.ExpectedEndDate is not null && device.PurchaseDate is not null)
            {
                Assert.IsTrue(device.ExpectedEndDate >= device.PurchaseDate,
                    "ExpectedEndDate must be after or equal to PurchaseDate.");
            }
        }
    }

    // Test to verify that GetByID method returns the correct device when the ID exists
    [TestMethod]
    public void GetByID_ShouldReturnDevice_WhenExists()
    {
        // Arrange
        var all = _repository.GetAll().ToList();
        var id = all.First().DeviceID;

        // Act
        var device = _repository.GetByID(id);

        // Assert
        Assert.IsNotNull(device);
        Assert.AreEqual(id, device!.DeviceID);
    }

    // Test to verify that GetByID method returns null when the device does not exist
    [TestMethod]
    public void GetByID_ShouldReturnNull_WhenNotFound()
    {
        // Arrange (handled in Setup)

        // Act
        var device = _repository.GetByID(999999);
        // Assert
        Assert.IsNull(device);
    }

    // Test to verify that GetByID method throws ArgumentOutOfRangeException for invalid IDs
    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetByID_ShouldThrow_WhenIdIsInvalid()
    {
        // Arrange (handled in Setup)

        // Act
        _repository.GetByID(0);

        // Assert handled by ExpectedException
    }

    // Test to verify that Add inserts a new device and returns a generated DeviceID
    [TestMethod]
    public void Add_ShouldInsertDevice_AndReturnNewID()
    {
        // Arrange
        var device = new Device
        {
            DeviceDescriptionID = 1,
            Status = DeviceStatus.INUSE,
            IsWiped = false,
            PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
            ExpectedEndDate = null
        };

        // Act
        _repository.Add(device);

        // Assert
        Assert.IsTrue(device.DeviceID > 0, "DeviceID should be set after Add().");

        var fromDb = _repository.GetByID(device.DeviceID);
        Assert.IsNotNull(fromDb);
        Assert.AreEqual(device.DeviceDescriptionID, fromDb!.DeviceDescriptionID);
        Assert.AreEqual(device.Status, fromDb.Status);
        Assert.AreEqual(device.IsWiped, fromDb.IsWiped);
        Assert.AreEqual(device.PurchaseDate, fromDb.PurchaseDate);
        Assert.AreEqual(device.ExpectedEndDate, fromDb.ExpectedEndDate);
    }

    // Test to verify that Add throws ArgumentNullException when device is null
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Add_ShouldThrow_WhenDeviceIsNull()
    {
        // Arrange (handled in Setup)

        // Act
        _repository.Add(null!);

        // Assert handled by ExpectedException
    }

    // Test to verify that Update modifies an existing device correctly
    [TestMethod]
    public void Update_ShouldModifyExistingDevice()
    {
        // Arrange
        var device = new Device
        {
            DeviceDescriptionID = 1,
            Status = DeviceStatus.INUSE,
            IsWiped = false,
            PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
            ExpectedEndDate = null
        };

        _repository.Add(device);

        var originalId = device.DeviceID;

        device.Status = DeviceStatus.CANCELLED;
        device.IsWiped = true;

        // Act
        _repository.Update(device);

        // Assert
        var updated = _repository.GetByID(originalId);
        Assert.IsNotNull(updated);
        Assert.AreEqual(DeviceStatus.CANCELLED, updated!.Status);
        Assert.AreEqual(true, updated.IsWiped);
    }


    // Test to verify that ExpectedEndDate cannot be cleared if a date already exists
    [TestMethod]
    public void Update_ShouldNotAllowClearingExpectedEndDate()
    {
        // Arrange
        var device = new Device
        {
            DeviceDescriptionID = 1,
            Status = DeviceStatus.INUSE,
            IsWiped = false,
            PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
            ExpectedEndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
        };

        _repository.Add(device);

        var originalEndDate = device.ExpectedEndDate;

        device.ExpectedEndDate = null;

        // Act
        _repository.Update(device);

        // Assert
        var updated = _repository.GetByID(device.DeviceID);
        Assert.AreEqual(originalEndDate, updated!.ExpectedEndDate);
    }

    // Test to verify that Update throws ArgumentOutOfRangeException for invalid ID
    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Update_ShouldThrow_WhenIdIsInvalid()
    {
        // Arrange
        var device = new Device
        {
            DeviceID = 0,
            DeviceDescriptionID = 1,
            Status = DeviceStatus.INUSE,
            IsWiped = true
        };

        // Act
        _repository.Update(device);

        // Assert handled by ExpectedException
    }

    // Test to verify that Update throws KeyNotFoundException when device does not exist
    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void Update_ShouldThrow_WhenDeviceNotFound()
    {
        // Arrange
        var device = new Device
        {
            DeviceID = 999999,
            DeviceDescriptionID = 1,
            Status = DeviceStatus.INUSE,
            IsWiped = true
        };

        // Act
        _repository.Update(device);

        // Assert handled by ExpectedException
    }

    // Test to verify that Delete removes an existing device
    [TestMethod]
    public void Delete_ShouldRemoveDevice()
    {
        // Arrange
        var device = new Device
        {
            DeviceDescriptionID = 1,
            Status = DeviceStatus.INUSE,
            IsWiped = true,
            PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
            ExpectedEndDate = null
        };

        _repository.Add(device);
        var id = device.DeviceID;

        // Act
        _repository.Delete(id);

        // Assert
        var deleted = _repository.GetByID(id);
        Assert.IsNull(deleted, "Device should be deleted and no longer retrievable.");
    }

    // Test to verify that Delete throws ArgumentOutOfRangeException for invalid ID
    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Delete_ShouldThrow_WhenIdIsInvalid()
    {
        // Arrange (handled in Setup)

        // Act
        _repository.Delete(0);

        // Assert handled by ExpectedException
    }

    // Test to verify that Delete throws KeyNotFoundException when device does not exist
    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void Delete_ShouldThrow_WhenDeviceNotFound()
    {
        // Arrange (handled in Setup)

        // Act
        _repository.Delete(999999);

        // Assert handled by ExpectedException
    }
}
